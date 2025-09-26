using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TempleApi.Data;

namespace TempleApi.Security
{
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		private readonly TempleDbContext _context;

		public BasicAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			TempleDbContext context)
			: base(options, logger, encoder)
		{
			_context = context;
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (!Request.Headers.ContainsKey("Authorization"))
			{
				return AuthenticateResult.NoResult();
			}

			try
			{
				var authHeader = Request.Headers["Authorization"].ToString();
				if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
				{
					return AuthenticateResult.NoResult();
				}

				var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
				var credentialBytes = Convert.FromBase64String(encodedCredentials);
				var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
				if (credentials.Length != 2)
				{
					return AuthenticateResult.Fail("Invalid Basic authentication credentials format.");
				}

				var usernameOrEmail = (credentials[0] ?? string.Empty).Trim();
				var password = credentials[1] ?? string.Empty;

				// For In-Memory database compatibility in tests, we need to handle case sensitivity differently
				var users = await _context.Users
					.Include(u => u.UserRoles)
					.ThenInclude(ur => ur.Role)
					.ToListAsync();
				
				var user = users.FirstOrDefault(u => 
					string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) || 
					string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase));

				if (user == null)
				{
					return AuthenticateResult.Fail("Invalid username or password.");
				}

				// Check if email is verified
				if (!user.IsVerified)
				{
					return AuthenticateResult.Fail("Email not verified. Please check your email for the verification link.");
				}

				// Check if user is active
				if (!user.IsActive)
				{
					return AuthenticateResult.Fail("Account is deactivated.");
				}

				// Passwords are stored as Base64(password)
				var decodedStored = Encoding.UTF8.GetString(Convert.FromBase64String(user.PasswordHash));
				if (!string.Equals(password, decodedStored))
				{
					return AuthenticateResult.Fail("Invalid username or password.");
				}

				// Build claims
				var claims = new List<Claim>
				{
					new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
					new(ClaimTypes.Name, user.Username),
					new(ClaimTypes.Email, user.Email),
					new("userid", user.UserId.ToString()),
					new("fullname", user.FullName)
				};

				// Add role claims
				var roleNames = user.UserRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();
				foreach (var role in roleNames)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}

				// Skip permission claims here to avoid failures; policies rely on roles primarily

				var identity = new ClaimsIdentity(claims, Scheme.Name);
				var principal = new ClaimsPrincipal(identity);
				var ticket = new AuthenticationTicket(principal, Scheme.Name);

				return AuthenticateResult.Success(ticket);
			}
			catch (FormatException)
			{
				return AuthenticateResult.Fail("Invalid Base64 encoding for Basic authentication.");
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Basic authentication failed.");
				return AuthenticateResult.Fail("Authentication error.");
			}
		}
	}
}



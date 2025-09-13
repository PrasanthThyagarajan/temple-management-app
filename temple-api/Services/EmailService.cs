using System.Net;
using System.Net.Mail;

namespace TempleApi.Services
{
	public class EmailService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;

		public EmailService(IConfiguration configuration, ILogger logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendAsync(string toEmail, string subject, string body)
		{
			var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
			var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
			var user = _configuration["Smtp:Username"] ?? string.Empty;
			var pass = _configuration["Smtp:Password"] ?? string.Empty;
			var from = _configuration["Smtp:From"] ?? user;

			using var client = new SmtpClient(host, port)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(user, pass)
			};
			using var msg = new MailMessage(from, toEmail, subject, body)
			{
				IsBodyHtml = false
			};
			await client.SendMailAsync(msg);
		}
	}
}

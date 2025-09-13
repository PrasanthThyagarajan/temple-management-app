using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TempleApi.Models.DTOs;
using TempleApi.Services;

namespace TempleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _authService.LoginAsync(request);
                
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login endpoint");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        [HttpGet("verify")]
        public async Task<ActionResult> Verify([FromQuery] string code)
        {
            try
            {
                var ok = await _authService.VerifyAsync(code);
                if (!ok) return BadRequest("Invalid or expired verification code");
                return Ok(new { message = "Account verified successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in verify endpoint");
                return StatusCode(500, "An internal error occurred");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _authService.RegisterAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("userid");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, "An internal error occurred");
            }
        }

        [HttpGet("roles")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetUserRoles()
        {
            try
            {
                var userIdClaim = User.FindFirst("userid");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var roles = await _authService.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return StatusCode(500, "An internal error occurred");
            }
        }

        [HttpGet("permissions")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetUserPermissions()
        {
            try
            {
                var userIdClaim = User.FindFirst("userid");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var permissions = await _authService.GetUserPermissionsAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions");
                return StatusCode(500, "An internal error occurred");
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, "An internal error occurred");
            }
        }
    }
}

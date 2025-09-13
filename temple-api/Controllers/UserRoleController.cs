using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TempleApi.Services.Interfaces;
using TempleApi.Domain.Entities;

namespace TempleApi.Controllers
{
    [Authorize(Policy = "UserRoleConfiguration")]
    [ApiController]
    [Route("api/userroles")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles()
        {
            var userRoles = await _userRoleService.GetAllUserRolesAsync();
            return Ok(userRoles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserRole([FromBody] UserRole userRole)
        {
            var createdUserRole = await _userRoleService.CreateUserRoleAsync(userRole);
            return CreatedAtAction(nameof(GetUserRoles), new { id = createdUserRole.UserRoleId }, createdUserRole);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UserRole userRole)
        {
            if (id != userRole.UserRoleId)
            {
                return BadRequest();
            }

            var updatedUserRole = await _userRoleService.UpdateUserRoleAsync(userRole);
            return Ok(updatedUserRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            var success = await _userRoleService.DeleteUserRoleAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}

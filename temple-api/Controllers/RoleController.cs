using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TempleApi.Services.Interfaces;
using TempleApi.Domain.Entities;

namespace TempleApi.Controllers
{
	[ApiController]
	[Route("api/roles")]
	[Authorize(Policy = "UserRoleConfiguration")]
	public class RoleController : ControllerBase
	{
		private readonly IRoleService _roleService;

		public RoleController(IRoleService roleService)
		{
			_roleService = roleService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var roles = await _roleService.GetAllRolesAsync();
			return Ok(roles);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var role = await _roleService.GetRoleByIdAsync(id);
			if (role == null) return NotFound();
			return Ok(role);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] Role role)
		{
			var created = await _roleService.CreateRoleAsync(role);
			return CreatedAtAction(nameof(GetById), new { id = created.RoleId }, created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] Role role)
		{
			if (id != role.RoleId) return BadRequest();
			var updated = await _roleService.UpdateRoleAsync(role);
			return Ok(updated);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var ok = await _roleService.DeleteRoleAsync(id);
			if (!ok) return NotFound();
			return NoContent();
		}
	}
}

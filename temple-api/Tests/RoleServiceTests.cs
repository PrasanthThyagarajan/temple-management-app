using System.Threading.Tasks;
using Xunit;
using Moq;
using TempleApi.Services;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using System.Collections.Generic;

namespace TempleApi.Tests
{
    public class RoleServiceTests
    {
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roleService = new RoleService(_roleRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllRolesAsync_ShouldReturnAllRoles()
        {
            // Arrange
            var roles = new List<Role> { new Role { RoleId = 1, RoleName = "Admin" } };
            _roleRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(roles);

            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            Assert.Equal(roles, result);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldAddRole()
        {
            // Arrange
            var role = new Role { RoleId = 1, RoleName = "Admin" };
            _roleRepositoryMock.Setup(repo => repo.AddAsync(role)).ReturnsAsync(role);

            // Act
            var result = await _roleService.CreateRoleAsync(role);

            // Assert
            _roleRepositoryMock.Verify(repo => repo.AddAsync(role), Times.Once);
            Assert.Equal(role, result);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldUpdateRole()
        {
            // Arrange
            var role = new Role { RoleId = 1, RoleName = "Admin" };
            _roleRepositoryMock.Setup(repo => repo.UpdateAsync(role)).Returns(Task.CompletedTask);

            // Act
            var result = await _roleService.UpdateRoleAsync(role);

            // Assert
            _roleRepositoryMock.Verify(repo => repo.UpdateAsync(role), Times.Once);
            Assert.Equal(role, result);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldDeleteRole()
        {
            // Arrange
            var role = new Role { RoleId = 1, RoleName = "Admin" };
            _roleRepositoryMock.Setup(repo => repo.GetByIdAsync(role.RoleId)).ReturnsAsync(role);
            _roleRepositoryMock.Setup(repo => repo.DeleteAsync(role)).Returns(Task.CompletedTask);

            // Act
            var result = await _roleService.DeleteRoleAsync(role.RoleId);

            // Assert
            _roleRepositoryMock.Verify(repo => repo.DeleteAsync(role), Times.Once);
            Assert.True(result);
        }
    }
}

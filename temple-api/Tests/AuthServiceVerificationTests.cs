using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class AuthServiceVerificationTests
    {
        private TempleDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TempleDbContext(options);
        }

        [Fact]
        public async Task VerifyAsync_ShouldActivateAndClearCode_WhenValid()
        {
            using var db = CreateDb();
            // Seed role General to avoid role lookups (not required for verify)
            db.Roles.Add(new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var user = new User
            {
                Username = "test@example.com",
                Email = "test@example.com",
                FullName = "Test",
                PasswordHash = "hash",
                IsActive = false,
                IsVerified = false,
                VerificationCode = "code123",
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var svc = new AuthService(db, cfg, new NullLogger<AuthService>());

            var ok = await svc.VerifyAsync("code123");
            ok.Should().BeTrue();

            var updated = await db.Users.FirstAsync();
            updated.IsActive.Should().BeTrue();
            updated.IsVerified.Should().BeTrue();
            updated.VerificationCode.Should().BeEmpty();
        }

        [Fact]
        public async Task VerifyAsync_ShouldReturnFalse_WhenCodeInvalid()
        {
            using var db = CreateDb();
            var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var svc = new AuthService(db, cfg, new NullLogger<AuthService>());

            var ok = await svc.VerifyAsync("nope");
            ok.Should().BeFalse();
        }
    }
}

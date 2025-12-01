using RoleService.Domain.Entities;
using RoleService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RoleService.Tests.Infrastructure.Persistence
{
    public class EfRoleRepositoryTests
    {
        [Fact]
        public async Task AddAsync_AddsRoleToDbContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
                .Options;

            using var context = new ApplicationDbContext(options);
            var repository = new EfRoleRepository(context);
            var Role = new Role
            {
                Id = Guid.NewGuid(),
                ServiceName = "TestService",
                ExceptionType = "TestException",
                Message = "Test message",
                StackTrace = "Test stack trace",
                OccurredAt = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(Role);

            // Assert
            var savedError = await context.Roles.FirstOrDefaultAsync();
            Assert.NotNull(savedError);
            Assert.Equal("TestService", savedError.ServiceName);
            Assert.Equal("TestException", savedError.ExceptionType);
            Assert.Equal("Test message", savedError.Message);
        }
    }
}

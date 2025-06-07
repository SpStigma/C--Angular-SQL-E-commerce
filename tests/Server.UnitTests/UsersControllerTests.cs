using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using server.Controllers;
using server.Data;
using server.Dtos;       // <-- Import de UserDto
using server.Models;
using Xunit;

namespace Server.UnitTests
{
    public class UsersControllerTests
    {
        private AppDbContext CreateDb(string name) => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options);

        private UsersController CreateController(AppDbContext ctx, ClaimsPrincipal? user = null)
        {
            var jwtSettings = Options.Create(new JwtSettings
            {
                Key = "testkey1234567890123456789012345",
                Issuer = "test",
                Audience = "test",
                ExpireMinutes = 60
            });
            var ctrl = new UsersController(ctx, jwtSettings);
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user ?? new ClaimsPrincipal()
                }
            };
            return ctrl;
        }

        [Fact]
        public async Task GetMe_ShouldReturnUnauthorized_IfNoUserClaim()
        {
            var ctrl = CreateController(CreateDb(nameof(GetMe_ShouldReturnUnauthorized_IfNoUserClaim)));
            var res = await ctrl.GetMe();
            res.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMe_ShouldReturnNotFound_IfUserNotExists()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"));
            var ctrl = CreateController(CreateDb(nameof(GetMe_ShouldReturnNotFound_IfUserNotExists)), user);
            var res = await ctrl.GetMe();
            res.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMe_ShouldReturnUser_WhenExists()
        {
            var ctx = CreateDb(nameof(GetMe_ShouldReturnUser_WhenExists));
            ctx.Users.Add(new User { Id = 2, Username = "bob", Email = "bob@example.com", Role = "user" });
            await ctx.SaveChangesAsync();

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "2") }, "TestAuth"));
            var ctrl = CreateController(ctx, user);

            var ok = await ctrl.GetMe() as OkObjectResult;
            ok.Should().NotBeNull();

            var resultObj = ok!.Value!;
            var type      = resultObj.GetType();
            var id        = (int)   type.GetProperty("Id")!.GetValue(resultObj)!;
            var username  = (string)type.GetProperty("Username")!.GetValue(resultObj)!;
            var email     = (string)type.GetProperty("Email")!.GetValue(resultObj)!;

            id.Should().Be(2);
            username.Should().Be("bob");
            email.Should().Be("bob@example.com");
        }

        [Fact]
        public void GetRole_ShouldReturnRole()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Role, "admin") }, "TestAuth"));
            var ctrl = CreateController(CreateDb(nameof(GetRole_ShouldReturnRole)), user);

            var ok = ctrl.GetRole() as OkObjectResult;
            ok.Should().NotBeNull();

            var val = ok!.Value!;
            string roleValue;

            if (val is string s)
            {
                roleValue = s;
            }
            else
            {
                var type = val.GetType();
                var prop = type.GetProperty("role") 
                        ?? type.GetProperty("Role");
                prop.Should().NotBeNull("le résultat doit exposer une propriété role/Role");
                roleValue = (string)prop!.GetValue(val)!;
            }

            roleValue.Should().Be("admin");
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnList_ForAdmin()
        {
            var ctx = CreateDb(nameof(GetAllUsers_ShouldReturnList_ForAdmin));
            ctx.Users.AddRange(
                new User { Id = 1, Username = "u1", Email = "u1@ex.com", Role = "user" },
                new User { Id = 2, Username = "u2", Email = "u2@ex.com", Role = "admin" }
            );
            await ctx.SaveChangesAsync();

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Role, "admin") }, "TestAuth"));
            var ctrl = CreateController(ctx, user);

            // Appel de l'action
            var action = await ctrl.GetAllUsers();
            // Le Result est un OkObjectResult
            var ok     = action.Result as OkObjectResult;
            ok.Should().NotBeNull();

            // On caste en IEnumerable<UserDto>, pas User
            var list = ok!.Value as IEnumerable<UserDto>;
            list.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(u => u.Username == "u1" && u.Email == "u1@ex.com" && u.Role == "user")
                .And.Contain(u => u.Username == "u2" && u.Email == "u2@ex.com" && u.Role == "admin");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Controllers;
using server.Data;
using server.Models;
using Xunit;

namespace Server.UnitTests
{
    public class CartControllerTests
    {
        private AppDbContext GetDb(string name)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(opts);
        }

        private CartController SetupController(AppDbContext ctx, int? userId)
        {
            var ctrl = new CartController(ctx);
            var http = new DefaultHttpContext();
            if (userId.HasValue)
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())
                }, "TestAuth");
                http.User = new ClaimsPrincipal(identity);
            }
            ctrl.ControllerContext = new ControllerContext { HttpContext = http };
            return ctrl;
        }

        [Fact]
        public async Task GetCart_ShouldReturnUnauthorized_IfNoUser()
        {
            var ctx = GetDb(nameof(GetCart_ShouldReturnUnauthorized_IfNoUser));
            var ctrl = SetupController(ctx, userId: null);

            var res = await ctrl.GetCart();

            res.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetCart_ShouldReturnEmpty_WhenNoItems()
        {
            var ctx = GetDb(nameof(GetCart_ShouldReturnEmpty_WhenNoItems));
            ctx.Carts.Add(new Cart { UserId = 1, Items = new List<CartItem>() });
            await ctx.SaveChangesAsync();

            var ctrl = SetupController(ctx, userId: 1);
            var ok = await ctrl.GetCart() as OkObjectResult;
            ok.Should().NotBeNull();

            var resultObj = ok!.Value!;
            var totalProp = resultObj.GetType().GetProperty("total", BindingFlags.Public | BindingFlags.Instance);
            var countProp = resultObj.GetType().GetProperty("itemCount", BindingFlags.Public | BindingFlags.Instance);
            totalProp.Should().NotBeNull();
            countProp.Should().NotBeNull();

            decimal total = (decimal)totalProp!.GetValue(resultObj)!;
            int count = (int)countProp!.GetValue(resultObj)!;

            total.Should().Be(0m);
            count.Should().Be(0);
        }

        [Fact]
        public async Task GetCart_ShouldComputeTotalAndItemCount()
        {
            var ctx = GetDb(nameof(GetCart_ShouldComputeTotalAndItemCount));
            var prod = new Product { Id = 10, Name = "P", Price = 2m };
            ctx.Products.Add(prod);
            var cart = new Cart { Id = 5, UserId = 2, Items = new List<CartItem>() };
            cart.Items.Add(new CartItem { CartId = 5, ProductId = 10, Quantity = 3, Product = prod });
            ctx.Carts.Add(cart);
            await ctx.SaveChangesAsync();

            var ctrl = SetupController(ctx, userId: 2);
            var ok = await ctrl.GetCart() as OkObjectResult;
            ok.Should().NotBeNull();

            var resultObj = ok!.Value!;
            var totalProp = resultObj.GetType().GetProperty("total", BindingFlags.Public | BindingFlags.Instance);
            var countProp = resultObj.GetType().GetProperty("itemCount", BindingFlags.Public | BindingFlags.Instance);
            totalProp.Should().NotBeNull();
            countProp.Should().NotBeNull();

            decimal total = (decimal)totalProp!.GetValue(resultObj)!;
            int count = (int)countProp!.GetValue(resultObj)!;

            total.Should().Be(6m);    // 2 * 3
            count.Should().Be(3);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
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
    public class OrdersControllerTests
    {
        private AppDbContext CreateDb(string name) => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options);

        private OrdersController CreateController(AppDbContext ctx, int? userId, bool isAdmin = false)
        {
            var ctrl = new OrdersController(ctx);
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
                if (isAdmin)
                    claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };
            return ctrl;
        }

        [Fact]
        public async Task PlaceOrder_ShouldReturnUnauthorized_IfNoUser()
        {
            var res = await CreateController(CreateDb(nameof(PlaceOrder_ShouldReturnUnauthorized_IfNoUser)), null)
                .PlaceOrder();
            res.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task PlaceOrder_ShouldReturnBadRequest_IfCartEmpty()
        {
            var ctrl = CreateController(CreateDb(nameof(PlaceOrder_ShouldReturnBadRequest_IfCartEmpty)), 1);
            var bad = await ctrl.PlaceOrder() as BadRequestObjectResult;
            bad.Should().NotBeNull();

            var resultObj = bad!.Value!;
            var message = (string)resultObj.GetType()
                .GetProperty("message")!.GetValue(resultObj)!;
            message.Should().Be("Le panier est vide");
        }

        [Fact]
        public async Task PlaceOrder_ShouldCreateOrder_WhenCartHasItems()
        {
            var ctx = CreateDb(nameof(PlaceOrder_ShouldCreateOrder_WhenCartHasItems));
            ctx.Products.Add(new Product { Id = 10, Name = "X", Stock = 5, Price = 2m });
            var cart = new Cart { UserId = 2, Items = new List<CartItem>() };
            cart.Items.Add(new CartItem { ProductId = 10, Quantity = 3 });
            ctx.Carts.Add(cart);
            await ctx.SaveChangesAsync();

            var ok = await CreateController(ctx, 2).PlaceOrder() as OkObjectResult;
            ok.Should().NotBeNull();

            var order = ok!.Value as Order;
            order.Should().NotBeNull();
            order!.TotalAmount.Should().Be(6m);
        }

        [Fact]
        public async Task GetMyOrders_ShouldReturnUnauthorized_IfNoUser()
        {
            var action = await CreateController(CreateDb(nameof(GetMyOrders_ShouldReturnUnauthorized_IfNoUser)), null)
                .GetMyOrders();
            action.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetMyOrders_ShouldReturnOnlyUserOrders()
        {
            var ctx = CreateDb(nameof(GetMyOrders_ShouldReturnOnlyUserOrders));
            ctx.Orders.AddRange(
                new Order { Id = 1, UserId = 1, TotalAmount = 1m },
                new Order { Id = 2, UserId = 2, TotalAmount = 2m }
            );
            await ctx.SaveChangesAsync();

            var action = await CreateController(ctx, 2).GetMyOrders();
            var ok     = action.Result as OkObjectResult;
            ok.Should().NotBeNull();

            var list = ok!.Value as IEnumerable<Order>;
            list.Should().ContainSingle(o => o.Id == 2);
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnList_ForAnyUser()
        {
            var ctx = CreateDb(nameof(GetAllOrders_ShouldReturnList_ForAnyUser));
            ctx.Orders.Add(new Order { Id = 5, UserId = 3, TotalAmount = 3m });
            await ctx.SaveChangesAsync();

            // non-admin
            var action1 = await CreateController(ctx, 1).GetAllOrders();
            var ok1     = action1.Result as OkObjectResult;
            ok1.Should().NotBeNull();
            var list1   = ok1!.Value as IEnumerable<Order>;
            list1.Should().ContainSingle(o => o.Id == 5);

            // admin
            var action2 = await CreateController(ctx, 1, isAdmin: true).GetAllOrders();
            var ok2     = action2.Result as OkObjectResult;
            ok2.Should().NotBeNull();
            var list2   = ok2!.Value as IEnumerable<Order>;
            list2.Should().ContainSingle(o => o.Id == 5);
        }
    }
}

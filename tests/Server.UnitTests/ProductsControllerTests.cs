using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using server.Controllers;
using server.Data;
using server.Models;
using Xunit;

namespace Server.UnitTests
{
    public class ProductsControllerTests
    {
        private AppDbContext GetInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private ProductsController CreateController(AppDbContext ctx)
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns("Development");
            return new ProductsController(ctx, envMock.Object);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnAllProducts()
        {
            // Arrange
            var ctx = GetInMemoryDb(nameof(GetProducts_ShouldReturnAllProducts));
            ctx.Products.AddRange(
                new Product { Id = 1, Name = "A", Stock = 2 },
                new Product { Id = 2, Name = "B", Stock = 5 }
            );
            await ctx.SaveChangesAsync();
            var ctrl = CreateController(ctx);

            // Act
            var ok = await ctrl.GetProducts() as OkObjectResult;

            // Assert
            ok.Should().NotBeNull();
            var list = ok.Value as List<Product>;
            list.Should().HaveCount(2)
                         .And.Contain(p => p.Name == "A")
                         .And.Contain(p => p.Name == "B");
        }

        [Fact]
        public async Task GetProduct_ShouldReturnNotFound_IfMissing()
        {
            var ctx = GetInMemoryDb(nameof(GetProduct_ShouldReturnNotFound_IfMissing));
            var ctrl = CreateController(ctx);

            var res = await ctrl.GetProduct(42);

            res.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetProduct_ShouldReturnProduct_WhenExists()
        {
            var ctx = GetInMemoryDb(nameof(GetProduct_ShouldReturnProduct_WhenExists));
            var prod = new Product { Id = 7, Name = "Test", Stock = 3 };
            ctx.Products.Add(prod);
            await ctx.SaveChangesAsync();
            var ctrl = CreateController(ctx);

            var ok = await ctrl.GetProduct(7) as OkObjectResult;

            ok.Should().NotBeNull();
            (ok.Value as Product).Name.Should().Be("Test");
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnNotFound_IfMissing()
        {
            var ctx = GetInMemoryDb(nameof(UpdateStock_ShouldReturnNotFound_IfMissing));
            var ctrl = CreateController(ctx);

            var res = await ctrl.UpdateStock(99, 5);

            res.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateStock_ShouldAdjustStock_WhenExists()
        {
            var ctx = GetInMemoryDb(nameof(UpdateStock_ShouldAdjustStock_WhenExists));
            var prod = new Product { Id = 3, Name = "X", Stock = 10 };
            ctx.Products.Add(prod);
            await ctx.SaveChangesAsync();
            var ctrl = CreateController(ctx);

            var ok = await ctrl.UpdateStock(3, -4) as OkObjectResult;

            ok.Should().NotBeNull();

            // Récupère par réflexion la propriété "stock"
            var resultObj = ok.Value!;
            var stockProp = resultObj.GetType()
                .GetProperty("stock", BindingFlags.Public | BindingFlags.Instance)!;
            var stockValue = (int)stockProp.GetValue(resultObj)!;

            stockValue.Should().Be(6);

            // et vérifier que ça a bien persisté en base
            (await ctx.Products.FindAsync(3))!.Stock.Should().Be(6);
        }
    }
}

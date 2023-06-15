using Microsoft.AspNetCore.Mvc;
using Playground.Shared.Models;
using Playground.Shared.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;

namespace client_sandbox.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly ProductService productService;

        public ProductController(ProductService productService)
        {
            this.productService = productService;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get(int count, bool includeOrderDetails = false)
        {
            return await productService.GetProductsAsync(count, includeOrderDetails);
        }

        // HttpClientJsonExtensions throw exception if directly accessing object https://github.com/dotnet/aspnetcore/issues/13052
        // Tested with System.Text.Json 5.0.0-preview.1.20120.5 - works fine. Til then, manual serialization/deserialization needed

        //[HttpPost]
        //public async Task<IEnumerable<Product>> GetRequestedProducts([FromBody] DataSourceRequest dataSourceRequest)
        //{
        //    var products = await productService.GetProductsAsync(100);
        //    var dataSourceResult = products.ToDataSourceResult(dataSourceRequest);

        //    return dataSourceResult.Data.OfType<Product>();
        //}

        [HttpPost]
        public async Task<ProductsDataSourceResult> GetRequestedProducts([FromBody] string dataSourceRequestJson)
        {
            var dataSourceRequest = JsonSerializer.Deserialize<DataSourceRequest>(dataSourceRequestJson);

            var products = await productService.GetProductsAsync(100);
            var dataSourceResult = products.ToDataSourceResult(dataSourceRequest);

            return new ProductsDataSourceResult()
            {
                Data = dataSourceResult.Data.OfType<Product>(),
                GroupData = dataSourceResult.Data.OfType<AggregateFunctionsGroup>(),
                Total = dataSourceResult.Total
            };
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Playground.Shared.Models;
using Playground.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace client_sandbox.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductEquatableController : ControllerBase
    {
        private readonly ProductService productService;

        public ProductEquatableController(ProductService productService)
        {
            this.productService = productService;
        }
        

        [HttpGet]
        public async Task<IEnumerable<ProductEquatable>> Get(int count)
        {

            return await productService.GetProductEquatableAsync(count);
        }
    }
}

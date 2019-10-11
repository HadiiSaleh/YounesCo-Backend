using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor
        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        #endregion

        #region GetProducts

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetProducts()
        {
            var result = _db.Products.ToList().Where(p => p.OutOfStock == false);

            if (result.Count() <= 0) return NotFound(new JsonResult("No products existed."));

            return Ok(result);
        }

        #endregion

        #region GetProductsByCategoryId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetProductsByCategoryId([FromRoute] int id)
        {
            var result = _db.Products.ToList().Where(p => p.OutOfStock == false).Where(p => p.CategoryId == id);

            if (result.Count() <= 0) return NotFound(new JsonResult("No products of this category existed."));

            return Ok(result);
        }

        #endregion

        #region GetProductByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Product>> GetProductByIdAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var findProduct = await _db.Products.FindAsync(id);

            if (findProduct != null && findProduct.OutOfStock == false)
                return Ok(new JsonResult(findProduct));

            else
                return NotFound();
        }

        #endregion

        #region CreateProductAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateProductAsync([FromBody] Product data)
        {
            var newproduct = new Product
            {
                Name = data.Name,
                Description = data.Description,
                OutOfStock = data.OutOfStock,
                Quantity = data.Quantity,
                Price = data.Price,
                CategoryId = data.CategoryId
            };

            await _db.Products.AddAsync(newproduct);

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("Product Added Successfully"));

        }

        #endregion

        #region UpdateProductAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] Product data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != data.ProductId) return BadRequest();

            var findProduct = _db.Products.FirstOrDefault(p => p.ProductId == id);

            if (findProduct == null)
            {
                return NotFound();
            }

            // If the product was found
            findProduct.Name = data.Name;
            findProduct.Description = data.Description;
            findProduct.OutOfStock = data.OutOfStock;
            findProduct.Price = data.Price;
            findProduct.Quantity = data.Quantity;
            findProduct.CategoryId = data.CategoryId;
            findProduct.UpdatedAt = DateTime.Now;

            _db.Entry(findProduct).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Product with id " + id + " is updated"));

        }

        #endregion

        #region DeleteProductAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // find the product

            var findProduct = await _db.Products.FindAsync(id);

            if (findProduct == null || findProduct.OutOfStock)
            {
                return NotFound();
            }

            // If the product was found
            findProduct.OutOfStock = true;
            findProduct.UpdatedAt = DateTime.Now;

            _db.Entry(findProduct).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Product with id " + id + " is now out of stock."));

        }

        #endregion

        #region DeleteProductsByCategoryIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteProductsByCategoryIdAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // find the product

            var result = _db.Products.ToList().Where(c => c.CategoryId == id).Where(c => c.OutOfStock == false);

            if (result.Count() <= 0)
            {
                return NotFound();
            }

            // If products was found

            foreach (Product product in result)
            {
                product.OutOfStock = true;
                product.UpdatedAt = DateTime.Now;

                _db.Entry(product).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();

            Category category = await _db.Categories.FindAsync(id);

            // Finally return the result to client
            return Ok(new JsonResult("The Products of category" + category.CategoryName + " is now out of stock."));

        }

        #endregion
    }
}
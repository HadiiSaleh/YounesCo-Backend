using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;
using YounesCo_Backend.ViewModels;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor
        public ProductsController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        #endregion

        #region GetProducts

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _db.Products
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                    .Where(p=>p.Deleted == false)
               // .Where(p => p.OutOfStock == false)
                .ToListAsync()
                ;

            var products = _mapper.Map<List<ProductToListViewModel>>(result);
            return Ok(products);
        }

        #endregion

        #region GetProductsByCategoryId

        //[HttpGet("[action]/{categoryId}")]
        //[Authorize(Policy = "RequireLoggedIn")]
        //public async Task<IActionResult> GetProductsByCategoryId([FromRoute] int categoryId)
        //{
        //    var result = await _db.Products
        //        .Include(p => p.Colors)
        //            .ThenInclude(c => c.Images)
        //       // .Where(p => p.OutOfStock == false && p.CategoryId == categoryId)
        //        .ToListAsync()
        //        ;

        //    return Ok(result);
        //}


        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetProductsByType([FromRoute] int id)
        {
            var products = await _db.Products
                .Include(p => p.Colors)
                .ThenInclude(c => c.Images)
                .Include(p => p.Type)
                .Where(p => p.TypeId == id)
                .ToListAsync();

            return Ok(products);
        }


        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Product>> AssigneProductToType([FromBody] AddProductsToTypeViewModel productsToTypeViewModel)
        {
            if(productsToTypeViewModel == null)
            {
                return BadRequest("Invalid Products");
            }

            var products = productsToTypeViewModel.ProductsIds;
            var type = await _db.Types.FindAsync(productsToTypeViewModel.TypeId);

            if(type == null)
            {
                return NotFound("Type not found");
            }

            for (int i = 0; i < products.Count; i++)
            {
                var product = await _db.Products
                    .Include(p => p.Type)
                    .FirstOrDefaultAsync(p => p.ProductId == products[i]);

                if(product != null && !product.Deleted)
                {
                    if (product.Type == null)
                    {
                        product.TypeId = productsToTypeViewModel.TypeId;
                        product.UpdatedAt = DateTime.Now;
                    }
                }
                else
                {
                    return NotFound($"Product with id {products[i]} not found");
                }
            }

            await _db.SaveChangesAsync();

            return Ok($"Products Added to {type.TypeName}");
        }

        #endregion

        #region GetProductByIdAsync

        [HttpGet("[action]/{id}",Name ="GetProductById")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Product>> GetProductById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest("Invalid Id!");

            var findProduct = await _db.Products
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                    .Where(p => p.Deleted == false)
                    .Where(p=> p.OutOfStock == false)
                .SingleOrDefaultAsync(p => p.ProductId == id)
                ;
            if(findProduct == null)
            {
                return NotFound("Product cannot be found");
            }

            return Ok(findProduct);
        }

        #endregion

        #region CreateProductAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductViewModel data)
        {
            var productFromDb = await _db.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == data.Name.ToLower());

            if(productFromDb != null)
            {
                return Conflict($"Product {productFromDb.Name} already exist");
            }

            var product = _mapper.Map<Product>(data);

            await _db.Products.AddAsync(product);

            await _db.SaveChangesAsync();

            return CreatedAtRoute("GetProductById", new { controller = "Products", id= product.ProductId}, product);
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
           // findProduct.CategoryId = data.CategoryId;
            findProduct.UpdatedAt = DateTime.Now;

            _db.Entry(findProduct).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Product with id " + id + " is updated"));

        }

        #endregion

        #region DeleteProductAsync

        [HttpPut("[action]/{id}")]
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
                return NotFound(new JsonResult("Product not found"));
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

        #region UnDeleteProductAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UnDeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // find the product

            var findProduct = await _db.Products.FindAsync(id);

            if (findProduct == null)
            {
                return NotFound(new JsonResult("Product not found"));
            }

            if (findProduct.OutOfStock == false)
                return NotFound(new JsonResult("Product already in stock"));

            // If the product was found
            findProduct.OutOfStock = false;
            findProduct.UpdatedAt = DateTime.Now;

            _db.Entry(findProduct).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Product with id " + id + " is now in stock."));

        }

        #endregion

        #region DeleteProductsByCategoryIdAsync

        //[HttpPut("[action]/{id}")]
        //[Authorize(Policy = "RequireAdministratorRole")]
        //public async Task<IActionResult> DeleteProductsByCategoryId([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // find the product

        //    var result = _db.Products.ToList().Where(c => c.CategoryId == id).Where(c => c.OutOfStock == false);

        //    Category category = await _db.Categories.FindAsync(id);

        //    if (category == null)
        //        return NotFound(new JsonResult("Invalid Category Id"));

        //    if (result.Count() <= 0)
        //    {
        //        return NotFound(new JsonResult("No Product in catgory " + category.CategoryName));
        //    }

        //    // If products was found

        //    foreach (Product product in result)
        //    {
        //        product.OutOfStock = true;
        //        product.UpdatedAt = DateTime.Now;

        //        _db.Entry(product).State = EntityState.Modified;
        //    }

        //    await _db.SaveChangesAsync();

        //    // Finally return the result to client
        //    return Ok(new JsonResult("The Products of category" + category.CategoryName + " is now out of stock."));

        //}

        #endregion
    }
}
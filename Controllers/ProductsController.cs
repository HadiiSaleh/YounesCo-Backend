using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Data;
using YounesCo_Backend.Helpers;
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
        public async Task<IActionResult> GetProducts([FromQuery] ProductsParams productsParams)
        {
            var products = _db.Products
                .Include(p => p.Color)
                .Include(p=>p.Images)
                    .Where(p=>p.Deleted == false)
                .AsQueryable();

            if (!string.IsNullOrEmpty(productsParams.CategoriesId))
            {
                string[] categoriesId = productsParams.CategoriesId.Split(',');

                products = products.Where(c => categoriesId.ToList().Contains(c.CategoryId.ToString()));

            }

            if (productsParams.MinPrice > 0) 
            {
                products = products.Where(p => p.Price >= productsParams.MinPrice);
            }

            //need to change the condition to set MaxPrice > MaxPrice Exist in DB
            if(productsParams.MaxPrice < 100000)
            {
                products = products.Where(p => p.Price <= productsParams.MaxPrice);
            }

            if (!string.IsNullOrEmpty(productsParams.ColorsId))
            {
                string[] colorsIds = productsParams.ColorsId.Split(',');

                products = products.Where(c => colorsIds.ToList().Contains(c.ColorId.ToString()));

            }

                if (!string.IsNullOrEmpty(productsParams.Search))
            {
                products = products.Where(p => p.Name.Contains(productsParams.Search));
            }

            if (!string.IsNullOrEmpty(productsParams.OrderByPrice))
            {
                products = productsParams.OrderByPrice == "asc" ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price);
            }
            var result = await Pagination.GetPagedAsync(products, productsParams.PageNumber, productsParams.PageSize);

            Response.AddPagination(result.CurrentPage, result.PageSize, result.RowCount, result.PageCount);
            var productsToReturn = _mapper.Map<List<ProductToListViewModel>>(result.Results);

            return Ok(productsToReturn);
        }

        #endregion

        #region GetProductsByCategoryId

        [HttpGet("[action]/{categoryId}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetProductsByCategoryId([FromRoute] int categoryId)
        {
            var result = await _db.Products
                .Include(p => p.Color)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();

            return Ok(result);
        }

        #endregion


        #region GetProductByIdAsync

        [HttpGet("[action]/{id}",Name ="GetProductById")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Product>> GetProductById([FromRoute] int id)
        {

            if (id <= 0) return BadRequest("Invalid Id!");

            var findProduct = await _db.Products
                .Include(p => p.Color)
                .Include(p => p.Images)
                    .Where(p => p.Deleted == false)
                .SingleOrDefaultAsync(p => p.ProductId == id)
                ;
            if(findProduct == null)
            {
                return NotFound("Product cannot be found");
            }
            var productToReturn = _mapper.Map<ProductDetailsViewModel>(findProduct);
            return Ok(productToReturn);
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


            var category = await _db.Categories.FirstOrDefaultAsync(category => category.CategoryName == data.CategoryName && !category.Deleted);

            if (category == null)
            {
                category = new Category()
                {
                    CategoryName = data.CategoryName
                };
                await _db.AddAsync(category);
            }

            product.Category = category;

            var color = await _db.Colors
                    .FirstOrDefaultAsync(c => (c.ColorName.ToLower() == data.ColorName.ToLower()) && !c.Deleted);


            if(color == null)
            {
                color = new Color()
                {
                    ColorName = data.ColorName,
                    ColorCode = data.ColorName.ToUpper(),
                    Default = true
                };
                await _db.AddAsync(color);
            }

             product.Color = color;
            //product.Colors.Add(new Color()
            //{
            //    ColorName = data.ColorName,
            //    ColorCode = data.ColorName,
            //    Default = true,
            //    Deleted = false,
            //}
            //);

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

            if (findProduct == null)
            {
                return NotFound("Product not found");
            }

            // If the product was found
            findProduct.OutOfStock = true;
            findProduct.Deleted = true;
            findProduct.UpdatedAt = DateTime.Now;

            _db.Entry(findProduct).State = EntityState.Modified;

           await _db.SaveChangesAsync();


            // Finally return the result to client
            return NoContent();

        }

        #endregion

        #region UnDeleteProductAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UnDeleteProduct([FromRoute] int id)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            // find the product

            var findProduct = await _db.Products.FindAsync(id);

            if (findProduct == null)
            {
                return NotFound("Product not found");
            }

            if (findProduct.OutOfStock == false)
                return NotFound("Product already in stock");

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
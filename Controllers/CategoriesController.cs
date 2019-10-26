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
    public class CategoriesController : ControllerBase
    {

        #region Attributes

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor
        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        #endregion

        #region GetCategories

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetCategories()
        {
            var result = _db.Categories.ToList().Where(c => c.Deleted == false);

            if (result == null) return NotFound(new JsonResult("No categories existed."));

            return Ok(result);

        }

        #endregion

        #region GetCategoryByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Category>> GetCategoryById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var category = await _db.Categories
                .Include(cat => cat.Products)
                .SingleOrDefaultAsync(c => c.CategoryId == id)
                ;

            if (category == null || category.Deleted)
            {
                return NotFound(new JsonResult("Category Not Found"));
            }

            return Ok(new JsonResult(category));
        }

        #endregion

        #region CreateCategoryAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateCategory([FromBody] Category data)
        {
            var newcategory = new Category
            {
                CategoryName = data.CategoryName,
                Deleted = false,
            };

            var createCat = await _db.Categories.AddAsync(newcategory);

            if (createCat == null)
                return BadRequest(new JsonResult("Category Creation Failed"));

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Category was Added Successfully"));

        }

        #endregion

        #region UpdateCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findCategory = await _db.Categories.FindAsync(id);

            if (findCategory == null)
            {
                return NotFound("Category Not Found");
            }

            // If the category was found
            findCategory.CategoryName = category.CategoryName;
            findCategory.Deleted = category.Deleted;
            findCategory.UpdatedAt = DateTime.Now;

            _db.Entry(findCategory).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Category with id " + id + " is updated"));
        }

        #endregion

        #region DeleteCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Category>> DeleteCategory([FromRoute] int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            category.Deleted = true;
            category.UpdatedAt = DateTime.Now;

            _db.Entry(category).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Category with id " + id + " is deleted."));
        }

        #endregion

        #region UnDeleteCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Category>> UnDeleteCategory([FromRoute] int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            category.Deleted = false;
            category.UpdatedAt = DateTime.Now;

            _db.Entry(category).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Category with id " + id + " is UnDeleted."));
        }

        #endregion
    }
}
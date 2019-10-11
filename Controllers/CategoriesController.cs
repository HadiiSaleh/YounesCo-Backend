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
        public async Task<ActionResult<Category>> GetCategoryByIdAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var category = await _db.Categories.FindAsync(id);

            if (category == null || category.Deleted)
            {
                return NotFound();
            }

            return Ok(new JsonResult(category));
        }

        #endregion

        #region CreateCategoryAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] Category data)
        {
            var newcategory = new Category
            {
                CategoryName = data.CategoryName,
                Deleted = false,
            };

            await _db.Categories.AddAsync(newcategory);

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Category was Added Successfully"));

        }

        #endregion

        #region UpdateCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateCategoryAsync([FromRoute] int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != category.CategoryId)
            {
                return BadRequest();
            }

            var findCategory = _db.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (findCategory == null)
            {
                return NotFound();
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

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Category>> DeleteCategoryAsync([FromRoute] int id)
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
    }
}
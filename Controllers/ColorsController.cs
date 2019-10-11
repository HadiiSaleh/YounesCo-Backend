using System;
using System.Collections.Generic;
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
    public class ColorsController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor
        public ColorsController(ApplicationDbContext db)
        {
            _db = db;
        }

        #endregion

        #region GetColors

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetColors()
        {
            var result = _db.Colors.ToList().Where(c => c.Deleted == false);

            if (result == null) return NotFound(new JsonResult("No colors existed."));

            return Ok(result);

        }

        #endregion

        #region GetColorByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Color>> GetColorByIdAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var color = await _db.Colors.FindAsync(id);

            if (color == null || color.Deleted)
            {
                return NotFound();
            }

            return Ok(new JsonResult(color));
        }

        #endregion

        #region GetColorsByProductId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetColorsByProductId([FromRoute] int id)
        {
            var result = _db.Colors.ToList().Where(f => f.ProductId == id);

            if (result.Count() < 0)
                return NotFound(new JsonResult("No colors existed."));

            return Ok(result);

        }

        #endregion

        #region CreateColorAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateColorAsync([FromBody] Color data)
        {
            var newcolor = new Color
            {
                ColorName = data.ColorName,
                ColorCode = data.ColorCode,
                Deleted = false,
                Default = data.Default,
                ProductId = data.ProductId
            };

            await _db.Colors.AddAsync(newcolor);

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Color was Added Successfully"));

        }

        #endregion

        #region UpdateColorAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateColorAsync([FromRoute] int id, [FromBody] Color color)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != color.ColorId)
            {
                return BadRequest();
            }

            var findColor = _db.Colors.FirstOrDefault(c => c.ColorId == id);

            if (findColor == null)
            {
                return NotFound();
            }

            // If the color was found
            findColor.ColorName = color.ColorName;
            findColor.ColorCode = color.ColorCode;
            findColor.ProductId = color.ProductId;
            findColor.Default = color.Default;
            findColor.Deleted = color.Deleted;
            findColor.UpdatedAt = DateTime.Now;

            _db.Entry(findColor).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Color with id " + id + " is updated"));
        }

        #endregion

        #region DeleteColorAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Color>> DeleteColorAsync([FromRoute] int id)
        {
            var color = await _db.Colors.FindAsync(id);

            if (color == null)
            {
                return NotFound();
            }

            color.Deleted = true;
            color.UpdatedAt = DateTime.Now;

            _db.Entry(color).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Color with id " + id + " is deleted."));
        }

        #endregion

        #region DeleteColorByProductIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Color>> DeleteColorByProductIdAsync([FromRoute] int id)
        {
            var result = _db.Colors.ToList().Where(f => f.ProductId == id);

            Product product = await _db.Products.FindAsync(id);

            if (result.Count() < 0)
            {
                return NotFound();
            }

            foreach (Color color in result)
            {
                _db.Colors.Remove(color);
            }

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Colors of product " + product.Name + " is deleted."));
        }

        #endregion
    }
}
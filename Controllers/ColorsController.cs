using System;
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
    public class ColorsController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor
        public ColorsController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        #endregion

        #region GetColors

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetColors()
        {
            var result = await _db.Colors.Where(c => c.Deleted == false).ToListAsync();
            return Ok(result);

        }
        #endregion

        #region GetColorByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Color>> GetColorById([FromRoute] int id)
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

            return Ok(color);
        }

        #endregion

        #region GetColorsByProductId

        //[HttpGet("[action]/{id}")]
        //[Authorize(Policy = "RequireLoggedIn")]
        //public IActionResult GetColorsByProductId([FromRoute] int id)
        //{
        //    var result = _db.Colors.ToList().Where(f => f.ProductId == id);

        //    if (result.Count() < 0)
        //        return NotFound(new JsonResult("No colors existed."));

        //    return Ok(result);

        //}

        #endregion

        #region CreateColorAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateColor([FromBody] ColorForCreationViewModel data)
        {
            var colorFromDb = await _db.Colors
                .FirstOrDefaultAsync(c => c.ColorName.ToLower() == data.ColorName.ToLower());
            
            if(colorFromDb != null)
            {
                return BadRequest($"Color {colorFromDb.ColorName} already exist");
            }
            var colorToSave = _mapper.Map<Color>(data);

             await _db.Colors.AddAsync(colorToSave);

            await _db.SaveChangesAsync();

            return StatusCode(201 ,"The Color was Added Successfully");

        }

        #endregion


        #region UpdateColorAsync

        //[HttpPut("[action]/{id}")]
        //[Authorize(Policy = "RequireAdministratorRole")]
        //public async Task<IActionResult> UpdateColor([FromRoute] int id, [FromBody] Color color)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var findColor = _db.Colors.FirstOrDefault(c => c.ColorId == id);

        //    if (findColor == null)
        //    {
        //        return NotFound();
        //    }

        //    // If the color was found
        //    findColor.ColorName = color.ColorName;
        //    findColor.ColorCode = color.ColorCode;
        //    findColor.ProductId = color.ProductId;
        //    findColor.Default = color.Default;
        //    findColor.Deleted = color.Deleted;
        //    findColor.UpdatedAt = DateTime.Now;

        //    _db.Entry(findColor).State = EntityState.Modified;

        //    await _db.SaveChangesAsync();

        //    return Ok(new JsonResult("The Color with id " + id + " is updated"));
        //}

        #endregion

        #region DeleteColorAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Color>> DeleteColor([FromRoute] int id)
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

        //[HttpDelete("[action]/{id}")]
        //[Authorize(Policy = "RequireLoggedIn")]
        //public async Task<ActionResult<Color>> DeleteColorByProductId([FromRoute] int id)
        //{
        //    var result = _db.Colors.ToList().Where(f => f.ProductId == id);

        //    Product product = await _db.Products.FindAsync(id);

        //    if (result.Count() < 0)
        //    {
        //        return NotFound();
        //    }

        //    foreach (Color color in result)
        //    {
        //        _db.Colors.Remove(color);
        //    }

        //    await _db.SaveChangesAsync();

        //    // Finally return the result to client
        //    return Ok(new JsonResult("The Colors of product " + product.Name + " is deleted."));
        //}

        #endregion
    }
}
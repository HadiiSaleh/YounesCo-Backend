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
    public class ImagesController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor
        public ImagesController(ApplicationDbContext db)
        {
            _db = db;
        }

        #endregion

        #region GetImages

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetImages()
        {
            var result = _db.Images.ToList();

            if (result == null) return NotFound(new JsonResult("No Images existed."));

            return Ok(result);

        }

        #endregion

        #region GetImageByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Image>> GetImageByIdAsync([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var image = await _db.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            return Ok(new JsonResult(image));
        }

        #endregion

        #region GetImagesByColorId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetImagesByColorId([FromRoute] int id)
        {
            var result = _db.Images.ToList().Where(f => f.ColorId == id);

            if (result.Count() < 0)
                return NotFound(new JsonResult("No images existed."));

            return Ok(result);

        }

        #endregion

        #region CreateImageAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateImageAsync([FromBody] Image data)
        {
            var newImage = new Image
            {
                ImageSource = data.ImageSource,
                Default = data.Default,
                ColorId = data.ColorId
            };

            await _db.Images.AddAsync(newImage);

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Image was Added Successfully"));

        }

        #endregion

        #region UpdateImageAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateImageAsync([FromRoute] int id, [FromBody] Image image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != image.ImageId)
            {
                return BadRequest();
            }

            var findImage = _db.Images.FirstOrDefault(c => c.ImageId == id);

            if (findImage == null)
            {
                return NotFound();
            }

            // If the image was found
            findImage.ImageSource = image.ImageSource;
            findImage.ColorId = image.ColorId;
            findImage.Default = image.Default;
            findImage.UpdatedAt = DateTime.Now;

            _db.Entry(findImage).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Image with id " + id + " is updated"));
        }

        #endregion

        #region DeleteImageAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Image>> DeleteImageAsync([FromRoute] int id)
        {
            var image = await _db.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            _db.Images.Remove(image);

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Image with id " + id + " is deleted."));
        }

        #endregion

        #region DeleteImagesByColorIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Favorite>> DeleteImagesByColorIdAsync([FromRoute] int id)
        {
            var result = _db.Images.ToList().Where(f => f.ColorId == id);

            Color color = await _db.Colors.FindAsync(id);

            if (result.Count() < 0)
            {
                return NotFound();
            }

            foreach (Image image in result)
            {
                _db.Images.Remove(image);
            }

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Images of color " + color.ColorName + " is deleted."));
        }

        #endregion
    }
}
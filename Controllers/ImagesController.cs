using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;
using YounesCo_Backend.ViewModels;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        #endregion

        #region Constructor
        public ImagesController(ApplicationDbContext db, IWebHostEnvironment env, IMapper mapper)
        {
            _db = db;
            _env = env;
            _mapper = mapper;
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
        public async Task<ActionResult<Image>> GetImageById([FromRoute] int id)
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
        public async Task<IActionResult> UploadProductImages([FromForm] ImageForCreationViewModel images)
        {
            var product = await _db.Products
                .Include(p => p.Color)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == images.productId);

            if (product == null)
            {
                return BadRequest("Product cannot be found");
            }

            var color = await _db.Colors
                .FirstOrDefaultAsync(c => c.ColorId == images.colorId);


            if (color == null)
            {
                return BadRequest("Color cannot be found");
            }

            if (images.files != null && images.files.Count > 0)
            {
                for (int i = 0; i < images.files.Count; i++)
                {

                    var file = images.files[i];
                    var folderName = Path.Combine("Resources", "Products", "Images");

                    try
                    {
                        if (file.Length > 0)
                        {
                            var pathToSave = Path.Combine(_env.WebRootPath, folderName);
                            if (!Directory.Exists(pathToSave))
                            {
                                Directory.CreateDirectory(pathToSave);
                            }
                            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            var dbPath = Path.Combine(folderName, fileName);

                            //var imageUrl = await _db.Images.FirstOrDefaultAsync(i => i.ImageSource.ToLower() == dbPath.ToLower());

                            //if (imageUrl != null)
                            //{
                            //    return Conflict("Image name already exist");
                            //}

                            var fileExtention = Path.GetExtension(dbPath);

                            if (fileExtention.ToLower() == ".jpg" || fileExtention.ToLower() == ".png" || fileExtention.ToLower() == ".jpeg")
                            {

                                using (FileStream fileStream = System.IO.File.Create(pathToSave + "\\" + fileName))
                                {
                                    await file.CopyToAsync(fileStream);
                                    fileStream.Flush();
                                    var imageToSave = new Image()
                                    {
                                        ImageSource = dbPath,
                                        ColorId = color.ColorId,
                                        ProductId = product.ProductId,
                                        ImageName = fileName
                                    };

                                    if (!product.Images.Any(c => c.Default))
                                    {
                                        imageToSave.Default = true;
                                    }

                                    await _db.Images.AddAsync(imageToSave);
                                    var result = await _db.SaveChangesAsync();
                                    if (result < 0)
                                    {
                                        return BadRequest("Failed to save image path");

                                    }
                                }
                            }
                            else
                            {
                                return BadRequest("Support Only files with type jpg or png");
                            }
                        }
                        else
                        {
                            return BadRequest("Failed to upload product image");
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }

                return StatusCode(201, "Images uploaded successfully");
            }
            else
            {
                return BadRequest("You need to insert at least one image per product");
            }

        }

        #endregion

        #region UpdateImageAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateImage([FromRoute] int id, [FromBody] Image image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
        public async Task<ActionResult<Image>> DeleteImage([FromRoute] int id)
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
        public async Task<ActionResult<Favorite>> DeleteImagesByColorId([FromRoute] int id)
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
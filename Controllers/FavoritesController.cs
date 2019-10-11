using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        private readonly UserManager<AppUser> _userManager;

        #endregion

        #region Constructor
        public FavoritesController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        #endregion

        #region GetFavoritesByCustomerId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetFavoritesByCustomerId([FromRoute] string id)
        {
            var result = _db.Favorites.ToList().Where(f => f.CustomerId == id);

            if (result.Count() < 0)
                return NotFound(new JsonResult("No favorites existed."));

            return Ok(result);

        }

        #endregion

        #region GetFavoritesByProductId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetFavoritesByProductId([FromRoute] int id)
        {
            var result = _db.Favorites.ToList().Where(f => f.ProductId == id);

            if (result.Count() < 0)
                return NotFound(new JsonResult("No favorites existed."));

            return Ok(result);

        }

        #endregion

        #region GetFavoriteByIdsAsync

        [HttpGet("[action]/{cid}/{pid}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Favorite>> GetFavoriteByIdsAsync([FromRoute] string cid, [FromRoute] int pid)
        {
            var result = await _db.Favorites.FindAsync(cid, pid);

            if (result == null) return NotFound(new JsonResult("Favorite not existed."));

            return Ok(result);
        }

        #endregion

        #region CreateFavoriteAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> CreateFavoriteAsync([FromBody] Favorite data)
        {
            var newFavorite = new Favorite
            {
                ProductId = data.ProductId,
                CustomerId = data.CustomerId
            };

            await _db.Favorites.AddAsync(newFavorite);

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Favorite was Added Successfully"));

        }

        #endregion

        #region DeleteFavoritesByCustomerIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Favorite>> DeleteFavoritesByCustomerIdAsync([FromRoute] string id)
        {
            var result = _db.Favorites.ToList().Where(f => f.CustomerId == id);

            AppUser customer = await _userManager.FindByIdAsync(id);

            if (result.Count() < 0)
            {
                return NotFound();
            }

            foreach (Favorite favorite in result)
            {
                _db.Favorites.Remove(favorite);
            }

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Favorites of customer " + customer.UserName + " is deleted."));
        }

        #endregion

        #region DeleteFavoritesByProductIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Favorite>> DeleteFavoritesByProductIdAsync([FromRoute] int id)
        {
            var result = _db.Favorites.ToList().Where(f => f.ProductId == id);

            Product produt = await _db.Products.FindAsync(id);

            if (result.Count() < 0)
            {
                return NotFound();
            }

            foreach (Favorite favorite in result)
            {
                _db.Favorites.Remove(favorite);
            }

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Favorites of product " + produt.Name + " is deleted."));
        }

        #endregion

        #region DeleteFavoriteByIdsAsync

        [HttpDelete("[action]/{cid}/{pid}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Favorite>> DeleteFavoriteByIdsAsync([FromRoute] string cid, [FromRoute] int pid)
        {
            var favorite = await _db.Favorites.FindAsync(cid, pid);

            if (favorite == null)
            {
                return NotFound();
            }

            _db.Favorites.Remove(favorite);

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok(new JsonResult("The Favorite is deleted."));
        }

        #endregion
    }
}
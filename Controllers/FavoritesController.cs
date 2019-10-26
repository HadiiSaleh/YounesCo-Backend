using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<Favorite>> GetFavoritesByCustomerId([FromRoute] string id)
        {
            var findUser = await _userManager.FindByIdAsync(id);

            if (findUser == null || findUser.Deleted == true)
            {
                return NotFound();
            }

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != id) return Unauthorized();

            var result = await _db.Favorites
                .Include(fav => fav.Product)
                .Where(f => f.CustomerId == id)
                .ToListAsync();

            if (result.Count() < 0)
                return NotFound(new JsonResult("No favorites existed."));

            return Ok(result);

        }

        #endregion

        #region GetFavoritesByProductId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<ActionResult<Favorite>> GetFavoritesByProductId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var favorites = await _db.Favorites
                .Include(fav => fav.Customer)
                .Where(f => f.ProductId == id)
                .ToListAsync()
                ;

            if (favorites.Count() < 0)
                return NotFound(new JsonResult("No favorites existed."));

            return Ok(favorites);

        }

        #endregion

        #region GetFavoriteByIdsAsync

        [HttpGet("[action]/{cid}/{pid}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Favorite>> GetFavoriteByIds([FromRoute] string cid, [FromRoute] int pid)
        {
            if (cid == null || pid <= 0) return NotFound(new JsonResult("Invalid Id"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != cid) return Unauthorized();

            var result = await _db.Favorites
                .Include(fav => fav.Product)
                .Include(fav => fav.Customer)
                .SingleOrDefaultAsync(fav => fav.CustomerId == cid && fav.ProductId == pid);

            if (result == null) return NotFound(new JsonResult("Favorite not existed."));

            return Ok(result);
        }

        #endregion

        #region CreateFavoriteAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> CreateFavorite([FromBody] Favorite data)
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != data.CustomerId) return Unauthorized();

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
        public async Task<ActionResult<Favorite>> DeleteFavoritesByCustomerId([FromRoute] string id)
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != id) return Unauthorized();

            var result = _db.Favorites.ToList().Where(f => f.CustomerId == id);

            var getCustomerTask = _userManager.FindByIdAsync(id);

            if (result.Count() < 0)
            {
                return NotFound();
            }

            foreach (Favorite favorite in result)
            {
                _db.Favorites.Remove(favorite);
            }

            await _db.SaveChangesAsync();

            var customer = await getCustomerTask;

            // Finally return the result to client
            return Ok(new JsonResult("The Favorites of customer " + customer.UserName + " is deleted."));
        }

        #endregion

        #region DeleteFavoritesByProductIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<ActionResult<Favorite>> DeleteFavoritesByProductId([FromRoute] int id)
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
        public async Task<ActionResult<Favorite>> DeleteFavoriteByIds([FromRoute] string cid, [FromRoute] int pid)
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != cid) return Unauthorized();

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
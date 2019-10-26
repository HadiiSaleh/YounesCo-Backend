using System.IdentityModel.Tokens.Jwt;
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
    public class OrderItemsController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor
        public OrderItemsController(ApplicationDbContext db)
        {
            _db = db;
        }

        #endregion

        #region GetOrderItems

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public IActionResult GetOrderItems()
        {
            var result = _db.OrderItems.ToList();

            if (result == null) return NotFound(new JsonResult("No Orders Items existed."));

            return Ok(result);

        }

        #endregion

        #region GetOrderItemById

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<IActionResult> GetOrderItemById([FromRoute] int id)
        {
            if (id <= 0) return NotFound(new JsonResult("Invalid Id"));

            var result = await _db.OrderItems.FindAsync(id);

            if (result == null) return NotFound(new JsonResult("No Orders Items existed."));

            return Ok(result);

        }

        #endregion

        #region GetItemsByOrderIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<OrderItem>> GetItemsByOrderId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var order = await _db.Orders.FindAsync(id);

            if (order == null || order.Deleted == true) return NotFound(new JsonResult("Order Not Found"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != order.CustomerId) return Unauthorized();

            var items = await _db.OrderItems
                .Include(item => item.Product)
                .Include(item => item.Color)
                    .ThenInclude(color => color.Images)
                .Where(item => item.OrderId == id)
                .ToListAsync()
                ;

            if (items == null || items.Count() <= 0)
            {
                return NotFound(new JsonResult("No Items"));
            }

            return Ok(new JsonResult(items));
        }

        #endregion

        #region GetItemsByProductIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<ActionResult<OrderItem>> GetItemsByProductId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var items = await _db.OrderItems
                .Include(item => item.Order)
                    .ThenInclude(order => order.Customer)
                .Include(item => item.Color)
                    .ThenInclude(color => color.Images)
                .Where(item => item.ProductId == id)
                .ToListAsync()
                ;

            if (items == null || items.Count() <= 0)
            {
                return NotFound(new JsonResult("No Items"));
            }

            return Ok(new JsonResult(items));
        }

        #endregion

        #region CreateOrderItemAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> CreateOrderItem([FromBody] OrderItem data)
        {
            var order = await _db.Orders.FindAsync(data.OrderId);

            if (order == null || order.Deleted == true) return NotFound(new JsonResult("Order Not Found"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != order.CustomerId) return Unauthorized();

            var item = new OrderItem
            {
                ProductId = data.ProductId,
                ColorId = data.ColorId,
                ColorName = data.ColorName,
                UnitPrice = data.UnitPrice,
                TotalPrice = data.TotalPrice,
                Quantity = data.Quantity,
                OrderId = data.OrderId
            };

            var create = await _db.OrderItems.AddAsync(item);

            if (create == null)
                return BadRequest(new JsonResult("Item Creation Failed"));

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Item was Added to Order Successfully"));

        }

        #endregion

        #region UpdateOrderItemAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> UpdateOrderItem([FromRoute] int id, [FromBody] OrderItem data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findItem = await _db.OrderItems
                .Include(i => i.Order)
                .SingleAsync(i => i.OrderItemId == id);

            if (findItem == null)
            {
                return NotFound("Item Not Found");
            }

            if (findItem.Order == null || findItem.Order.Deleted == true) return NotFound(new JsonResult("Order Not Found"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != findItem.Order.CustomerId) return Unauthorized();

            // If the item was found
            findItem.ColorId = data.ColorId;
            findItem.ProductId = data.ProductId;
            findItem.ColorName = data.ColorName;
            findItem.UnitPrice = data.UnitPrice;
            findItem.TotalPrice = data.TotalPrice;
            findItem.Quantity = data.Quantity;
            findItem.OrderId = data.OrderId;

            _db.Entry(findItem).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Item with id " + id + " is updated"));
        }

        #endregion

        #region DeleteOrderItemAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<OrderItem>> DeleteOrderItem([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound(new JsonResult("Invalid Id"));

            }

            var item = await _db.OrderItems
                .Include(i => i.Order)
                .SingleAsync(i => i.OrderItemId == id);

            if (item == null)
            {
                return NotFound(new JsonResult("Item Not Found"));
            }

            if (item.Order == null || item.Order.Deleted == true) return NotFound(new JsonResult("Order Not Found"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != item.Order.CustomerId) return Unauthorized();

            var delete = _db.OrderItems.Remove(item);

            if (delete != null)
            {
                await _db.SaveChangesAsync();

                // Finally return the result to client
                return Ok(new JsonResult("The Item with id " + id + " is deleted."));
            }

            return BadRequest(new JsonResult("The Item is not deleted."));

        }

        #endregion
    }
}
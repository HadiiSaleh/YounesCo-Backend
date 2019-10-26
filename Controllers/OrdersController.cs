using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructor
        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        #endregion

        #region CreateOrderAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> CreateOrder([FromBody] Order data)
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != data.CustomerId) return Unauthorized();

            var order = new Order
            {
                CustomerId = data.CustomerId,
                Deleted = false,
                RequestedOn = data.RequestedOn,
                TotalPrice = data.TotalPrice
            };

            var create = await _db.Orders.AddAsync(order);

            if (create == null)
                return BadRequest(new JsonResult("order Creation Failed"));

            await _db.SaveChangesAsync();

            return Created("", new JsonResult("The Order is added Successfully"));

        }

        #endregion

        #region GetOrders

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetOrders()
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            IList<Order> result = null;

            if (userRoleLoggedIn == "Customer")
            {
                result = _db.Orders
                .Where(o => o.Deleted == false && o.CustomerId == userIdLoggedIn)
                .ToList()
                ;
            }
            else
            {
                result = _db.Orders
                .Where(o => o.Deleted == false)
                .ToList()
                ;
            }


            if (result.Count() <= 0) return NotFound(new JsonResult("No Orders existed."));

            return Ok(result);
        }

        #endregion

        #region GetOrderByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Order>> GetOrderById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest(new JsonResult("Invalid Id!"));

            var findOrder = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(items => items.Product)
                .Include(o => o.Customer)
                .SingleOrDefaultAsync(o => o.OrderId == id && o.Deleted == false)
                ;

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != findOrder.CustomerId) return Unauthorized();

            if (findOrder != null)
                return Ok(new JsonResult(findOrder));

            else
                return NotFound(new JsonResult("Order Not Found"));
        }

        #endregion

        #region GetOrdersByCustomerId

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetOrdersByCustomerId([FromRoute] string id)
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != id) return Unauthorized();

            if (id == null)
                return NotFound(new JsonResult("Invalid Id"));

            var result = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(item => item.Product)
                .Include(o => o.Customer)
                .Where(o => o.Deleted == false && o.CustomerId == id)
                .ToListAsync()
                ;

            if (result.Count() <= 0) return NotFound(new JsonResult("No orders of this customer existed."));

            return Ok(result);
        }

        #endregion

        #region UpdateOrderAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> UpdateOrder([FromRoute] int id, [FromBody] Order data)
        {
            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != data.CustomerId) return Unauthorized();

            if (id <= 0) return NotFound(new JsonResult("Invalid Id"));

            var order = await _db.Orders.FindAsync(id);

            if (order == null || order.Deleted == true) return NotFound(new JsonResult("Order Not Found"));

            order.CustomerId = data.CustomerId;
            order.Deleted = data.Deleted;
            order.RequestedOn = data.RequestedOn;
            order.TotalPrice = data.TotalPrice;

            _db.Entry(order).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Order is Updated Successfully"));

        }

        #endregion

        #region DeleteOrderAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        {
            if (id <= 0) return NotFound(new JsonResult("Invalid Id"));

            var order = await _db.Orders.FindAsync(id);

            if (order == null || order.Deleted == true) return NotFound(new JsonResult("Order Not Found"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != order.CustomerId) return Unauthorized();

            order.Deleted = true;

            _db.Entry(order).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Order is Deleted Successfully"));

        }

        #endregion

        #region UnDeleteOrderAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> UnDeleteOrder([FromRoute] int id)
        {
            if (id <= 0) return NotFound(new JsonResult("Invalid Id"));

            var order = await _db.Orders.FindAsync(id);

            if (order == null) return NotFound(new JsonResult("Order Not Found"));

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
            var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != order.CustomerId) return Unauthorized();

            order.Deleted = false;

            _db.Entry(order).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Order is UnDeleted Successfully"));

        }

        #endregion
    }
}
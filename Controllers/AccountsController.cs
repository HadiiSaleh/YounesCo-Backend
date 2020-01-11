using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YounesCo_Backend.Data;
using YounesCo_Backend.Email;
using YounesCo_Backend.Helpers;
using YounesCo_Backend.Models;
using YounesCo_Backend.Services;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        #region Attributes

        private readonly ApplicationDbContext _db;

        private readonly UserManager<AppUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly SignInManager<AppUser> _signManager;

        private readonly AppSettings _appSettings;

        private readonly IEmailSender _emailsender;

        private readonly ExceptionsLoggerService _exceptionLogger;

        private readonly DataCleaner _dataCleaner;

        #endregion

        #region Constructor 
        public AccountsController(ApplicationDbContext db, UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, DataCleaner dataCleaner,
            IOptions<AppSettings> appSettings, IEmailSender emailsender, ExceptionsLoggerService exceptionLogger)
        {
            _db = db;
            _userManager = userManager;
            _signManager = signInManager;
            _roleManager = roleManager;
            _appSettings = appSettings.Value;
            _emailsender = emailsender;
            _exceptionLogger = exceptionLogger;
            _dataCleaner = dataCleaner;
        }

        #endregion

        #region Register

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel registerViewModel)
        {
            // Will hold all the errors related to registration
            List<string> errorList = new List<string>();

            var user = new AppUser
            {
                Email = registerViewModel.Email,
                UserName = registerViewModel.UserName,
                PhoneNumber = registerViewModel.PhoneNumber,
                FirstName = registerViewModel.FirstName,
                MiddleName = registerViewModel.MiddleName,
                LastName = registerViewModel.LastName,
                DisplayName = registerViewModel.MiddleName != null ? registerViewModel.FirstName + " " + registerViewModel.MiddleName + " " + registerViewModel.LastName : registerViewModel.FirstName + " " + registerViewModel.LastName,
                Location = registerViewModel.Location,
                Deleted = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);

            var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString();

            if (jwt == "") registerViewModel.Role = "Customer";

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, registerViewModel.Role);

                //Sending Comfirmation Email
                try
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var callbackUrl = Url.Action("ConfirmEmail", "Accounts", new { UserId = user.Id, Code = code }, protocol: HttpContext.Request.Scheme);

                    var send = await _emailsender.SendEmailAsync(user.Email, "younesco.com - Confirm Your Email", "Please confirm your e-mail by clicking this link: <a href=\"" + callbackUrl + "\">click here</a>");

                    return Created("", new JsonResult(registerViewModel));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    errorList.Add(ex.Message);
                    _exceptionLogger.saveExceptionLog("Email Sender", "Accounts Register", ex.Message);
                    return Created("", new JsonResult(errorList));
                }

            }

            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    errorList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));

        }

        #endregion

        #region Login

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
            // Get the User from Database
            var user = await _userManager.FindByNameAsync(loginViewModel.UserName);

            if (user == null || user.Deleted) return Unauthorized(new { LoginError = "Please check the login credentials - Invalid Username/Password was entered" });

            var checkPasswordTask = _userManager.CheckPasswordAsync(user, loginViewModel.Password);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            double tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null && await checkPasswordTask)
            {
                // THen Check If Email Is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    try
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        var callbackUrl = Url.Action("ConfirmEmail", "Accounts", new { UserId = user.Id, Code = code }, protocol: HttpContext.Request.Scheme);

                        await _emailsender.SendEmailAsync(user.Email, "younesco.com - Confirm Your Email", "Please confirm your e-mail by clicking this link: <a href=\"" + callbackUrl + "\">click here</a>");

                        ModelState.AddModelError(string.Empty, "User Has not Confirmed Email.");

                        return Unauthorized(new { LoginError = "We sent you an Confirmation Email. Please Confirm Your Registration With younesco.com To Log in." });

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "User Has not Confirmed Email.");
                        ModelState.AddModelError("", ex.Message);
                        _exceptionLogger.saveExceptionLog("Email Sender", "Accounts Login", ex.Message);
                        return Unauthorized(new { LoginError = "Email Not Confirmed. Failed to send a new confirmation email.Email Sender Response: " + ex.Message + "." });
                    }

                }

                var roles = await _userManager.GetRolesAsync(user);

                if (roles == null) { return Unauthorized(new { LoginError = "Invalid Role!" }); }

                // Generate Token

                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, loginViewModel.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString()),

                     }),

                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Site,
                    Audience = _appSettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new { token = tokenHandler.WriteToken(token), expiration = token.ValidTo, username = user.UserName, userRole = roles.FirstOrDefault() });

            }

            // return error
            ModelState.AddModelError("", "Username/Password was not Found");
            return Unauthorized(new { LoginError = "Please Check the Login Credentials - Ivalid Username/Password was entered" });

        }

        #endregion

        #region ConfirmEmail

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);

            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new JsonResult("User Invalid");
            }

            if (user.EmailConfirmed)
            {
                return Redirect("/login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Redirect("/email-confirmed");
                //return RedirectToAction("EmailConfirmed", "Notifications", new { userId, code });

            }

            else
            {
                List<string> errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.ToString());
                }
                return new JsonResult(errors);
            }
        }

        #endregion

        #region ForgotPassword

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordEmail data)
        {
            if (data == null) return new JsonResult("Input Invalid");

            string email = data.email;

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                try
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var callbackUrl = Url.Action("ResetPasswordEmail", "Accounts", new { UserId = user.Id, Code = code }, protocol: HttpContext.Request.Scheme);

                    var send = await _emailsender.SendEmailAsync(user.Email, "younesco.com - Reset Your Password", "Please reset your password by clicking this link: <a href=\"" + callbackUrl + "\">click here</a>");

                    return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Reset Password Email Was Sent To " + email + " !" });
                }
                catch (Exception ex)
                {
                    _exceptionLogger.saveExceptionLog(user.Id, "Account Forgot Password", ex.Message);
                    return BadRequest(new JsonResult("Failed to send password reset token Email. Response : " + ex.Message));
                }

            }

            return NotFound(new JsonResult("Email Not Found!"));
        }

        #endregion

        #region ResetPasswordEmail

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);

            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new JsonResult("Invalid User!"));
            }

            return Ok(new JsonResult("Succeeded!"));
        }

        #endregion

        #region ResetPassword

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel data)
        {
            if (data == null) return BadRequest(new JsonResult("Input Invalid"));

            var user = await _userManager.FindByIdAsync(data.Id);

            if (user != null)
            {

                var result = await _userManager.ResetPasswordAsync(user, data.Code, data.Password);

                if (result.Succeeded) return Ok(new JsonResult("Password reset succeeded!"));

            }

            return BadRequest(new JsonResult("Reset Failed"));
        }

        #endregion

        #region GetUsersByRoleAsync

        [HttpGet("[action]/{data}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public IActionResult GetUsersByRole([FromRoute] string data)
        {
            if (data == null || _roleManager.RoleExistsAsync(data).Result == false)
                return BadRequest(new JsonResult("Please Provide a valid role"));

            var result = (
                from user in _db.Users
                join userRole in _db.UserRoles
                on user.Id equals userRole.UserId
                join role in _db.Roles
                on userRole.RoleId equals role.Id
                where role.Name == data
                where user.Deleted == false
                select new BaseUser
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    Location = new Address
                    {
                        Country = user.Location.Country,
                        City = user.Location.City,
                        Region = user.Location.Region,
                        Street = user.Location.Street,
                        Building = user.Location.Building,
                        Floor = user.Location.Floor
                    },

                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Deleted = user.Deleted,
                    role = role.Name,

                }
                ).ToList();

            if (result.Count() > 0)
                return Ok(result);

            else
                return BadRequest(new JsonResult("No " + data + "s to show"));
        }

        #endregion

        #region GetAllUsersAsync

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        //[Obsolete]
        public IActionResult GetAllUsers()
        {
            var users = (
                from user in _db.Users
                join userRole in _db.UserRoles
                on user.Id equals userRole.UserId
                join role in _db.Roles
                on userRole.RoleId equals role.Id
                where user.Deleted == false
                select new BaseUser
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    Location = new Address
                    {
                        Country = user.Location.Country,
                        City = user.Location.City,
                        Region = user.Location.Region,
                        Street = user.Location.Street,
                        Building = user.Location.Building,
                        Floor = user.Location.Floor
                    },

                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Deleted = user.Deleted,
                    role = role.Name,

                }
                ).ToList();

            // Clean data

            var result = _dataCleaner.cleanAllUsersListBasedOnRoles(users);

            if (result.Count() == 0 || result == null)
                return BadRequest(new JsonResult("No users to show"));

            else
                return Ok(result);

        }

        #endregion

        #region GetUserByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            if (id == null) return BadRequest(new JsonResult("NULL Id!"));

            var user = await _db.Users
                .Include(user => user.Favorites)
                 .ThenInclude(fav => fav.Product)
                .Include(user => user.Orders)
                .SingleOrDefaultAsync(u => u.Id == id)
                ;

            if (user != null && user.Deleted == false)
            {
                var jwt = HttpContext.Request.Headers.FirstOrDefault(c => c.Key == "Authorization").Value.ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userIdLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "nameid").Value.ToString();
                var userRoleLoggedIn = token.Payload.SingleOrDefault(p => p.Key == "role").Value.ToString();

                if (userRoleLoggedIn == "Customer" && userIdLoggedIn != user.Id) return Unauthorized();

                var userRole = await _userManager.GetRolesAsync(user);

                var result = new BaseUser
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Location = user.Location,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Deleted = user.Deleted,
                    role = userRole[0],
                    Orders = user.Orders,
                    Favorites = _dataCleaner.cleanFavorites(user.Favorites)
                };

                return Ok(result);

            }
            else
                return BadRequest(new JsonResult("User with id " + id + " not found"));
        }

        #endregion

        #region UpdateUserByIdAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> UpdateUserById([FromRoute] string id, [FromBody] BaseUser user)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null) return BadRequest(new JsonResult("NULL Id!"));

            //here we will hold all the errors of registration
            List<string> errorList = new List<string>();

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

            if (userRoleLoggedIn == "Customer" && userIdLoggedIn != findUser.Id) return Unauthorized();

            // If the user was found

            findUser.Email = user.Email;
            findUser.UserName = user.UserName;
            findUser.PhoneNumber = user.PhoneNumber;
            findUser.FirstName = user.FirstName;
            findUser.MiddleName = user.MiddleName;
            findUser.LastName = user.LastName;
            findUser.DisplayName = user.FirstName + " " + user.MiddleName + " " + user.LastName;
            findUser.Location = user.Location;
            findUser.UpdatedAt = DateTime.Now;

            var updateUser = await _userManager.UpdateAsync(findUser);

            if (updateUser.Succeeded)
            {
                await _db.SaveChangesAsync();

                return Ok(new JsonResult(user));
            }

            else
            {
                foreach (var error in updateUser.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    errorList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));
        }

        #endregion

        #region ChangeRoleAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<IActionResult> ChangeRole([FromRoute] string id, [FromBody] string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null || role == null) return BadRequest(new JsonResult("NULL data!"));

            //find user's account
            var findUser = await _userManager.FindByIdAsync(id);

            if (findUser == null || findUser.Deleted == true) return NotFound(new JsonResult("User Not Found Or Already Deleted"));

            var findRole = await _roleManager.RoleExistsAsync(role);

            if (findRole == false) return NotFound(new JsonResult("Role Not Found"));

            if (await _userManager.IsInRoleAsync(findUser, role)) return Ok(new JsonResult("User is already in this role"));

            var userRole = await _userManager.GetRolesAsync(findUser);

            await _userManager.RemoveFromRolesAsync(findUser, userRole);

            var changeRole = await _userManager.AddToRoleAsync(findUser, role);

            if (changeRole.Succeeded)
                return Ok(new JsonResult("The user with username " + findUser.UserName + "'s role is changed to " + role));

            return BadRequest(new JsonResult("Role Change is Failed!"));
        }

        #endregion

        #region DeleteUserByIdAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<IActionResult> DeleteUserById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null) return BadRequest(new JsonResult("NULL Id!"));

            //find user's account
            var findUser = await _userManager.FindByIdAsync(id);

            if (findUser == null || findUser.Deleted == true) return NotFound(new JsonResult("User Not Found Or Already Deleted"));

            //delete user's account
            findUser.Deleted = true;
            findUser.UpdatedAt = DateTime.Now;

            var deleteuser = await _userManager.UpdateAsync(findUser);

            if (deleteuser.Succeeded)
            {
                await _db.SaveChangesAsync();
                return Ok(new JsonResult("The user with username " + findUser.UserName + " is Deleted"));
            }

            return BadRequest("Delete Failed!");
        }

        #endregion

        #region UnDelteUserByIdAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<IActionResult> UnDelteUserById([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null) return BadRequest(new JsonResult("NULL Id!"));

            //find user's account
            var findUser = await _userManager.FindByIdAsync(id);

            if (findUser == null || findUser.Deleted == false) return NotFound(new JsonResult("User Not Found Or Already Active"));

            //delete user's account
            findUser.Deleted = false;
            findUser.UpdatedAt = DateTime.Now;

            var unDeleteuser = await _userManager.UpdateAsync(findUser);

            if (unDeleteuser.Succeeded)
            {
                await _db.SaveChangesAsync();
                return Ok(new JsonResult("The user with username " + findUser.UserName + " is Undeleted"));
            }

            return BadRequest(new JsonResult("Undelete Failed!"));
        }

        #endregion
    }
}
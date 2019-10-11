using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        #endregion

        #region Constructor 
        public AccountsController(ApplicationDbContext db, UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager,
            IOptions<AppSettings> appSettings, IEmailSender emailsender, ExceptionsLoggerService exceptionLogger)
        {
            _db = db;
            _userManager = userManager;
            _signManager = signInManager;
            _roleManager = roleManager;
            _appSettings = appSettings.Value;
            _emailsender = emailsender;
            _exceptionLogger = exceptionLogger;
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
                PhoneNumber = registerViewModel.Phone,
                FirstName = registerViewModel.FirstName,
                MiddleName = registerViewModel.MiddleName,
                LastName = registerViewModel.LastName,
                DisplayName = registerViewModel.FirstName + " " + registerViewModel.MiddleName + " " + registerViewModel.LastName,
                Location = registerViewModel.Location,
                Deleted = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, registerViewModel.Role);

                //Sending Comfirmation Email
                try
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var callbackUrl = Url.Action("ConfirmEmail", "Accounts", new { UserId = user.Id, Code = code }, protocol: HttpContext.Request.Scheme);

                    var send = await _emailsender.SendEmailAsync(user.Email, "younesco.com - Confirm Your Email", "Please confirm your e-mail by clicking this link: <a href=\"" + callbackUrl + "\">click here</a>");

                    return Created("", new { username = user.UserName, email = user.Email, status = 1, message = "Registration Successful" });
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

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            double tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginViewModel.Password))
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
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
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

            return new JsonResult("Email Does Not Exists!");
        }

        #endregion

        #region ResetPasswordEmail

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordEmail(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);

            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new JsonResult("User Invalid");
            }

            return Redirect("/ResetPassword");
        }

        #endregion

        #region ResetPassword

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel data)
        {
            if (data == null) return new JsonResult("Input Invalid");

            var user = await _userManager.FindByIdAsync(data.Id);

            if (user != null)
            {

                var result = await _userManager.ResetPasswordAsync(user, data.Code, data.Password);

                if (result.Succeeded)
                {

                    return RedirectToAction("PasswordSet", "Notifications", new { data.Id, data.Code });

                }
            }

            return new JsonResult("Reset Failed");
        }

        #endregion

        #region GetUsersByRoleAsync

        [HttpGet("[action]/{role}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<IActionResult> GetUsersByRoleAsync([FromRoute] string role)
        {

            if (role == null || _roleManager.RoleExistsAsync(role).Result == false) return BadRequest(new JsonResult("Please Provide a valid role"));

            var result = await _userManager.GetUsersInRoleAsync(role);

            foreach (var user in result)
            {
                if (user.Deleted == true) result.Remove(user);
            }

            if (result.Count() > 0)
                return Ok(result);

            else
                return BadRequest(new JsonResult("No " + role + "s to show"));
        }

        #endregion

        #region GetUserByIdAsync

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {

            if (id == null) return BadRequest(new JsonResult("Null Id!"));

            var result = await _userManager.FindByIdAsync(id);

            if (result != null && result.Deleted == false)
                return Ok(result);

            else
                return BadRequest(new JsonResult("User with id " + id + " not found"));
        }

        #endregion

        #region UpdateUserByIdAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> UpdateUserByIdAsync([FromRoute] string id, [FromBody] RegisterViewModel user)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null) return BadRequest(new JsonResult("Null Id!"));

            //here we will hold all the errors of registration
            List<string> errorList = new List<string>();

            var findUser = await _userManager.FindByIdAsync(id);

            if (findUser == null || findUser.Deleted == true)
            {
                return NotFound();
            }

            // If the user was found

            findUser.Email = user.Email;
            findUser.UserName = user.UserName;
            findUser.PhoneNumber = user.Phone;
            findUser.FirstName = user.FirstName;
            findUser.MiddleName = user.MiddleName;
            findUser.LastName = user.LastName;
            findUser.DisplayName = user.FirstName + " " + user.MiddleName + " " + user.LastName;
            findUser.Location = user.Location;
            findUser.UpdatedAt = DateTime.Now;

            var updateUser = await _userManager.UpdateAsync(findUser);

            if (updateUser.Succeeded)
            {

                IList<string> roles = await _userManager.GetRolesAsync(findUser);

                if (roles.Count() > 0)
                    return Ok(new JsonResult(roles[0] + " " + user.UserName + " is updated successfuly."));

                return Ok(new JsonResult("user " + user.UserName + " is updated successfuly."));
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

        #region DeleteUserByIdAsync

        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdminModeratorRole")]
        public async Task<IActionResult> DeleteUserByIdAsync([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id == null) return BadRequest(new JsonResult("Null Id!"));

            //find user's account
            var findUser = await _userManager.FindByIdAsync(id);

            if (findUser == null || findUser.Deleted == true) return NotFound();

            //delete doctor's account
            findUser.Deleted = true;
            findUser.UpdatedAt = DateTime.Now;

            var deleteuser = await _userManager.UpdateAsync(findUser);

            if (deleteuser.Succeeded)
                return Ok(new JsonResult("The user with username " + findUser.UserName + " is Deleted"));

            return BadRequest("Delete Failed!");
        }

        #endregion
    }
}
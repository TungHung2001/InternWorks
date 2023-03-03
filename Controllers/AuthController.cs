using DatacontextTest2;
using IdentityTest2.DTO;
using IdentityTest2.Email;
using IdentityTest2.Entities;
using IdentityTest2.Interface;
using IdentityTest2.Model;
using IdentityTest2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IdentityTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        /*private readonly JwtService jwtService;*/
        private readonly TokenService tokenService;
        private readonly SchoolContext schoolContext;
        private readonly EmailSender emailSender;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            TokenService tokenService, SchoolContext schoolContext, EmailSender emailSender, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.schoolContext = schoolContext;
            this.emailSender = emailSender;
            this.roleManager = roleManager;
        }

        /*public AuthController(UserManager<IdentityUser> userManager, SchoolContext schoolContext)
        {
            this.userManager = userManager;
            *//*this.jwtService = jwtService;*//*
            this.schoolContext = schoolContext;
        }*/



        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> login(LoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized("Invalid Email");

            /*if(user.UserName == "jim2") user.EmailConfirmed= true;*/

            if(!user.EmailConfirmed) return Unauthorized("Email not confirmed");

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            var userRole = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                /*new Claim(JwtRegisteredClaimNames)*/
            };
            foreach(var role in userRole)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (result.Succeeded)
            {
                await SetRefreshToken(user);
                return await CreateUserObject(user);

            }

            return Unauthorized("Invalid password");

        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> register(RegisterDto registerDto)
        {
            if (await userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
            {
                return BadRequest("Username is already taken");
            }

            if (await userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
            {
                return BadRequest("Email is already taken");
            }
            var user = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,

            };
            var result = await userManager.CreateAsync(user, registerDto.Password);

            userManager.AddToRoleAsync(user, "User");

            /*schoolContext.UserRoles.Add()*/
            //add role
            /*await userManager.AddToRoleAsync(user, role);*/


            /* if (!result.Succeeded)
             {
                 return BadRequest("Problem registering user");
             }

             if (result.Succeeded)
             {
                 return CreateUserObject(user);
             }*/

            /*return BadRequest(result.Errors);*/
            if (!result.Succeeded) return BadRequest("Problem register user!!");

            /* var origin = Request.Headers["origin"];*/
            var origin = $"{Request.Scheme}://{Request.Host}:{Request.Host.Port ?? 80}";
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var verifyUrl = $"{origin}/api/Auth/verifyEmail?token={token}&email={user.Email}";
            var message = $"<p> please click the below  link to verify your email:</p><p><a href='{verifyUrl}'>Click to verify email</a></p>";

            await emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return Ok("Register success - please verify email");

        }
        [AllowAnonymous]
        [HttpGet("verifyEmail")]
        public async Task<IActionResult> verifyEmail(string token, string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)  return Unauthorized();
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded) return BadRequest("Could not verify email");

            return Ok("Email confirmed - you can login");
        }


        [AllowAnonymous]
        [HttpGet("resendEmailConfirmationLink")]
        public async Task<IActionResult> ResendEmailConfirmation(string email) 
        {
            var user = await userManager.FindByEmailAsync(email);

            if(user == null) return Unauthorized();

            var origin = Request.Headers["origin"];
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";
            var message = $"<p> please click the below  link to verify your email:</p><p><a href='{verifyUrl}'>Click to verify email</a></p>";

            await emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return Ok("Email verify resent link ");
        }



        private async Task SetRefreshToken(AppUser appUser)
        {
            var refreshToken = tokenService.RefreshToken(appUser);
            appUser.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(appUser);
            var cookieOption = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOption);
        }
        private async Task<UserDto> CreateUserObject(AppUser user)
        {
            var UserRole = await userManager.GetRolesAsync(user);
            return new UserDto
            {
                DisplayName = user.UserName,
                /*Image = null,*/
                Token = tokenService.CreateToken(user, (List<string>)UserRole),
                Username = user.UserName,
                RefreshToken = user.RefreshTokens.FirstOrDefault()?.Token

            };
        }
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<UserDto>> RefreshToken(TokenApiModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;
            var principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
          
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }
            string username = principal.Identity.Name;
            var user = await userManager.FindByNameAsync(username);
            //var userRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken);
            var userRefreshToken = await userManager.Users.Include(x => x.RefreshTokens).SelectMany(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (user == null || userRefreshToken == null || userRefreshToken.ExpirationDate <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            /*var newAccessToken = TokenService.CreateToken(principal.Claims.ToList());*/

            await SetRefreshToken(user);
            var newToken =  CreateUserObject(user);

            /*user.RefreshTokens = newRefreshToken;*/
            /* await userManager.UpdateAsync(user);*/
            /* var tokenHandler = new JwtSecurityTokenHandler();*/
            /* return new ObjectResult(new
             {
                 accessToken = new JwtSecurityTokenHandler().WriteToken(newToken),
                 refreshToken = newToken
             });*/

            /*return Unauthorized();*/

            return await newToken;

        }

        /*[AllowAnonymous]*/
        //[Authorize(AuthenticationSchemes = "Bearer")]
        /*[Authorize(Roles = "User")]*/
        //[Authorize]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        [HttpGet("getName")]
        public string getName ()
        {
            return "hung";
        }



        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Hr, Editor")]
        [HttpGet("getBatman")]
        public string getBatman()
        {
            return "NGuoi doi";
        }

    }
}

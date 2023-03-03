using DatacontextTest2;
using IdentityTest2.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController: ControllerBase
    {
        private readonly SchoolContext _context;

        private readonly UserManager<AppUser> userManager;

        private readonly RoleManager<IdentityRole> roleManager;

        private readonly ILogger<SetupController> logger;

        public SetupController(SchoolContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<SetupController> logger)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllRoles()
        {
            var roles = roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            var roleExist = await roleManager.RoleExistsAsync(name);
            if (!roleExist) 
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(name));

                if(roleResult.Succeeded) 
                {
                    logger.LogInformation($"The Role{name} has been added successfully");
                    return Ok(new
                    {
                        result = $"the role {name} has been added successfully"
                    });
                }
            }
                else
                {
                    logger.LogInformation($"The Role{name} has not added ");
                    return BadRequest(new
                    {
                        error = $"the role {name} has not been added"
                    });
                }


            return BadRequest(new {error = "Role Already exit"});
        }

        [HttpGet]
        [Route("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await userManager.Users.ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        [Route("AddUserRole")]
        public async Task<IActionResult> AddUserRole(string Id, string roleName)
        {
            var user = await userManager.FindByIdAsync(Id);

            if (user == null)
            {
                logger.LogInformation($"The user with {Id} does not exist ");
                return BadRequest(new
                {
                    error = "User does exist"
                });
            }

            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist) 
            {
                logger.LogInformation($"The role {Id} does not exist ");
                return BadRequest(new
                {
                    error = "Role does exist"
                });
            }

            var result = await userManager.AddToRoleAsync(user, roleName);

            if(result.Succeeded)
            {
                return Ok(new{
                        result = "Success, user has been added  to the role"
                    });
                
            }
            else
            {
                logger.LogInformation($"The user was not be able to be added the role");
                return BadRequest(new
                {
                    error = "The user was not be able to be added the role"
                });
            }
        }
        [HttpGet]
        [Route("GetUserRole")]
        public async Task<IActionResult> GetUserRoles (string Id)
        {
            var user =await userManager.FindByIdAsync(Id); 
            if (user == null) 
            {
                logger.LogInformation($"The user with the {Id} does not exist ");
                return BadRequest(new
                {
                    error = "User does not exist"
                });
            }

            var roles = await userManager.GetRolesAsync(user);

            return Ok(roles);
        }

        [HttpPost]
        [Route("RemoveUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(string Id, string roleName)
        {
            var user = await userManager.FindByIdAsync(Id);

            if (user == null)
            {
                logger.LogInformation($"The user with {Id} does not exist ");
                return BadRequest(new
                {
                    error = "User does exist"
                });
            }

            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                logger.LogInformation($"The role {Id} does not exist ");
                return BadRequest(new
                {
                    error = "Role does exist"
                });
            }

            var result = await userManager.RemoveFromRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = $"User {Id} has been remove role {roleName}"
                });
            }

            return BadRequest(new
            {
                error = $"Unable to remove User{Id} from role {roleName}"
            });
        }

    }
}

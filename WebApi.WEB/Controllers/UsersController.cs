using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DAL.EF;
using WebApi.DAL.Entities;
using WebApi.WEB.Models;

namespace WebApi.WEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<User> userManager, SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            return await _userManager.Users.ToListAsync();
        }

        [HttpGet]
        [Route("{name}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetById(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(user));
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new User { UserName = model.Username, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                // установка куки
                await _signInManager.SignInAsync(user, false);
                return Ok("User created and SignIn");
            }

            return BadRequest("Error register");

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (User.Identity.IsAuthenticated)
                return BadRequest("User is already authenticated. Logout to change user.");

            if (!ModelState.IsValid)
                return BadRequest("Wrong login input!");

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, false);

            if (!result.Succeeded)
                return BadRequest("Wrong login and/or password!");

            return Ok("Logged out successfully.");
        }

        [HttpPost]
        [Route("addFirstAdminAndRoles")]
        public async Task<IActionResult> Login()
        {
            if (_roleManager.Roles.Count(x => x.Name == "admin") == 0 &&
                _roleManager.Roles.Count(x => x.Name == "writer") == 0 &&
                _roleManager.Roles.Count(x => x.Name == "reader") == 0)
            {
                await _roleManager.CreateAsync(new IdentityRole("admin"));
                await _roleManager.CreateAsync(new IdentityRole("writer"));
                await _roleManager.CreateAsync(new IdentityRole("reader"));
            }
            
            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var user = new User { UserName = "admin", Email = "admin@mail.ru" };

                await _userManager.CreateAsync(user, "-$C8DjnDlob");
                await _userManager.AddToRoleAsync(user, "admin");
                return Ok("admin has been added successfuly");
            }
            return Ok("admin already exists");
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            // deleting Cookie
            await _signInManager.SignOutAsync();
            return Ok("Logged out successfully.");
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        [Route("{name}")]
        public async Task<ActionResult> Delete(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user == null)
                return BadRequest("error");
            await _userManager.DeleteAsync(user);
            return Ok($"user {name} deleated");
        }



        //
        // RolesController
        //
        [HttpPost]
        [Authorize(Roles = "admin")]
        [Route("role")]
        public async Task<IActionResult> AddUserRole([FromBody] string userName, string newRole)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return BadRequest("error");
            await _userManager.AddToRoleAsync(user, newRole);
            return Ok($"Added user role successfully.");
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        [Route("role")]
        public async Task<IActionResult> RemoveUserRole([FromBody] string userName, string role)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return BadRequest("error");
            await _userManager.RemoveFromRoleAsync(user, role);
            return Ok($"Removed user role successfully.");
        }
    }
}

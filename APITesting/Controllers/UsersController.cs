using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITesting.Models;
using APITesting.Services;
using APITesting.Models.DTOs;

namespace APITesting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                await _userService.createUser(user);
                return Ok("User created successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return Ok(await _userService.getUsers());
        }

        // GET: api/Users/username
        [HttpGet("{username}")]
        public async Task<ActionResult<User>> GetUser(string username)
        {
            var user = await _userService.getUser(username);

            if (user == null)
                return NotFound("User doesn't exist.");

            return Ok(user);
        }

        // PUT: api/Users/5
        [HttpPatch("{username}")]
        public async Task<IActionResult> PutUser(string username, [FromBody] UserDTO user)
        {
            try
            {
                await _userService.updateUser(username, user);
                return Ok("User updated successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);

            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                await _userService.deleteUser(username);
                return Ok("User deleted successfully.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

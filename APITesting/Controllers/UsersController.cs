using APITesting.Models;
using APITesting.Models.DTOs;
using APITesting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APITesting.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        #region Anonymous endpoints
        // POST: api/users/create
        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<ActionResult<User>> CreateUser(UserDTO user)
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

        // POST: api/users/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginDTO user)
        {
            try
            {
                bool loginSuccess = await _userService.login(user);
                if (loginSuccess)
                {
                    var token = _userService.generateToken(user.Username);
                    return Ok(token);
                } else 
                    return Unauthorized("Invalid password.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Secure endpoints

        //obtain username from JWT token
        private string? GetUsernameFromToken()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        //we don't really need this one anymore
        // GET: api/users
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        //{
        //    return Ok(await _userService.getUsers());
        //}

        // GET: api/users/information
        [HttpGet("information")]
        public async Task<ActionResult<User>> GetUser()
        {
            try
            {
                //obtain username from JWT token and use it to get user information
                string username = GetUsernameFromToken() ?? throw new InvalidOperationException("User not authenticated.");
                var user = await _userService.getUser(username);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }

        }

        // PATCH: api/users/update
        [HttpPatch("update")]
        public async Task<IActionResult> PutUser(UserDTO user)
        {
            try
            {
                string username = GetUsernameFromToken() ?? throw new InvalidOperationException("User not authenticated.");
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

        // DELETE: api/users/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                string username = GetUsernameFromToken() ?? throw new InvalidOperationException("User not authenticated.");
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

        #endregion
    }
}

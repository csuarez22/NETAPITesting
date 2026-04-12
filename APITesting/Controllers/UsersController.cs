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
        private string? _tokenUsername => User.FindFirst(ClaimTypes.Name)?.Value;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        #region Anonymous endpoints
        // POST: api/users/create
        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<ActionResult> CreateUser(UserDTO user)
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
        public async Task<ActionResult> Login(LoginDTO user)
        {
            try
            {
                bool loginSuccess = await _userService.login(user);
                if (loginSuccess)
                {
                    var accessToken = _userService.generateAccessToken(user.Username);
                    var refreshToken = await _userService.generateRefreshToken(user.Username);
                    return Ok(new 
                    { 
                        accessToken, 
                        refreshToken 
                    });
                } else 
                    return Unauthorized("Invalid password.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/users/refresh
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh(RefreshTokenDTO dto)
        {
            try
            {
                var accessToken = _userService.refreshAccessToken(dto.Token);
                return Ok(new { accessToken });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Secure endpoints

        // POST: api/users/logout
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                string username = _tokenUsername ?? throw new InvalidOperationException("User not authenticated.");
                await _userService.invalidateRefreshToken(username);
                return Ok("Logged out successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
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
                string username = _tokenUsername ?? throw new InvalidOperationException("User not authenticated.");
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
        public async Task<ActionResult> PutUser(UserDTO user)
        {
            try
            {
                string username = _tokenUsername ?? throw new InvalidOperationException("User not authenticated.");
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
        public async Task<ActionResult> DeleteUser()
        {
            try
            {
                string username = _tokenUsername ?? throw new InvalidOperationException("User not authenticated.");
                await _userService.deleteUser(username);
                await _userService.invalidateRefreshToken(username); //invalidate refresh token upon account deletion
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

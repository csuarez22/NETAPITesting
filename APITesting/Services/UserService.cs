using APITesting.Models;
using APITesting.Models.DTOs;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APITesting.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        #region Token Generation

        public string generateAccessToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //access token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpiry"])), //token expires in 15mins
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> generateRefreshToken(string username)
        {
            //invalidate any existing refreshTokens for this user
            var existingTokens = await _db.RefreshTokens
                .Where(rt => rt.Username == username)
                .ToListAsync();

            //technically we only have one refreshToken at the time
            _db.RemoveRange(existingTokens);

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Username = username,
                ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiry"])), //refresh token expires in 7 days
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return refreshToken.Token;
        }

        public async Task<string> refreshAccessToken(string refreshToken)
        {
            var existingToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingToken == null)
                throw new ArgumentException("Invalid refresh token.");

            if (existingToken.ExpiryDate < DateTime.UtcNow)
            {
                _db.RefreshTokens.Remove(existingToken);
                await _db.SaveChangesAsync();
                throw new ArgumentException("Refresh token has expired.");
            }

            return generateAccessToken(existingToken.Username);
        }

        //invalidate all tokens when user logs out 
        public async Task invalidateRefreshToken(string username)
        {
            var existingTokens = await _db.RefreshTokens
                .Where(rt => rt.Username == username)
                .ToListAsync();

            _db.RemoveRange(existingTokens);
            await _db.SaveChangesAsync();
        }

        #endregion

        public async Task createUser(UserDTO user)
        {
            try
            {
                #region Validations

                if (string.IsNullOrWhiteSpace(user.Username))
                    throw new ArgumentException("Username is required.");

                if (await _db.Users.AnyAsync(u => u.Username == user.Username))
                    throw new ArgumentException("User already exists.");

                if (string.IsNullOrWhiteSpace(user.Password))
                    throw new ArgumentException("Password is required.");

                if (string.IsNullOrWhiteSpace(user.Email))
                    throw new ArgumentException("Email is required.");

                #endregion

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                User _dbUser = new User { 
                    Username = user.Username, 
                    Email = user.Email, 
                    EncodedPassword = passwordHash, 
                    FirstName = user.FirstName ?? string.Empty, 
                    LastName = user.LastName ?? string.Empty, 
                    DateOfBirth = user.DateOfBirth ?? DateOnly.MinValue, //date of birth isn't required, so we set it to a default value if it's not provided
                    Address = user.Address
                };

                _db.Users.Add(_dbUser);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to create user: " + ex.Message);
            }
        }

        public async Task<bool> login(LoginDTO user)
        {
            #region Validations

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required.");

            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required.");

            #endregion

            User? login = await _db.Users.FindAsync(user.Username);

            if (login == null)
                throw new ArgumentException("User not found.");

            // verify the provided password against the stored hash
            return BCrypt.Net.BCrypt.Verify(user.Password, login.EncodedPassword);
        }

        public Task<List<User>> getUsers()
        {
            return _db.Users.ToListAsync();
        }

        public async Task<UserDTO> getUser(string username)
        {
            var userFromDB = await _db.Users.FindAsync(username);

            if (userFromDB == null)
                throw new ArgumentException("User not found.");

            //we don't want to return the DB user directly because it contains the password hash
            UserDTO user = new UserDTO
            {
                Username = userFromDB?.Username,
                Email = userFromDB?.Email,
                FirstName = userFromDB?.FirstName,
                LastName = userFromDB?.LastName,
                DateOfBirth = userFromDB?.DateOfBirth,
                Address = userFromDB?.Address
            };

            return user;
        }

        public async Task updateUser(string username, UserDTO data)
        {
            try
            {
                User? user = await _db.Users.FindAsync(username);

                if (user == null)
                    throw new ArgumentException("User not found.");

                if (data.Address != null)
                    user.Address = data.Address;
                if (data.DateOfBirth != null) 
                    user.DateOfBirth = data.DateOfBirth.Value; //we use .value because DateOfBirth is not nullable in User, but it is in UserDTO
                if (data.FirstName != null)
                    user.FirstName = data.FirstName;
                if (data.LastName != null)
                    user.LastName = data.LastName;

                _db.Entry(user).State = EntityState.Modified;

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update user {username} {ex.Message}.");
            }
        }

        public async Task deleteUser(string username) {
            User? user = await _db.Users.FindAsync(username);

            if (user == null)
                throw new ArgumentException("User not found.");

            try
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete user.", ex);
            }
        }
    }
}

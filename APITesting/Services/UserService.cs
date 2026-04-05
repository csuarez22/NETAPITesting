using APITesting.Models;
using APITesting.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace APITesting.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task createUser(User user)
        {
            try
            {
                #region Validations

                if (string.IsNullOrWhiteSpace(user.Username))
                    throw new ArgumentException("Username is required.");

                if (string.IsNullOrWhiteSpace(user.Email))
                    throw new ArgumentException("Email is required.");

                if (await _db.Users.AnyAsync(u => u.Username == user.Username))
                    throw new ArgumentException("User already exists.");

                #endregion

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to create user: " + ex.Message);
            }
        }

        public Task<List<User>> getUsers()
        {
            return _db.Users.ToListAsync();
        }

        public ValueTask<User?> getUser(string username)
        {
            return _db.Users.FindAsync(username);
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

using APITesting.Models;

namespace APITesting.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User> createUser (User user)
        {
            //await _db.Users.FindAsync(user.Username);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

    }
}

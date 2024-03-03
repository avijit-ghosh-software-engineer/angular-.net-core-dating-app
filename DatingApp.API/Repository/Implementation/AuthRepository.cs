using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Repository.Implementation
{
    public class AuthRepository : IAuthRepository {
        private readonly DataContext dataContext;
        public AuthRepository (DataContext _dataContext) {
            dataContext = _dataContext;
        }
        public async Task<User> Register (User user, string password) {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash (password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await dataContext.Users.AddAsync (user);
            await dataContext.SaveChangesAsync ();
            return user;
        }
        public async Task<User> Login (string userName, string password) {
            var user = await dataContext.Users.Include (x => x.Photos).FirstOrDefaultAsync (x => x.UserName == userName);
            if (user == null || !VerifyPasswordHash (password, user.PasswordHash, user.PasswordSalt)) {
                return null;
            }
            return user;
        }
        public async Task<bool> IsUserExists (string userName) {
            if (await dataContext.Users.AnyAsync (x => x.UserName == userName)) {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash (string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hmac = new System.Security.Cryptography.HMACSHA512 ()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
            }
        }

        private bool VerifyPasswordHash (string password, byte[] passwordHash, byte[] passwordSalt) {
            using (var hmac = new System.Security.Cryptography.HMACSHA512 (passwordSalt)) {
                var computedHash = hmac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
                for (int i = 0; i < computedHash.Length; i++) {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }
    }
}
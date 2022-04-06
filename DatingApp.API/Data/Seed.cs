using System.Collections.Generic;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data {
    public class Seed {
        private readonly DataContext dataContext;
        public Seed (DataContext _dataContext) {
            dataContext = _dataContext;
        }

        public void SeedUsers () {
            var userData = System.IO.File.ReadAllText ("SeedData/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>> (userData);
            foreach (var user in users) {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash ("123456", out passwordHash, out passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.UserName = user.UserName.ToLower ();
                dataContext.Users.Add (user);
            }
            dataContext.SaveChanges ();
        }

        private void CreatePasswordHash (string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hmac = new System.Security.Cryptography.HMACSHA512 ()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
            }
        }
    }
}
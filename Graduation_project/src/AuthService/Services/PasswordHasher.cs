
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthService
{
    public class PasswordHasher
    {
        private readonly byte[] _salt;

        public PasswordHasher(string salt)
        {
            this._salt = Encoding.UTF8.GetBytes(salt);
        }

        public string HashPassword(string password)
        {
            return Encoding.UTF8.GetString(KeyDerivation.Pbkdf2(password, _salt, KeyDerivationPrf.HMACSHA256, 100, 32));
        }
    }
}
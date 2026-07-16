using System;
using System.Security.Cryptography;
using System.Text;

namespace OneSWebBooking.Helpers
{
    public static class PasswordHelper
    {
        // Hàm dùng để mã hóa mật khẩu khi đăng ký / tạo tài khoản mới
        public static (string Hash, string Salt) HashPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var salt = Convert.ToBase64String(hmac.Key);
                var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return (hash, salt);
            }
        }

        // Hàm dùng để đối chiếu mật khẩu lúc người dùng Đăng nhập
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }
    }
}
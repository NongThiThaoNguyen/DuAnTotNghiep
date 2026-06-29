namespace DuAnTotNghiep.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Mã hóa mật khẩu sử dụng thư viện BCrypt
        /// </summary>
        /// <param name="password">Mật khẩu gốc (raw password)</param>
        /// <returns>Chuỗi mật khẩu đã được mã hóa (Hash)</returns>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Xác thực mật khẩu nhập vào so với mật khẩu đã mã hóa trong CSDL
        /// </summary>
        /// <param name="password">Mật khẩu nhập vào</param>
        /// <param name="hashedPassword">Mật khẩu đã mã hóa lưu trong DB</param>
        /// <returns>True nếu khớp, False nếu không khớp</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

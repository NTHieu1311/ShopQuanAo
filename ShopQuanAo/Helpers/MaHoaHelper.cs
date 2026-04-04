using System.Security.Cryptography;
using System.Text;

namespace ShopQuanAo.Helpers
{
    public static class MaHoaHelper
    {
        public static string ToSHA256(string matKhau)
        {
            if (string.IsNullOrEmpty(matKhau)) return "";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Chuyển đổi mật khẩu thành một mảng byte
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(matKhau));

                // Chuyển đổi mảng byte thành chuỗi string mã hóa
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
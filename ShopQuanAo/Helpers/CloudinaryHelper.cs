using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ShopQuanAo.Helpers
{
    public class CloudinaryHelper
    {
        private readonly Cloudinary _cloudinary;

        // Hàm khởi tạo: Tự động lấy cấu hình từ appsettings.json mà bạn vừa nhập
        public CloudinaryHelper(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        // Hàm xử lý Upload ảnh
        public async Task<string> UploadImageAsync(IFormFile file, string folderName = "FashionStore")
        {
            if (file == null || file.Length == 0) return string.Empty;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName, // Thư mục lưu trên Cloudinary
                UseFilename = true,
                UniqueFilename = true
            };

            // Đẩy lên mây
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Trả về cái Link ảnh (https://...)
            return uploadResult.SecureUrl.ToString();
        }
    }
}
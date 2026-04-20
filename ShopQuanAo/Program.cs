using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ShopQuanAoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ShopQuanAo.Services.IVnPayService, ShopQuanAo.Services.VnPayService>();

// Đăng ký CloudinaryHelper để dùng ở mọi nơi
builder.Services.AddScoped<CloudinaryHelper>();

// CẤU HÌNH SESSION CHO GIỎ HÀNG
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Giỏ hàng tồn tại trong 60 phút nếu không thao tác
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// [ĐÃ SỬA] CẤU HÌNH COOKIE AUTHENTICATION
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FashionStore_Security_Cookie";
        options.LoginPath = "/TaiKhoan/Login";
        options.AccessDeniedPath = "/TaiKhoan/AccessDenied";

        // Tự động đăng xuất nếu để máy đó không thao tác gì sau 30 phút
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        // Tự động gia hạn thêm 30 phút nếu khách vẫn đang click xem web
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// BẮT BUỘC ĐẶT TRƯỚC UseAuthentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
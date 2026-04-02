using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ShopQuanAoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ShopQuanAo.Services.IVnPayService, ShopQuanAo.Services.VnPayService>();
// ==========================================
// 💡 BỔ SUNG: CẤU HÌNH SESSION CHO GIỎ HÀNG
// ==========================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Giỏ hàng tồn tại trong 60 phút nếu không thao tác
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// CẤU HÌNH COOKIE AUTHENTICATION (Đã có)
// ==========================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/Login";
        options.AccessDeniedPath = "/TaiKhoan/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ==========================================
// 💡 BỔ SUNG: GỌI USE SESSION (BẮT BUỘC ĐẶT TRƯỚC UseAuthentication)
// ==========================================
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
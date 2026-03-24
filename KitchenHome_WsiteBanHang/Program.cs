using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // 1. Thư viện Cookie
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// EPPlus License Context (BẮT BUỘC, nếu không sẽ bị lỗi "A license context must be specified to use EPPlus")
ExcelPackage.License.SetNonCommercialOrganization("KitchenHome");


builder.Services.AddDbContext<DbConnect_KitchenHome_WsiteBanHang>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(60) // tăng timeout 60s
    )
    .EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, LogLevel.Information)
);
// --- 1. CẤU HÌNH DB CONTEXT ---
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<DbConnect_KitchenHome_WsiteBanHang>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddScoped<KitchenHome_WsiteBanHang.Services.CartService>();

// --- 2. ĐĂNG KÝ DỊCH VỤ TRUY CẬP HTTP CONTEXT (QUAN TRỌNG - BẮT BUỘC) ---
// Dòng này sửa lỗi "InvalidOperationException: No service for type 'IHttpContextAccessor'"
builder.Services.AddHttpContextAccessor();
// -------------------------------------------------------------------------

// --- 3. CẤU HÌNH SESSION ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- 4. CẤU HÌNH AUTHENTICATION ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // LƯU Ý: Kiểm tra lại tên Controller của bạn. 
        // Nếu Controller tên là "LoginController" thì đường dẫn phải là "/Login_Wsite/Login/Index"
        // Nếu Controller tên là "HomeController" (trong Area Login) thì để như dưới là đúng.
        options.LoginPath = "/Login_Wsite/Login/Index";

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- 5. KÍCH HOẠT MIDDLEWARE (Thứ tự đúng) ---
app.UseSession();        // 1. Session
app.UseAuthentication(); // 2. Xác thực (Cookie)
app.UseAuthorization();  // 3. Phân quyền




//app.MapControllerRoute(
//    name: "Login_Wsite",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
//);
//app.MapControllerRoute(
//    name: "Admin",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
//);
//app.MapControllerRoute(
//    name: "Quan_Ly",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
//);

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
using KTXReportSystem.Data;
using KTXReportSystem.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// 2. Đăng ký các Dịch vụ quan trọng (BẮT BUỘC)
builder.Services.AddHttpContextAccessor(); // Để truy cập Session ở Layout
builder.Services.AddScoped<ReportService>(); // Logic xử lý báo cáo

// 3. Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Thêm Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Cấu hình Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Kích hoạt Session
app.UseAuthorization();

app.MapRazorPages();

app.Run();
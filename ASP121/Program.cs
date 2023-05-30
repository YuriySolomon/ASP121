using ASP121.Data;
using ASP121.Middleware;
using ASP121.Services.Hash;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddTransient<IHashService, Md5HashService>();
//builder.Services.AddScoped<IHashService, Md5HashService>();
builder.Services.AddSingleton<IHashService, Md5HashService>();
//builder.Services.AddSingleton<IHashService, Sha1HashService>();

// Add Data Context
builder.Services.AddDbContext<DataContext>(options => 
    options.UseMySql(
        builder.Configuration.GetConnectionString("PlanetDb"),
        new MySqlServerVersion(new Version(8, 0, 23))
));

// ������������ ����:
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); // ��������� ����

app.UseSessionAuth(); // ������� Middleware �� ��������, SessionAuth ��� ���� UseSession

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

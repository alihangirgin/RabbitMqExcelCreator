using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMqExcelCreator;
using RabbitMqExcelCreator.Hubs;
using RabbitMqExcelCreator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IConnectionFactory>(x => new ConnectionFactory
{
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMq") ?? string.Empty)
});
builder.Services.AddScoped<IRabbitMqClientService, RabbitMqClientService>();

var connectionString = builder.Configuration.GetConnectionString("ExcelCreatorDb");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

// Apply migrations
dbContext.Database.Migrate();

// Create default user
var user = new IdentityUser
{
    UserName = "alican",
    Email = "alican@testrabbitmq.com"
};

if (await userManager.FindByEmailAsync(user.Email) == null)
{
    await userManager.CreateAsync(user, "Password.123");
}

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
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<FileHub>("/fileHub");

app.Run();
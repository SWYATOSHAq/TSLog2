using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TSLog.Yard;
using TSLog.Yard.BO;
using TSLog.Yard.WebUI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

//
string oracleTns = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521))) (CONNECT_DATA = (SERVICE_NAME = eccobdb)))";
string oracleUserName = "ecco";
string oraclePassword = "e";
string oracleCommonSchema = "ecco";
string pgsqlHost = "10.50.254.14";
string pgsqlUserName = "yardadmin";
string pgsqlPassword = "12";
string pgsqlDatabase = "tslog";
string pgsqlYardSchema = "yard";

var storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase);

storage.ConnectOracle(oracleUserName, oraclePassword);
storage.SetOracleCommonSchema(oracleCommonSchema);
storage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
storage.SetPgsqlYardSchema(pgsqlYardSchema);

builder.Services.AddSingleton<IYardStorage>(storage);
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

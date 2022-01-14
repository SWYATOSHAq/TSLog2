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
//;
builder.Services.AddRazorPages();

var oracleConnectionSettings = builder.Configuration.GetSection("OracleConnectionSettings");
string oracleTns = oracleConnectionSettings.GetValue<string>("DataSource");
string oracleUserName = oracleConnectionSettings.GetValue<string>("UserName");
string oraclePassword = oracleConnectionSettings.GetValue<string>("Password");
string oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema");

var isDebug = builder.Configuration.GetValue<bool>("IsDebug");
var pgsqlConnectionSettings = isDebug 
    ? builder.Configuration.GetSection("PostgreeSqlConnectionDebugSettings")
    : builder.Configuration.GetSection("PostgreeSqlConnectionSettings");
string pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host");
string pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName");
string pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password");
string pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database");
string pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema");

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

// map api
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
//

app.MapRazorPages();

app.Run();
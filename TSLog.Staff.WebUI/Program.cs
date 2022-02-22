using TSLog.Common;
using TSLog.Staff.BO;
using TSLog.Staff.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var isDebug = builder.Configuration.GetValue<bool>("IsDebug");

var oracleConnectionSettings = isDebug
    ? builder.Configuration.GetSection("OracleConnectionDebugSettings")
    : builder.Configuration.GetSection("OracleConnectionSettings");
string oracleTns = oracleConnectionSettings.GetValue<string>("DataSource");

string oracleUserName = oracleConnectionSettings.GetValue<string>("UserName");
string oraclePassword = oracleConnectionSettings.GetValue<string>("Password");

string oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema");
string oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema");
string oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema");

var storage = new StaffOracleStorage(oracleTns);
storage.Connect(oracleUserName, Utils.StringToSecureString(oraclePassword));
storage.CommonSchema = oracleCommonSchema;
storage.LegalEntitiesSchema = oracleLegalEntitiesSchema;
storage.StaffSchema = oracleStaffSchema;

builder.Services.AddSingleton<IStaffStorage>(storage);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
app.MapFallbackToFile("index.html");

app.Run();
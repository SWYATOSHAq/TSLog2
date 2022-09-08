using Microsoft.AspNetCore.Authentication.Negotiate;
using TSLog.Reports.BO;
using TSLog.Reports.Data;

var builder = WebApplication.CreateBuilder(args);

var useAuthentication = builder.Configuration.GetValue<bool>("UseAuthentication");
if (useAuthentication)
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
    builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
}

builder.Services.AddRazorPages();
builder.Services.AddMvc().AddRazorPagesOptions(options =>
{
    options.RootDirectory = "/View";
});

var isDebug = builder.Configuration.GetValue<bool>("IsDebug");

var oracleConnectionSettings = isDebug
    ? builder.Configuration.GetSection("OracleConnectionDebugSettings")
    : builder.Configuration.GetSection("OracleConnectionSettings");
string oracleTns = oracleConnectionSettings.GetValue<string>("DataSource");
string oracleUserName = oracleConnectionSettings.GetValue<string>("UserName");
string oraclePassword = oracleConnectionSettings.GetValue<string>("Password");
string oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema");
string oracleWmsExtensionsSchema = oracleConnectionSettings.GetValue<string>("WmsExtensionsSchema");
string oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema");
string oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema");
string oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema");

var storage = new ReportsStorage(oracleTns) 
{ 
    MaxRowsCount = builder.Configuration.GetValue<int>("MaxRowsCount") 
};

storage.ConnectOracle(oracleUserName, oraclePassword);
storage.SetOracleCommonSchema(oracleCommonSchema);
storage.SetOracleWmsExtensionsSchema(oracleWmsExtensionsSchema);
storage.SetOracleStaffSchema(oracleStaffSchema);
storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
storage.SetOraclePartnersSchema(oraclePartnersSchema);

builder.Services.AddSingleton<IReportsStorage>(storage);

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

if (useAuthentication)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// map api
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});

app.MapRazorPages();

app.Run();
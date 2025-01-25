using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Reflection;
using TSLog.Authorization;
using TSLog.Reports.BO;
using TSLog.Reports.Data;

const string DefaultCommonSchema = "common";
const string DefaultWmsExtensionsSchema = "wms_ext";
const string DefaultStaffSchema = "staff";
const string DefaultLegalEntitiesSchema = "legal_entities";
const string DefaultPartnersSchema = "partners";

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
var oracleTns = oracleConnectionSettings.GetValue<string>("DataSource") ?? string.Empty;
var oracleUserName = oracleConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
var oraclePassword = oracleConnectionSettings.GetValue<string>("Password") ?? string.Empty;
var oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema") ?? DefaultCommonSchema;
var oracleWmsExtensionsSchema = oracleConnectionSettings.GetValue<string>("WmsExtensionsSchema") ?? DefaultWmsExtensionsSchema;
var oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema") ?? DefaultStaffSchema;
var oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema") ?? DefaultLegalEntitiesSchema;
var oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema") ?? DefaultPartnersSchema;

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

var rightsChecker = new RightsChecker();
builder.Services.AddSingleton(rightsChecker);
var programFileName = Assembly.GetExecutingAssembly().Location;
var programDirectory = Path.GetDirectoryName(programFileName) ?? throw new Exception("Rights file not found");
var rightsFileName = Path.Combine(programDirectory, "rights.xml");
rightsChecker.LoadRights(rightsFileName);

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

// Map api.
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});

app.MapRazorPages();

app.Run();

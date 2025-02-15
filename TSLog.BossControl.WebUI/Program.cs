using Microsoft.AspNetCore.Authentication.Negotiate;
using TSLog.BossControl.BO;
using TSLog.BossControl.Data;

const string DefaultCommonSchema = "common";
const string DefaultStaffSchema = "staff";
const string DefaultLegalEntitiesSchema = "legal_entities";
const string DefaultPartnersSchema = "partners";

var builder = WebApplication.CreateBuilder(args);

var useAuthentication = builder.Configuration.GetValue<bool>("UseAuthentication");
if (useAuthentication)
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
    builder.Services.AddAuthorization(options =>
    {
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
    });
}
builder.Services.AddRazorPages();
builder.Services.AddMvc().AddRazorPagesOptions(options =>
    {
        options.RootDirectory = "/View";
    });
builder.Services.AddMvc();

var isDebug = builder.Configuration.GetValue<bool>("IsDebug");

var oracleConnectionSettings = isDebug
    ? builder.Configuration.GetSection("OracleConnectionDebugSettings")
    : builder.Configuration.GetSection("OracleConnectionSettings");
var oracleTns = oracleConnectionSettings.GetValue<string>("DataSource") ?? string.Empty;
var oracleUserName = oracleConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
var oraclePassword = oracleConnectionSettings.GetValue<string>("Password") ?? string.Empty;
var oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema") ?? DefaultCommonSchema;
var oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema") ?? DefaultStaffSchema;
var oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema") ?? DefaultLegalEntitiesSchema;
var oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema") ?? DefaultPartnersSchema;

var storage = new BossControlOracleStorage(oracleTns);
storage.Connect(oracleUserName, oraclePassword);
storage.SetOracleCommonSchema(oracleCommonSchema);
storage.SetOracleStaffSchema(oracleStaffSchema);
storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
storage.SetOraclePartnersSchema(oraclePartnersSchema);

builder.Services.AddSingleton<IBossControlStorage>(storage);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});

app.MapRazorPages();
app.Run();

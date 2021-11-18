using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security;
using TSLog.BO;
using TSLog.Links.Db;
using TSLog.Links.Gs1EdiXml;

namespace TSLog.Links.Services
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InitSingletrons(services);

            var temp = services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true; // false by default
            });
            temp.AddXmlSerializerFormatters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void InitSingletrons(IServiceCollection services)
        {
            // TODO: Connection params брать из конфига

            // ---------------------------------------
            // init storage
            // this storage is then used to get dada from db
            // information on how data access is then split is in ILinksStorage interface
            var dataSource =
               "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = eccobdb)))";

            var storage = new DbLinksStorage(dataSource);
            storage.CommonSchema = "ecco";
            storage.LinksSchema = "links";
            storage.WmsSchema = "ecco2_s1_pdba";

            storage.IncomeStates = new Catalog<Entity>("INBOUND_STATES", "Состояия приходов");
            storage.OutgoStates = new Catalog<Entity>("OUTBOUND_STATES", "Состояия расходов");
            storage.MeasureUnits = new Catalog<MeasureUnit>("MEASURE_UNITS", "Единицы измерения");
            storage.Partners = new Catalog<LegalEntity>("PARTNERS", "Партнёры");
            storage.Currencies = new Catalog<Entity>("CURRENCIES", "валюты");

            storage.Products = new ProductCatalog<Product>("PRODUCTS", "Товары");

            var password = new SecureString();
            foreach (var symbol in "e")
                password.AppendChar(symbol);
            storage.Connect("ecco", password);

            // preload units and states
            storage.IncomeStates.Add(storage.GetIncomeStates());
            storage.OutgoStates.Add(storage.GetOutgoStates());
            storage.MeasureUnits.Add(storage.GetMeasureUnits());
            storage.Currencies.Add(storage.GetCurrncies());

            //init convertor
            var xmlConverter = new Gs1EdiXmlConverter();
            xmlConverter.LinksStorage = storage;
            // ---------------------------------------
            services.AddSingleton<ILinksStorage>(storage);
            services.AddSingleton(xmlConverter);
        }
    }
}
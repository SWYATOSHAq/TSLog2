using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security;
using TSLog.Links.Db;

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
            // TODO: Connection params брать из конфига 

            var storage = new DbLinksStorage("(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.50.254.29)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = eccobdb)))");
            var password = new SecureString();
            foreach (var symbol in "e")
                password.AppendChar(symbol);
            storage.Connect("ecco", password);

            services.AddSingleton<ILinksStorage>(storage);
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
    }
}

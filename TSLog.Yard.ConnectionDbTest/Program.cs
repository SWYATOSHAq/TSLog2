using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace TSLog.Yard.ConnectionDbTest
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var builder = new ConfigurationBuilder();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(("appsettings.json"))
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            var isDebug = configuration.GetValue<bool>("IsDebug");

            var pgsqlConnectionSettings = isDebug
                ? configuration.GetSection("PostgreSqlConnectionDebugSettings")
                : configuration.GetSection("PostgreSqlConnectionSettings");
            string pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host");
            string pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName");
            string pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password");
            string pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database");
            string pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema");

            var app = builder.Build();
        }
    }
}



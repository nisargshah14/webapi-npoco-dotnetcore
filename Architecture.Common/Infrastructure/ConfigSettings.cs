using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Architecture.Common.Infrastructure
{
    public static class ConfigSettings
    {
        public static readonly string _connectionString = string.Empty;
        public static readonly int _pageSize;

        static ConfigSettings()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            var appSetting = root.GetSection("AppSettings");

            _connectionString = root.GetConnectionString("DefaultConnection");
            _pageSize = Convert.ToInt32(appSetting.GetSection("PageSize").Value);

        }

        public static string ConnectionString
        {
            get => _connectionString;
        }

        public static int PageSize
        {
            get => _pageSize;
        }

    }
}
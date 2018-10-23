using System;
using System.IO;
using events_planner.Utils.DataFormatter.Accessors;
using events_planner.Utils.DataFormatter.Implementations;
using Microsoft.AspNetCore.Hosting;

namespace events_planner.Utils.DataFormatter
{
    public class DataFormatterFactory
    {
        private IHostingEnvironment _environment { get; }
        
        public DataFormatterFactory(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public IFormatter GetFormatter(FormatterType type)
        {
            return GetFormatter(type, null);
        }

        public IFormatter GetFormatter(FormatterType type, IFormatterConfig config)
        {
            IFormatter formatter = null;
            switch (type)
            {
                case FormatterType.CSV:
                    formatter = InstantiateCSV(config);
                    break;
                default:
                    throw new ArgumentException($"Formatter Not Found {type}");
            }

            return formatter;
        }
        
        #region Configurations

        public IFormatterConfig UseFileConfiguration<T>(FormatterType type)
        {
            IFormatterConfig cfg = null;
            switch (type)
            {
                case FormatterType.CSV:
                    return InstantiateFileCSVConfiguration<T>();
                    break;
                default:
                    throw new ArgumentException($"Formatter Not Found {type}"); 
            }

            return cfg;
        }
        
        #endregion
        
        #region PRIVATE

        private IFormatter InstantiateCSV(IFormatterConfig config)
        {
            if (config == null)
            {
                config = new FormatterConfig()
                {
                    IsInMemory = true,
                    MappingType = null,
                    Path = string.Empty
                };
            }
            return new CsvFormatter(config);
        }

        private IFormatterConfig InstantiateFileCSVConfiguration<T>()
        {
            return new FormatterConfig()
            {
                IsInMemory = false,
                MappingType = typeof(T),
                Path = Path.Combine(_environment.WebRootPath, "csv")
            };
        }
        
        #endregion
    }
}
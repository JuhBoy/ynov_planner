using System;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using events_planner.Utils.DataFormatter.Accessors;

namespace events_planner.Utils.DataFormatter.Implementations
{
    public class CsvFormatter : IFormatter
    {
        /// <summary>
        /// Hold Configuration informations
        /// </summary>
        private IFormatterConfig _config;

        /// <summary>
        /// Is the operation completed successfully
        /// </summary>
        public bool IsSuccess { get; set; } = true;
        
        /// <summary>
        /// The Error that occured [Nullable]
        /// </summary>
        public Exception Error { get; set; }
        
        /// <summary>
        /// Configuration Accessor
        /// </summary>
        /// <exception cref="NullReferenceException">Cannot be null</exception>
        public IFormatterConfig Configuration
        {
            get { return _config; }
            set
            {
                if (value.Equals(null))
                {
                    throw new NullReferenceException("Configuration cannot be null");    
                }

                _config = value;
            }
        }

        /// <summary>
        /// Base constructor with configuration object
        /// </summary>
        /// <param name="config">IFormatterConfig</param>
        public CsvFormatter(IFormatterConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Format A uniq Entity to TextStream.
        /// </summary>
        /// <param name="record">The Record</param>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <returns>a string csv or path to the file generated</returns>
        /// <exception cref="NotImplementedException">Not Yet Implemented</exception>
        public async Task<string> FormatAsync<T>(T record) where T : IEnumerable
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Format A IEnumerable<T> to TextStream.
        /// </summary>
        /// <param name="record">The Records</param>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <returns>a string csv or path to the file generated</returns>
        /// <exception cref="NotImplementedException">Not Yet Implemented</exception>
        public async Task<string> FormatMultipleAsync<T>(T records) where T : IEnumerable
        {
            try
            {
                if (_config.IsInMemory)
                {
                    return await InMemoryWrites(records);
                }
                return await InFileWritters(records);
            }
            catch (Exception ex)
            {
                Error = ex;
                IsSuccess = false;
                return null;
            }
        }

        #region PRIVATE
        
        private async Task<string> InFileWritters<T>(T records) where T : IEnumerable
        {
            return await Task.Run(() =>
            {
                var name = "export_" + DateTime.Now.ToString("HH_mm_ss_ms") + ".csv";
                var path = Path.Combine(_config.Path, name);
                
                using (var stream = new StreamWriter(path, false)) 
                {
                    using (var csvWriter = new CsvWriter(stream))
                    {
                        csvWriter.Configuration.RegisterClassMap(_config.MappingType);
                        csvWriter.Configuration.UseNewObjectForNullReferenceMembers = true;
                        csvWriter.WriteRecords(records);
                        return name;
                    }
                }
            }).ConfigureAwait(false);
        }

        private async Task<string> InMemoryWrites<T>(T records) where T : IEnumerable
        {
            return await Task.Run(() =>
            {
                using (var stream = new StringWriter())
                {
                    using (var csvWriter = new CsvWriter(stream))
                    {
                        csvWriter.Configuration.RegisterClassMap(_config.MappingType);
                        csvWriter.Configuration.UseNewObjectForNullReferenceMembers = true;
                        csvWriter.WriteRecords(records);
                        return stream.ToString();
                    }
                }
            }).ConfigureAwait(false);
        }
        
        #endregion
        
        #region Errors
        
        /// <summary>
        /// Ensure the operation completed succefully or throw the inner error
        /// </summary>
        /// <exception cref="<Exception>">The Exception</exception>
        public void EnsureSuccessOperation()
        {
            if (Error != null)
            {
                var tmpError = Error;
                Error = null;
                IsSuccess = true;
                throw tmpError;
            }
        }

        /// <summary>
        /// Get the last exception that occured
        /// </summary>
        /// <returns>The Exception</returns>
        public Exception GetError()
        {
            return Error;
        }
        
        #endregion
    }
}
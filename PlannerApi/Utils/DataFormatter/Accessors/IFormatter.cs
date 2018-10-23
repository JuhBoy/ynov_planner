using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace events_planner.Utils.DataFormatter.Accessors
{
    public interface IFormatter
    {
        bool IsSuccess { get; set; }
        Exception Error { get; set; }
        
        IFormatterConfig Configuration { get; set; }
        Task<string> FormatAsync<T>(T record) where T : IEnumerable;
        Task<string> FormatMultipleAsync<T>(T Record) where T : IEnumerable;
    }
}
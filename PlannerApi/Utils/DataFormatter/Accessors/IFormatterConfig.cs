using System;

namespace events_planner.Utils.DataFormatter.Accessors
{
    public interface IFormatterConfig
    {
        bool IsInMemory { get; set; }
        
        string Path { get; set; }
        
        Type MappingType { get; set; }
    }
}
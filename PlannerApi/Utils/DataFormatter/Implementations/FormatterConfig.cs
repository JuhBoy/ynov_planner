using System;
using events_planner.Utils.DataFormatter.Accessors;

namespace events_planner.Utils.DataFormatter.Implementations
{
    public class FormatterConfig : IFormatterConfig
    {
        public bool IsInMemory { get; set; }
        
        public string Path { get; set; }
        
        public Type MappingType { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers {
    public class CategoryDeserializer {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        public string Name { get; set; }

        public int? subCategoryId { get; set; }
    }

    public class CategoryDeserializerUpdate {
        [StringLength(20, MinimumLength = 2)]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        public string Name { get; set; }
        public int? subCategoryId { get; set; }
    }
}
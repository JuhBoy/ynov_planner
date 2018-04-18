using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {

    [Table("image")]
    public class Image {

        [Column("image_id"), Key]
        public int ImageId { get; set; }

        [Column("url"), Required, Url]
        public string Url { get; set; }

        [Column("alt"), MaxLength(50), MinLength(8)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alt { get; set; }

        [Column("title"), MaxLength(50), MinLength(8)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [Column("event_id"), ForeignKey("event_id"), JsonIgnore]
        public int EventId { get; set; }

        [JsonIgnore]
        public Event Event { get; set; }
    }
}

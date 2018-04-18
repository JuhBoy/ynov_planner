using events_planner.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace events_planner.Deserializers {
    
    public class ImageUploadDeserializer {

        public string ImagesData { get; set; }

        private List<AltAndTitleFormatter> list;

        public List<AltAndTitleFormatter> GetImagesData() {
            if (list == null) {
                list = JsonConvert.DeserializeObject<List<AltAndTitleFormatter>>(ImagesData);
            }

            return list;
        }

        // ============================
        //   Serializer for inner Data
        // ============================
        public class AltAndTitleFormatter {
            
            public string FileName { get; set; }

            public string Alt { get; set; }

            public string Title { get; set; }

        }
    }
}

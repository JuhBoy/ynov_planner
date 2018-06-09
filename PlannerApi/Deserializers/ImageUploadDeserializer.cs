using Newtonsoft.Json;
using System.Collections.Generic;

namespace events_planner.Deserializers {
    
    public class ImageUploadDeserializer {

        public string ImagesData { get; set; }

        private List<AltAndTitleFormatter> List;

        public List<AltAndTitleFormatter> GetImagesData() {
            if (List == null) {
                List = JsonConvert.DeserializeObject<List<AltAndTitleFormatter>>(ImagesData);
            }

            return List;
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

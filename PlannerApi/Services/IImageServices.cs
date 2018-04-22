using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace events_planner.Services {
    public interface IImageServices {
        bool IsValidExtension(string file);
        bool IsValidMymeType(string file);
        Task<Dictionary<string, string>> UploadImageAsync(IFormFileCollection files,
                                                          string baseFileName,
                                                          string folder = "images");
        Task RemoveImages(string path);
    }
}

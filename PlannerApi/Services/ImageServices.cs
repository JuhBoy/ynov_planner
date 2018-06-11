using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace events_planner.Services {

    public class ImageServices : IImageServices {

        private static string PATH_EXTENSION = @"(.png|.jpg|.svg|.gif|.webp|.bmp)\Z";

        private static string[] MYME_TYPES = new string[] {
            "image/gif",
            "image/png",
            "image/jpeg",
            "image/bmp",
            "image/webp"
        };

        IHostingEnvironment Environment { get; set; }

        public ImageServices(IHostingEnvironment environment) {
            Environment = environment;
        }

        #region VALIDATORS

        public bool IsValidExtension(string fileName) {
            Regex regex = new Regex(PATH_EXTENSION);
            Match match = regex.Match(fileName);
            return (match.Length != 1);
        }

        public bool IsValidMymeType(string contentType) {
            return MYME_TYPES.Contains(contentType);
        }

        #endregion

        #region FORMATTERS

        private string GenerateName(ref string name, string extension) {
            return Guid.NewGuid() + "_" + name + extension;
        }

        #endregion

        public async Task<Dictionary<string, string>> UploadImageAsync(IFormFileCollection files,
                                                                       string baseFileName,
                                                                       string folder = "images") {
            string path = Path.Combine(Environment.WebRootPath, folder);

            ThreadResponse resultFromIOThread = await Task.Run<ThreadResponse>(() => {
                var response = new ThreadResponse {
                    UrlsName = new Dictionary<string, string>()
                };

                // CREATE DIRECTORY IF NECESSARY
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                // LOOP TO COPY IMAGES
                foreach (IFormFile file in files) {
                    if (file.Length <= 0) continue;

                    if (IsValidMymeType(file.ContentType) &&
                        IsValidExtension(file.FileName)) {
                        string newFileName = GenerateName(ref baseFileName, Path.GetExtension(file.FileName));
                        string filePath = Path.Combine(path, newFileName);

                        using (var stream = new FileStream(filePath, FileMode.CreateNew)) {
                            try {
                                file.CopyTo(stream);
                                response.UrlsName.Add(folder + "/" + newFileName, file.FileName);
                            } catch (IOException e) {
                                response.Exception = e;
                            }
                        }
                    }
                }
                return response;
            });

            if (resultFromIOThread.Exception != null) {
                throw new IOException(resultFromIOThread.Exception.Message);
            }

            return resultFromIOThread.UrlsName;
        }

        public async Task RemoveImages(string path) {
            string fullPath = Path.Combine(Environment.WebRootPath, path);
            if (!File.Exists(fullPath)) {
                throw new FileNotFoundException(string.Format("File not found at path {0}", fullPath));
            }
            await Task.Run(() => File.Delete(fullPath));
        }

        private struct ThreadResponse {
            public IOException Exception { get; set; }
            public Dictionary<string, string> UrlsName { get; set; }
        }

    }
}
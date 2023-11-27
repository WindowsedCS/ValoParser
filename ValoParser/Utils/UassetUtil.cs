using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Textures;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ValoParser.Utils
{
    public class UassetUtil
    {
        static DefaultFileProvider provider = Program.provider;

        public static JsonNode loadJson(string path)
        {
            if (path != null)
            {
                var allExports = Program.provider.LoadObject(path);
                var jsonUnparsed = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var json = JsonNode.Parse(jsonUnparsed);
                return json;
            }
            else
            {
                return null;
            }
        }

        public static JsonNode loadFullJson(string path)
        {
            if (path != null)
            {
                var allExports = Program.provider.LoadAllObjects(path);
                var jsonUnparsed = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var json = JsonNode.Parse(jsonUnparsed);
                return json;
            }
            else
            {
                return null;
            }
        }

        public static void exportJson(JsonObject json, string outputPath)
        {
            string output = json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            if (!Directory.Exists(string.Format(@"{0}/{1}", Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), ""))))
            {
                Directory.CreateDirectory(string.Format(@"{0}/{1}", Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), "")));
            }
            File.WriteAllText(string.Format(@"{0}/{1}", Program.exportRoot, outputPath), output, Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully exported {0} in {1}/{2}", outputPath.Split("/").Last(), Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), "")));
        }

        public static void exportJson(JsonArray json, string outputPath)
        {
            string output = json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            if (!Directory.Exists(string.Format(@"{0}/{1}", Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), ""))))
            {
                Directory.CreateDirectory(string.Format(@"{0}/{1}", Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), "")));
            }
            File.WriteAllText(string.Format(@"{0}/{1}", Program.exportRoot, outputPath), output, Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully exported {0} in {1}/{2}", outputPath.Split("/").Last(), Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), "")));
        }

        public static void exportJson(JsonNode json, string outputPath)
        {
            string output = json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            if (!Directory.Exists(string.Format(@"{0}/{1}", Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), ""))))
            {
                Directory.CreateDirectory(string.Format(@"{0}/{1}", Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), "")));
            }
            File.WriteAllText(string.Format(@"{0}/{1}", Program.exportRoot, outputPath), output, Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully exported {0} in {1}/{2}", outputPath.Split("/").Last(), Program.exportRoot, outputPath.Replace(outputPath.Split("/").Last(), "")));
        }

        public static void exportImage(string path, string outputPath)
        {
            var bitmap = provider.LoadObject<UTexture2D>(path).Decode();
            if (!Directory.Exists(string.Format(@"{0}", outputPath.Replace(outputPath.Split("/").Last(), ""))))
            {
                Directory.CreateDirectory(outputPath.Replace(outputPath.Split("/").Last(), ""));
            }
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(string.Format(@"{0}", outputPath)))
            {
                data.SaveTo(stream);
            }
        }
    }
}

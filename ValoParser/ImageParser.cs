using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Textures;
using SkiaSharp;
using System;
using System.IO;

namespace ValoParser
{
    public class ImageParser
    {
        public void Parse(String path, String outputPath)
        {
            var provider = Program.provider;
            var bitmap = provider.LoadObject<UTexture2D>(path).Decode();
            var currentPath = "./files";
            for (var i = 0; i < outputPath.Split("/").Length; i++)
            {
                currentPath += "/" + outputPath.Split("/")[i];
                if (!outputPath.Split("/")[i].Contains("."))
                {
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(String.Format(@"./files/{0}", outputPath)))
            {
                data.SaveTo(stream);
            }
        }
    }
}
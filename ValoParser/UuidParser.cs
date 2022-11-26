using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Textures;
using SkiaSharp;
using System;
using System.IO;

namespace ValoParser
{
    public static class UuidParser
    {
        public static String Parse(String uuid)
        {
            uuid = uuid.ToLower();
            uuid = uuid.Replace(@"-", "");
            uuid = uuid.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
            return uuid;
        }
    }
}
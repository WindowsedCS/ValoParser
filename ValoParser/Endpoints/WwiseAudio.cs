using CUE4Parse.FileProvider;
using System;
using System.IO;
using System.Linq;
using ValoParser.Parsers;

namespace ValoParser.Endpoints
{
    public static class WwiseAudio
    {

        public static void Parse(GameFile file)
        {
            var provider = Program.provider;
            if (file.Path.StartsWith("ShooterGame/Content/WwiseAudio/Localized/") && file.Path.Contains("/Media/"))
            {
                AudioParser.Parse(file.Path, String.Format("assets/audios/{0}/{1}", file.Path.Split("/")[4], file.Path.Split(".")[0].Split("/").Last()));
            }
        }

        public static void Parse(GameFile file, string language)
        {
            var provider = Program.provider;
            if (file.Path.StartsWith("ShooterGame/Content/WwiseAudio/Localized/" + language) && file.Path.Contains("/Media/"))
            {
                AudioParser.Parse(file.Path, String.Format("assets/audios/{0}/{1}", file.Path.Split("/")[4], file.Path.Split(".")[0].Split("/").Last()));
            }
        }
    }
}
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse_Conversion.Textures;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ValoParser
{
    public static class Program
    {
        public static bool logDetailed = false;
        private const string _aesKey = "0x4BE71AF2459CF83899EC9DC2CB60E22AC4B3047E0211034BBABE9D174C069DD6";
        public static DefaultFileProvider provider;

        public static List<ELanguage> languageCodes = new List<ELanguage>() {
            ELanguage.Arabic,
            ELanguage.German,
            ELanguage.English,
            ELanguage.Spanish,
            ELanguage.SpanishMexico,
            ELanguage.French,
            ELanguage.Indonesian,
            ELanguage.Italian,
            ELanguage.Japanese,
            ELanguage.Korean,
            ELanguage.Polish,
            ELanguage.PortugueseBrazil,
            ELanguage.Russian,
            ELanguage.Thai,
            ELanguage.Turkish,
            ELanguage.VietnameseVietnam,
            ELanguage.Chinese,
            ELanguage.TraditionalChinese,
        };

        public static void Main(string[] args)
        {
            string gameDirectory = args.Length > 0 ? args[0] : "C:\\Riot Games\\VALORANT\\live";

            provider = new(gameDirectory, SearchOption.AllDirectories, true, new VersionContainer(EGame.GAME_Valorant));

            provider.LoadLocalization(ELanguage.Chinese);
            provider.Initialize();
            provider.SubmitKey(new FGuid(), new FAesKey(_aesKey));

            Console.WriteLine("Parsing game assets...");
            if (Directory.Exists(gameDirectory))
            {
                //Check output directory
                if (!Directory.Exists(@"./files"))
                {
                    Directory.CreateDirectory("files");
                }
                if (!Directory.Exists(@"./files/weapons"))
                {
                    Directory.CreateDirectory("./files/weapons");
                }
                //Parse game assets
                foreach (var file in provider.Files.Values)
                {
                    Equippables.weapons(file);
                    Battlepass.Parse(file);
                }
                //Parse localizations
                foreach (var lang in languageCodes)
                {
                    provider.LoadLocalization(lang);
                    Equippables.Localization(lang);
                    Battlepass.Localization(lang);
                }

                Console.WriteLine("VALORANT bas been successfully parsed!");
            } else
            {
                Console.Error.WriteLine(String.Format("Path does not exist!"));
            }
        }
    }
}
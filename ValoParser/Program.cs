using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ValoParser.Parsers;

namespace ValoParser
{
    public static class Program
    {

        private const string _aesKey = "0x4BE71AF2459CF83899EC9DC2CB60E22AC4B3047E0211034BBABE9D174C069DD6";
        public static DefaultFileProvider provider;
        public static string exportRoot = @"./exports";

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

        public static async Task Main(string[] args)
        {
            string gameDirectory = args.Length > 0 ? args[0] : "C:\\Riot Games\\VALORANT\\live";
            string riotClientDir = args.Length > 1 ? args[1] : "C:\\Riot Games\\Riot Client";

            provider = new(gameDirectory, SearchOption.AllDirectories, true, new VersionContainer(EGame.GAME_Valorant));

            provider.LoadLocalization(ELanguage.English);
            provider.Initialize();
            provider.SubmitKey(new FGuid(), new FAesKey(_aesKey));

            Console.WriteLine("Parsing Game Assets...");

            VersionParser VersionParser = new VersionParser();
            VersionParser.GetVersionContent(gameDirectory, riotClientDir);

            LocresParser LocresParser = new LocresParser();
            LocresParser.getLocresContent();

            AgentsParser AgentsParser = new AgentsParser();
            AgentsParser.getAgentsContent();

            foreach (var locale in LocresParser.AvailableLocres)
            {
                AgentsParser.Localization(locale.ToString());
            }
        }
    }
}
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

        public static void Main(string[] args)
        {
            // string gameDirectory = args.Length > 0 ? args[0] : "C:\\Riot Games\\VALORANT\\live";
            // string riotClientDir = args.Length > 1 ? args[1] : "E:\\Riot Games\\Riot Client";
            string gameDirectory = args.Length > 0 ? args[0] : "E:\\ManifestRmanTest\\valorant";
            string riotClientDir = args.Length > 1 ? args[1] : "E:\\ManifestRmanTest\\riotclient";

            provider = new(gameDirectory, SearchOption.AllDirectories, true, new VersionContainer(EGame.GAME_Valorant));

            provider.LoadLocalization(ELanguage.English);
            provider.Initialize();
            provider.SubmitKey(new FGuid(), new FAesKey(_aesKey));

            Console.WriteLine("Parsing Game Assets...");

            VersionParser versionParser = new();
            versionParser.GetVersionContent(gameDirectory, riotClientDir);

            LocresParser locresParser = new();
            locresParser.getLocresContent();

            AgentsParser agentsParser = new();
            agentsParser.getAgentsContent();

            CeremoniesParser veremoniesParser = new();
            veremoniesParser.getCeremoniesContent();

            ContentTiersParser contentTiersParser = new();
            contentTiersParser.getContentTiersContent();

            CurrenciesParser currenciesParser = new();
            currenciesParser.getCurrenciesContent();

            LevelBordersParser levelBordersParser = new();
            levelBordersParser.getLevelBordersContent();

            PlayerTitlesParser playerTitlesParser = new();
            playerTitlesParser.getPlayerTitlesContent();

            ThemesParser themesParser = new();
            themesParser.getThemesContent();

            foreach (var locale in locresParser.AvailableLocres)
            {
                string localeStr = provider.GetLanguageCode(locale);
                provider.LoadLocalization(locale);
                agentsParser.Localization(localeStr);
                veremoniesParser.Localization(localeStr);
                contentTiersParser.Localization(localeStr);
                currenciesParser.Localization(localeStr);
                levelBordersParser.Localization(localeStr);
                playerTitlesParser.Localization(localeStr);
                themesParser.Localization(localeStr);
            }
        }
    }
}
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValNet.Objects.Authentication;
using ValNet;
using ValNet.Enums;
using ValNet.Objects;
using ValoParser.Endpoints;

namespace ValoParser
{
    public static class Program
    {

        public static bool logDetailed = false;
        private const string _aesKey = "0x4BE71AF2459CF83899EC9DC2CB60E22AC4B3047E0211034BBABE9D174C069DD6";
        public static DefaultFileProvider provider;
        public static JsonObject version = JsonNode.Parse(File.ReadAllText("./files/version.json")).AsObject();
        public static RiotUser User;

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

            provider = new(gameDirectory, SearchOption.AllDirectories, true, new VersionContainer(EGame.GAME_Valorant));

            provider.LoadLocalization(ELanguage.Chinese);
            provider.Initialize();
            provider.SubmitKey(new FGuid(), new FAesKey(_aesKey));

            // await loginRiotAPI();

            Console.WriteLine("Parsing Game Assets...");
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
                /* if (!Directory.Exists(@"./files/contracts"))
                {
                    Directory.CreateDirectory("./files/contracts");
                }
                if (!Directory.Exists(@"./files/contenttiers"))
                {
                    Directory.CreateDirectory("./files/contenttiers");
                }
                if (!Directory.Exists(@"./assets/audios"))
                {
                    Directory.CreateDirectory("./assets/audios");
                }
                if (!Directory.Exists(@"./assets/levelborders"))
                {
                    Directory.CreateDirectory("./assets/levelborders");
                } */
                //Parse game assets
                Parallel.ForEach(provider.Files.Values, file =>
                {
                    Equippables.Weapons(file);
                    // Contracts.Parse(file);
                    // ContentTiers.Parse(file);
                    // LevelBorders.Parse(file);
                    if (args.Length > 1)
                    {
                        if (args[1] == "true")
                        {
                            if (args.Length > 2)
                            {
                                WwiseAudio.Parse(file, args[2]);
                            }
                            else
                            {
                                WwiseAudio.Parse(file);
                            }
                        }
                    }
                });
                Console.WriteLine("Game Assets have been successfully parsed!");
                /* Console.WriteLine("Adding VP Cost to weapons...");
                await Equippables.AddVpCost();
                Console.WriteLine("VP Cost has been successfully added to weapons!"); */
                //Parse localizations
                Console.WriteLine("Localizing all endpoint files...");
                foreach (var lang in languageCodes)
                {
                    provider.LoadLocalization(lang);
                    Equippables.Localization(lang);
                    // Contracts.Localization(lang);
                    // ContentTiers.Localization(lang);
                    // LevelBorders.Localization(lang);
                }
                Console.WriteLine("VALORANT bas been successfully parsed!");
            } else
            {
                Console.Error.WriteLine(String.Format("Path does not exist!"));
            }
        }

        public static async Task loginRiotAPI()
        {
            Console.WriteLine("Logging into Riot Games API...");
            RiotLogin LoginData = new()
            {
                username = "WatchAndyUS",
                password = "2agidrdl"
            };
            User = new RiotUserBuilder().WithCredentials(LoginData).WithSettings(new RiotUserSettings() { AuthenticationMethod = AuthenticationMethod.CURL }).Build();
            await User.Authentication.AuthenticateWithCloud();
            Console.WriteLine("Riot Games API has been successfully logged in!");
        }
    }
}
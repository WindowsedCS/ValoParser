using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ValoParser.Parsers;
using System.IO;

namespace ValoParser.Endpoints
{
    internal class LevelBorders
    {

        private static JsonObject jsonObject = new JsonObject();

        public static void Parse(GameFile file)
        {
            var provider = Program.provider;
            JsonObject itemObject = new JsonObject();
            if (file.Path.StartsWith("ShooterGame/Content/Personalization/LevelBorders") && file.Path.EndsWith("_PrimaryAsset.uasset"))
            {
                // PrimaryAssets
                var allExports = provider.LoadObjectExports(file.Path);
                var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var primaryAsset = JsonNode.Parse(fullJson);
                var uuid = UuidParser.Parse(primaryAsset[1]["Properties"]["Uuid"].ToString());
                itemObject.Add("startingLevel", int.Parse(primaryAsset[1]["Properties"]["StartingLevel"].ToString()));

                // UIData
                var uidataExports = provider.LoadObjectExports(primaryAsset[1]["Properties"]["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                var uidataJson = JsonConvert.SerializeObject(uidataExports, Formatting.Indented);
                var UIData = JsonNode.Parse(uidataJson);
                ImageParser parser = new ImageParser();

                // LevelNumberAppearance
                var LevelNumberAppearance = UIData[1]["Properties"]["LevelNumberAppearance"]["ObjectPath"].ToString();
                parser.Parse(LevelNumberAppearance.Split(".")[0], "levelborders/" + uuid + "/levelnumberappearance.png");
                itemObject.Add("levelNumberAppearance", "https://assets.empressival.com/levelborders/" + uuid + "/levelnumberappearance.png");

                // SmallPlayerCardAppearance
                var SmallPlayerCardAppearance = UIData[1]["Properties"]["SmallPlayerCardAppearance"]["ObjectPath"].ToString();
                parser.Parse(SmallPlayerCardAppearance.Split(".")[0], "levelborders/" + uuid + "/smallplayercardappearance.png");
                itemObject.Add("smallPlayerCardAppearance", "https://assets.empressival.com/levelborders/" + uuid + "/smallplayercardappearance.png");

                // Final
                jsonObject.Add(uuid, itemObject);
            }
        }

        public static void Localization(ELanguage lang)
        {
            File.WriteAllText(string.Format(@"./files/levelborders/{0}.json", Program.provider.GetLanguageCode(lang)), jsonObject.ToJsonString(), Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully saved levelborders in {0}!", Program.provider.GetLanguageCode(lang)));
        }
    }
}

using System;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Drawing.Imaging;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using System.Text.Json.Nodes;
using System.Linq;
using File = System.IO.File;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Textures;
using System.Text;
using System.Text.RegularExpressions;
using static CUE4Parse.UE4.Objects.Core.i18N.FTextHistory;

namespace ValoParser
{
    public static class Battlepass
    {

        static JsonObject jsonObject = new JsonObject();
        public static void Parse(GameFile file)
        {
            var provider = Program.provider;
            if (file.Path.StartsWith("ShooterGame/Content/Contracts/Story/") && file.Path.EndsWith("_DataAssetV2.uasset"))
            {
                if (Program.logDetailed) Console.WriteLine(String.Format("Parsing battlepass season \"{0}\"...", file.Name.Replace("_DataAssetV2.uasset", "")));

                var allExports = provider.LoadObjectExports(file.Path);
                var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var jsonNode = JsonNode.Parse(fullJson);
                JsonNode json = jsonNode[2]["Properties"];
                var output = GetBattlePassSeason(json["Season"]["AssetPathName"].ToString());
                output.AsObject().Add("displayName", GetDisplayNamePath(jsonNode[1]["Properties"]["UIData"]["AssetPathName"].ToString()));
                output.AsObject().Add("freeRewardScheduleID", UuidParser.Parse(jsonNode[1]["Properties"]["FreeRewardScheduleID"].ToString()));
                output.AsObject().Add("levels", getBattlePassChapters(jsonNode));

                var uuid = UuidParser.Parse(jsonNode[1]["Properties"]["Uuid"].ToString());

                if (!Directory.Exists(@"./files/battlepass"))
                {
                    Directory.CreateDirectory("./files/battlepass");
                }

                jsonObject.Add(uuid, output);
                if (Program.logDetailed) Console.WriteLine(String.Format("Successfully parsed battlepass season \"{0}\"!", file.Name.Replace("_DataAssetV2.uasset", "")));
            }
        }

        static JsonNode GetBattlePassSeason(String assetPathName)
        {
            var provider = Program.provider;
            var allExports = provider.LoadObjectExports(assetPathName.Replace("/Game", "/ShooterGame/Content").Split(".")[0]);
            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
            var jsonNode = JsonNode.Parse(fullJson);
            JsonNode json = jsonNode[1]["Properties"];
            String type = json["Type"] != null ? json["Type"].ToString() : "CB";
            long startTime = Int64.Parse(json["StartTime"]["Ticks"].ToString());
            long endTime = Int64.Parse(json["EndTime"]["Ticks"].ToString());
            var returnJson = JsonNode.Parse("{}");
            returnJson["type"] = type;
            returnJson["startTime"] = new DateTime(startTime).GetDateTimeFormats('s')[0].ToString() + ".000Z";
            returnJson["endTime"] = new DateTime(endTime).GetDateTimeFormats('s')[0].ToString() + ".000Z";

            return returnJson;
        }

        //For future usage
        static String GetDisplayNamePath(String assetPathName)
        {
            var provider = Program.provider;
            var allExports = provider.LoadObjectExports(assetPathName.Replace("/Game", "/ShooterGame/Content").Split(".")[0]);
            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
            var jsonNode = JsonNode.Parse(fullJson);
            JsonNode json = jsonNode[1]["Properties"];
            String TableId = json["DisplayName"]["TableId"].ToString();

            var stringTable = provider.LoadObjectExports(TableId.Split(".")[0]);
            var tableJson = JsonConvert.SerializeObject(stringTable, Formatting.Indented);
            var jsonNode1 = JsonNode.Parse(tableJson);
            String namespacee = jsonNode1[0]["StringTable"]["TableNamespace"].ToString();
            String key = json["DisplayName"]["Key"].ToString();
            String defaultValue = jsonNode1[0]["StringTable"]["KeysToMetaData"][key].ToString();
            String locres = key + "." + namespacee + "." + defaultValue;

            return locres;
        }

        static JsonNode getBattlePassChapters(JsonNode json)
        {
            var provider = Program.provider;
            JsonArray returnArray = new JsonArray();
            int level = 0;

            foreach (var item in json[2]["Properties"]["Chapters"].AsArray())
            {
                JsonArray itemArray = new JsonArray();
                foreach (var item1 in item["Levels"].AsArray())
                {
                    level++;
                    var itemPath = json[int.Parse(item1["Reward"]["ObjectPath"].ToString().Split(".")[1])];
                    var assetPath = itemPath["Properties"].AsObject().Select(p => p.Value).ToArray()[0]["AssetPathName"].ToString().Split(".")[0];

                    var allExports = provider.LoadObjectExports(assetPath.ToString());
                    var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                    var jsonNode = JsonNode.Parse(fullJson);

                    String uuid = UuidParser.Parse(jsonNode[1]["Properties"]["Uuid"].ToString());

                    JsonObject obj = new JsonObject();
                    obj.Add("uuid", uuid);
                    obj.Add("level", level);
                    obj.Add("type", itemPath["Properties"].AsObject().Select(p => p.Key).ToArray()[0].ToString());
                    obj.Add("xp", int.Parse(item1["XP"].ToString()));
                    obj.Add("vpCost", int.Parse(item1["VPCost"].ToString()));
                    obj.Add("purchasableWithVP", bool.Parse(item1["bPurchasableWithVP"].ToString()));
                    obj.Add("isEpilogue", bool.Parse(item["bIsEpilogue"].ToString()));
                    obj.Add("isFree", false);
                    itemArray.Add(obj);
                }

                foreach (var item1 in item["FreeChapterRewards"].AsArray())
                {
                    var itemPath = json[int.Parse(item1["ObjectPath"].ToString().Split(".")[1])];
                    var assetPath = itemPath["Properties"].AsObject().Select(p => p.Value).ToArray()[0]["AssetPathName"].ToString().Split(".")[0];

                    var allExports = provider.LoadObjectExports(assetPath.ToString());
                    var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                    var jsonNode = JsonNode.Parse(fullJson);

                    String uuid = UuidParser.Parse(jsonNode[1]["Properties"]["Uuid"].ToString());

                    JsonObject obj = new JsonObject();
                    obj.Add("uuid", uuid);
                    obj.Add("level", null);
                    obj.Add("type", itemPath["Properties"].AsObject().Select(p => p.Key).ToArray()[0].ToString());
                    obj.Add("xp", null);
                    obj.Add("vpCost", null);
                    obj.Add("purchasableWithVP", false);
                    obj.Add("isEpilogue", false);
                    obj.Add("isFree", true);
                    itemArray.Add(obj);
                }

                returnArray.Add(itemArray);
            }
            return returnArray;
        }

        public static void Localization(ELanguage lang)
        {
            JsonObject obj = JsonNode.Parse(jsonObject.ToString()).AsObject();
            for (int i = 0; i < jsonObject.Select(p => p.Value).ToArray().Length; i++)
            {
                var all = jsonObject.Select(p => p.Value).ToArray()[i]["displayName"].ToString().Split(".");
                var key = all[0];
                var namespacee = all[1];
                jsonObject.Select(p => p.Value).ToArray()[i]["displayName"] = Program.provider.GetLocalizedString(namespacee, key, all[2]).Replace(@"""", "//MARK_");
            }
            File.WriteAllText(String.Format(@"./files/battlepass/{0}.json", Program.provider.GetLanguageCode(lang)), Regex.Unescape(jsonObject.ToJsonString()).Replace(@"//MARK_", "\\\""), Encoding.UTF8);
            Console.WriteLine(String.Format("Successfully saved battlepass in {0}!", Program.provider.GetLanguageCode(lang)));
            jsonObject = obj;
        }
    }
}
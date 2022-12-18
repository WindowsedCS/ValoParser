using System;
using System.IO;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Linq;
using File = System.IO.File;
using System.Text;
using System.Text.RegularExpressions;
using ValoParser.Parsers;

namespace ValoParser.Endpoints
{
    public static class Contracts
    {

        static JsonObject jsonObject = new JsonObject();
        public static void Parse(GameFile file)
        {
            var provider = Program.provider;
            if (file.Path.StartsWith("ShooterGame/Content/Contracts/") && file.Path.EndsWith("_DataAssetV2.uasset"))
            {
                var allExports = provider.LoadObjectExports(file.Path);
                var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var jsonNode = JsonNode.Parse(fullJson);
                JsonNode json = jsonNode[2]["Properties"];
                string relationType = file.Path.Replace("ShooterGame/Content/Contracts/", "").Split("/")[0];
                string type = null;
                switch (relationType)
                {
                    case "Events":
                        type = "Event";
                        break;
                    case "Story":
                        type = "Season";
                        break;
                    case "Characters":
                        type = "RelatedCharacter";
                        break;
                }
                var output = GetBattlePassSeason(type == null ? null : json[type]["AssetPathName"].ToString(), relationType);
                output.AsObject().Add("displayName", GetDisplayNamePath(jsonNode[1]["Properties"]["UIData"]["AssetPathName"].ToString()));
                output.AsObject().Add("freeRewardScheduleID", UuidParser.Parse(jsonNode[1]["Properties"]["FreeRewardScheduleID"].ToString()));
                output.AsObject().Add("levels", getBattlePassChapters(jsonNode, relationType));

                var uuid = UuidParser.Parse(jsonNode[1]["Properties"]["Uuid"].ToString());

                jsonObject.Add(uuid, output);
            }
        }

        static JsonNode GetBattlePassSeason(string assetPathName, string relationType)
        {
            var returnJson = JsonNode.Parse("{}");
            if (assetPathName != null)
            {
                var provider = Program.provider;
                var allExports = provider.LoadObjectExports(assetPathName.Replace("/Game", "/ShooterGame/Content").Split(".")[0]);
                var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var jsonNode = JsonNode.Parse(fullJson);
                JsonNode json = jsonNode[1]["Properties"];
                string type = null;
                switch (relationType)
                {
                    case "Events":
                        type = "Event";
                        break;
                    case "Story":
                        type = "Season";
                        break;
                    case "Characters":
                        type = "Agent";
                        break;
                    case "NPE":
                        type = "NPE";
                        break;
                }
                returnJson["type"] = type;
                if (json["StartTime"] != null) returnJson["StartTime"] = new DateTime(long.Parse(json["StartTime"]["Ticks"].ToString())).GetDateTimeFormats('s')[0].ToString() + ".000Z";
                if (json["EndTime"] != null) returnJson["StartTime"] = new DateTime(long.Parse(json["EndTime"]["Ticks"].ToString())).GetDateTimeFormats('s')[0].ToString() + ".000Z";
            }
            return returnJson;
        }

        //For future usage
        static string GetDisplayNamePath(string assetPathName)
        {
            var provider = Program.provider;
            var allExports = provider.LoadObjectExports(assetPathName.Replace("/Game", "/ShooterGame/Content").Split(".")[0]);
            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
            var jsonNode = JsonNode.Parse(fullJson);
            JsonNode json = jsonNode[1]["Properties"];
            string TableId = json["DisplayName"]["TableId"].ToString();

            var stringTable = provider.LoadObjectExports(TableId.Split(".")[0]);
            var tableJson = JsonConvert.SerializeObject(stringTable, Formatting.Indented);
            var jsonNode1 = JsonNode.Parse(tableJson);
            string namespacee = jsonNode1[0]["StringTable"]["TableNamespace"].ToString();
            string key = json["DisplayName"]["Key"].ToString();
            string defaultValue = jsonNode1[0]["StringTable"]["KeysToMetaData"][key].ToString();
            string locres = key + "." + namespacee + "." + defaultValue;

            return locres;
        }

        static JsonNode getBattlePassChapters(JsonNode json, string relationType)
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

                    string uuid = UuidParser.Parse(jsonNode[1]["Properties"]["Uuid"].ToString());

                    JsonObject obj = new JsonObject();
                    obj.Add("uuid", uuid);
                    obj.Add("level", level);
                    obj.Add("type", itemPath["Properties"].AsObject().Select(p => p.Key).ToArray()[0].ToString());
                    obj.Add("xp", int.Parse(item1["XP"].ToString()));
                    obj.Add("vpCost", int.Parse(item1["VPCost"].ToString()));
                    obj.Add("purchasableWithVP", bool.Parse(item1["bPurchasableWithVP"].ToString()));
                    if (relationType == "Story")
                    {
                        obj.Add("isEpilogue", bool.Parse(item["bIsEpilogue"].ToString()));
                        obj.Add("isFree", false);
                    }
                    itemArray.Add(obj);
                }

                if (item["FreeChapterRewards"] != null)
                {
                    foreach (var item1 in item["FreeChapterRewards"].AsArray())
                    {
                        var itemPath = json[int.Parse(item1["ObjectPath"].ToString().Split(".")[1])];
                        var assetPath = itemPath["Properties"].AsObject().Select(p => p.Value).ToArray()[0]["AssetPathName"].ToString().Split(".")[0];

                        var allExports = provider.LoadObjectExports(assetPath.ToString());
                        var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                        var jsonNode = JsonNode.Parse(fullJson);

                        string uuid = UuidParser.Parse(jsonNode[1]["Properties"]["Uuid"].ToString());

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
            File.WriteAllText(string.Format(@"./files/contracts/{0}.json", Program.provider.GetLanguageCode(lang)), Regex.Unescape(jsonObject.ToJsonString()).Replace(@"//MARK_", "\\\""), Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully saved contracts in {0}!", Program.provider.GetLanguageCode(lang)));
            jsonObject = obj;
        }
    }
}
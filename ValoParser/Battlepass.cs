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

namespace ValoParser
{
    public static class Battlepass
    {
        public static void parse()
        {
            Console.WriteLine("Parsing battlepass data...");
            var provider = Program.provider;
            var jsonObject = new JsonObject();
            foreach (var file in provider.Files.Values)
            {
                if (file.Path.StartsWith("ShooterGame/Content/Contracts/Story/") && file.Path.EndsWith("_DataAssetV2.uasset"))
                {
                    if (Program.logDetailed) Console.WriteLine(String.Format("Parsing battlepass season \"{0}\"...", file.Name.Replace("_DataAssetV2.uasset", "")));
                    
                    var allExports = provider.LoadObjectExports(file.Path);
                    var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                    var jsonNode = JsonNode.Parse(fullJson);
                    JsonNode json = jsonNode[2]["Properties"];
                    var output = getBattlePassSeason(json["Season"]["AssetPathName"].ToString());
                    output.AsObject().Add("levels", getBattlePassChapters(jsonNode));
                    
                    if (!Directory.Exists(@"./files/battlepass"))
                    {
                        Directory.CreateDirectory("./files/battlepass");
                    }
                    
                    jsonObject.Add(file.Name.Replace("_DataAssetV2p.uasset", ""), output);
                    File.WriteAllText(String.Format(@"./files/battlepass/{0}.json", file.Name.Replace("_DataAssetV2.uasset", "").Replace("Contract_", "")), output.ToJsonString(), Encoding.UTF8);

                    if (Program.logDetailed) Console.WriteLine(String.Format("Successfully parsed battlepass season \"{0}\"!", file.Name.Replace("_DataAssetV2.uasset", "")));
                }
            }

            File.WriteAllText(String.Format(@"./files/battlepass/Battlepass.json"), jsonObject.ToJsonString(), Encoding.UTF8);
            Console.WriteLine("Battlepass data has been parsed successfully!");
        }

        static JsonNode getBattlePassSeason(String assetPathName)
        {
            var provider = Program.provider;
            var allExports = provider.LoadObjectExports(assetPathName.Replace("/Game", "/ShooterGame/Content").Split(".")[0]);
            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
            var jsonNode = JsonNode.Parse(fullJson);
            JsonNode json = jsonNode[1]["Properties"];
            String type = json["Type"] != null ? json["Type"].ToString() : "CB";
            long startTime = Int64.Parse(json["StartTime"]["Ticks"].ToString());
            long endTime = Int64.Parse(json["StartTime"]["Ticks"].ToString());
            var returnJson = JsonNode.Parse("{}");
            returnJson["type"] = type;
            returnJson["startTime"] = startTime;
            returnJson["endTime"] = endTime;

            return returnJson;
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
                    obj.Add("XP", int.Parse(item1["XP"].ToString()));
                    obj.Add("VPCost", int.Parse(item1["VPCost"].ToString()));
                    obj.Add("bPurchasableWithVP", bool.Parse(item1["bPurchasableWithVP"].ToString()));
                    obj.Add("bIsEpilogue", bool.Parse(item["bIsEpilogue"].ToString()));
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
                    obj.Add("type", itemPath["Properties"].AsObject().Select(p => p.Key).ToArray()[0].ToString());
                    obj.Add("isFree", true);
                    itemArray.Add(obj);
                }

                returnArray.Add(itemArray);
            }
            return returnArray;
        }
    }
}
using System;
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
    public static class ContentTiers
    {

        static JsonObject jsonObject = new JsonObject();
        public static void Parse(GameFile file)
        {
            var provider = Program.provider;
            if (file.Path.StartsWith("ShooterGame/Content/ContentTiers/") && file.Path.EndsWith("_PrimaryAsset.uasset"))
            {
                if (Program.logDetailed) Console.WriteLine(string.Format("Parsing battlepass season \"{0}\"...", file.Name.Replace("_DataAssetV2.uasset", "")));
                // PrimaryAsset
                var allExports = provider.LoadObjectExports(file.Path);
                var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                var jsonNode = JsonNode.Parse(fullJson);
                JsonNode jsonPrimary = jsonNode[1]["Properties"];
                // UIData
                var uiDataPath = jsonPrimary["UIData"]["AssetPathName"].ToString();
                var allExports1 = provider.LoadObjectExports(uiDataPath.Split(".")[0]);
                var fullJson1 = JsonConvert.SerializeObject(allExports1, Formatting.Indented);
                var uiData = JsonNode.Parse(fullJson1);
                var uuid = UuidParser.Parse(jsonPrimary["Uuid"].ToString());
                var juiceValue = jsonPrimary["JuiceValue"].ToString();
                var juiceCost = jsonPrimary["JuiceCost"].ToString();
                JsonObject output = new JsonObject();
                if (jsonPrimary["TierRank"] != null)
                {
                    var tierRank = jsonPrimary["TierRank"].ToString();
                    output.Add("tierRank", int.Parse(tierRank));
                }
                else
                {
                    output.Add("tierRank", 0);
                }
                output.Add("juiceValue", int.Parse(juiceValue));
                output.Add("juiceCost", int.Parse(juiceCost));
                // DisplayName
                // Ultra_UIData is not providing DisplayName TableID, instead, it provides Namespace, Key and Sourcestring.
                string locres = "";
                if (uiData[1]["Properties"]["DisplayName"]["TableId"] != null)
                {
                    string TableId = uiData[1]["Properties"]["DisplayName"]["TableId"].ToString();
                    var stringTable = provider.LoadObjectExports(TableId.Split(".")[0]);
                    var tableJson = JsonConvert.SerializeObject(stringTable, Formatting.Indented);
                    var jsonNode1 = JsonNode.Parse(tableJson);
                    string namespacee = jsonNode1[0]["StringTable"]["TableNamespace"].ToString();
                    string key = uiData[1]["Properties"]["DisplayName"]["Key"].ToString();
                    string defaultValue = jsonNode1[0]["StringTable"]["KeysToMetaData"][key].ToString();
                    locres = key + "." + namespacee + "." + defaultValue;
                }
                else
                {
                    string namespacee = uiData[1]["Properties"]["DisplayName"]["Namespace"].ToString();
                    string key = uiData[1]["Properties"]["DisplayName"]["Key"].ToString();
                    string defaultValue = uiData[1]["Properties"]["DisplayName"]["SourceString"].ToString();
                    locres = key + "." + namespacee + "." + defaultValue;
                }
                output.Add("displayName", locres);
                // DisplayIcon
                if (uiData[1]["Properties"]["DisplayIcon"] != null)
                {
                    string path = uiData[1]["Properties"]["DisplayIcon"]["ObjectPath"].ToString();
                    var split = path.Split(".")[0];
                    ImageParser parser = new ImageParser();
                    parser.Parse(split, "contenttiers/" + uuid + "/displayicon.png");
                    output.Add("displayIcon", "https://assets.empressival.com/contenttiers/" + uuid + "/displayicon.png");
                }
                jsonObject.Add(uuid, output);
            }
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
            File.WriteAllText(string.Format(@"./files/contenttiers/{0}.json", Program.provider.GetLanguageCode(lang)), Regex.Unescape(jsonObject.ToJsonString()).Replace(@"//MARK_", "\\\""), Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully saved contenttiers in {0}!", Program.provider.GetLanguageCode(lang)));
            jsonObject = obj;
        }
    }
}
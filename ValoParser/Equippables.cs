using System;
using System.IO;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using CUE4Parse.UE4.Versions;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace ValoParser
{
    public static class Equippables
    {
        public static void weapons()
        {
            var provider = Program.provider;
            Console.WriteLine(String.Format("Parsing weapons data and assets..."));
            if (!Directory.Exists(@"./files/weapons"))
            {
                Directory.CreateDirectory("./files/weapons");
            }
            var jsonArray = new JsonArray();
            var jsonObject = new JsonObject();
            foreach (var file in provider.Files.Values)
            {
                if (file.Path.StartsWith("ShooterGame/Content/Equippables/Guns/") && file.Path.EndsWith("_PrimaryAsset.uasset"))
                {

                    if (file.Path.Split("/")[4] != "_Core" && file.Path.Split("/")[4] != "Attachments" && file.Path.Split("/")[4] != "_Archetypes")
                    {
                        var splited = file.Path.Split("/");
                        if (splited[7].EndsWith("_PrimaryAsset.uasset") || splited[7] == "Chromas" || splited[7] == "Levels")
                        {
                            if (Program.logDetailed) Console.WriteLine(String.Format("Parsing equippables \"{0}\"...", file.Name.Replace("_PrimaryAsset.uasset", "")));
                            // PrimaryAsset
                            var allExports = provider.LoadObjectExports(file.Path);
                            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                            var primaryAsset = JsonNode.Parse(fullJson);
                            String uuid = UuidParser.Parse(primaryAsset[1]["Properties"]["Uuid"].ToString());
                            // UIData
                            var allExports1 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                            var fullJson1 = JsonConvert.SerializeObject(allExports1, Formatting.Indented);
                            var uiData = JsonNode.Parse(fullJson1);
                            // Data Json
                            JsonObject dataObject = new JsonObject();
                            // DisplayIcon
                            if (uiData[1]["Properties"]["DisplayIcon"] != null)
                            {
                                String path = uiData[1]["Properties"]["DisplayIcon"]["ObjectPath"].ToString();
                                var split = path.Split(".")[0];
                                ImageParser parser = new ImageParser();
                                parser.Parse(split, "weapons/" + uuid + "/displayicon.png");
                                dataObject.Add("displayIcon", "https://assets.empressival.com/weapons/" + uuid + "/displayicon.png");
                            }
                            else
                            {
                                dataObject.Add("displayIcon", null);
                            }
                            if (uiData[1]["Properties"]["FullRender"] != null)
                            {
                                String path = uiData[1]["Properties"]["FullRender"]["ObjectPath"].ToString();
                                var split = path.Split(".")[0];
                                ImageParser parser = new ImageParser();
                                parser.Parse(split, "weapons/" + uuid + "/fullrender.png");
                                dataObject.Add("fullRender", "https://assets.empressival.com/weapons/" + uuid + "/fullrender.png");
                            }
                            else
                            {
                                dataObject.Add("fullRender", null);
                            }
                            // Swatch
                            if (uiData[1]["Properties"]["Swatch"] != null)
                            {
                                String path = uiData[1]["Properties"]["Swatch"]["ObjectPath"].ToString();
                                var split = path.Split(".")[0];
                                ImageParser parser = new ImageParser();
                                parser.Parse(split, "weapons/" + uuid + "/swatch.png");
                                dataObject.Add("swatch", "https://assets.empressival.com/weapons/" + uuid + "/swatch.png");
                            }
                            else
                            {
                                dataObject.Add("swatch", null);
                            }
                            // StreamedVideo
                            if (primaryAsset[1]["Properties"]["StreamedVideo"] != null)
                            {
                                var allExports2 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["StreamedVideo"]["AssetPathName"].ToString().Split(".")[0]);
                                var fullJson2 = JsonConvert.SerializeObject(allExports2, Formatting.Indented);
                                var streamedVideo = JsonNode.Parse(fullJson2);
                                if (streamedVideo[1]["Properties"]["Uuid"] != null)
                                {
                                    var uuidVideo = streamedVideo[1]["Properties"]["Uuid"].ToString();
                                    var link = String.Format("https://valorant.dyn.riotcdn.net/x/videos/{0}/{1}_default_universal.mp4", "release-05.10", UuidParser.Parse(uuidVideo));
                                    dataObject.Add("streamedVideo", link);
                                }
                                else
                                {
                                    dataObject.Add("streamedVideo", null);
                                }
                            }
                            else
                            {
                                dataObject.Add("streamedVideo", null);
                            }
                            jsonObject.Add(uuid, dataObject);
                            // DisplayName
                            if (uiData[1]["Properties"]["DisplayName"]["TableId"] != null)
                            {
                                var allExports2 = provider.LoadObjectExports(uiData[1]["Properties"]["DisplayName"]["TableId"].ToString().Split(".")[0]);
                                var fullJson2 = JsonConvert.SerializeObject(allExports2, Formatting.Indented);
                                var stringTable = JsonNode.Parse(fullJson2);

                                var key = uiData[1]["Properties"]["DisplayName"]["Key"].ToString();

                                JsonObject obj = new JsonObject();
                                obj.Add("uuid", uuid);
                                obj.Add("namespacee", stringTable[0]["StringTable"]["TableNamespace"].ToString());
                                obj.Add("key", key);
                                obj.Add("defaultValue", stringTable[0]["StringTable"]["KeysToMetaData"][uiData[1]["Properties"]["DisplayName"]["Key"].ToString()].ToString());
                                jsonArray.Add(obj);
                            }
                            else
                            {
                                if (uiData[1]["Properties"]["DisplayName"]["Namespace"] != null && uiData[1]["Properties"]["DisplayName"]["Key"] != null)
                                {
                                    JsonObject obj = new JsonObject();
                                    obj.Add("uuid", uuid);
                                    obj.Add("namespacee", uiData[1]["Properties"]["DisplayName"]["Namespace"].ToString());
                                    obj.Add("key", uiData[1]["Properties"]["DisplayName"]["Key"].ToString());
                                    obj.Add("defaultValue", "");
                                    jsonArray.Add(obj);
                                }
                            }
                        }
                    }
                }
                else if (file.Path.StartsWith("ShooterGame/Content/Equippables/Melee/") && file.Path.EndsWith("_PrimaryAsset.uasset"))
                {
                    var splited = file.Path.Split("/");
                    if (splited.Length > 5)
                    {
                        if (splited[5].EndsWith("_PrimaryAsset.uasset") || splited[5] == "Chromas" || splited[5] == "Levels")
                        {
                            if (Program.logDetailed) Console.WriteLine(String.Format("Parsing equippables \"{0}\"...", file.Name.Replace("_PrimaryAsset.uasset", "")));
                            // PrimaryAsset
                            var allExports = provider.LoadObjectExports(file.Path);
                            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                            var primaryAsset = JsonNode.Parse(fullJson);
                            String uuid = UuidParser.Parse(primaryAsset[1]["Properties"]["Uuid"].ToString());
                            // UIData
                            var allExports1 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                            var fullJson1 = JsonConvert.SerializeObject(allExports1, Formatting.Indented);
                            var uiData = JsonNode.Parse(fullJson1);
                            // Data Json
                            JsonObject dataObject = new JsonObject();
                            // DisplayIcon
                            if (uiData[1]["Properties"]["DisplayIcon"] != null)
                            {
                                String path = uiData[1]["Properties"]["DisplayIcon"]["ObjectPath"].ToString();
                                var split = path.Split(".")[0];
                                ImageParser parser = new ImageParser();
                                parser.Parse(split, "weapons/" + uuid + "/displayicon.png");
                                dataObject.Add("displayIcon", "https://assets.empressival.com/weapons/" + uuid + "/displayicon.png");
                            }
                            else
                            {
                                dataObject.Add("displayIcon", null);
                            }
                            if (uiData[1]["Properties"]["FullRender"] != null)
                            {
                                String path = uiData[1]["Properties"]["FullRender"]["ObjectPath"].ToString();
                                var split = path.Split(".")[0];
                                ImageParser parser = new ImageParser();
                                parser.Parse(split, "weapons/" + uuid + "/fullrender.png");
                                dataObject.Add("fullRender", "https://assets.empressival.com/weapons/" + uuid + "/fullrender.png");
                            }
                            else
                            {
                                dataObject.Add("fullRender", null);
                            }
                            // Swatch
                            if (uiData[1]["Properties"]["Swatch"] != null)
                            {
                                String path = uiData[1]["Properties"]["Swatch"]["ObjectPath"].ToString();
                                var split = path.Split(".")[0];
                                ImageParser parser = new ImageParser();
                                parser.Parse(split, "weapons/" + uuid + "/swatch.png");
                                dataObject.Add("swatch", "https://assets.empressival.com/weapons/" + uuid + "/swatch.png");
                            }
                            else
                            {
                                dataObject.Add("swatch", null);
                            }
                            // StreamedVideo
                            if (primaryAsset[1]["Properties"]["StreamedVideo"] != null)
                            {
                                var allExports2 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["StreamedVideo"]["AssetPathName"].ToString().Split(".")[0]);
                                var fullJson2 = JsonConvert.SerializeObject(allExports2, Formatting.Indented);
                                var streamedVideo = JsonNode.Parse(fullJson2);
                                if (streamedVideo[1]["Properties"]["Uuid"] != null)
                                {
                                    var uuidVideo = streamedVideo[1]["Properties"]["Uuid"].ToString();
                                    var link = String.Format("https://valorant.dyn.riotcdn.net/x/videos/{0}/{1}_default_universal.mp4", "release-05.10", UuidParser.Parse(uuidVideo));
                                    dataObject.Add("streamedVideo", link);
                                }
                                else
                                {
                                    dataObject.Add("streamedVideo", null);
                                }
                            }
                            else
                            {
                                dataObject.Add("streamedVideo", null);
                            }
                            jsonObject.Add(uuid, dataObject);
                            // DisplayName
                            if (uiData[1]["Properties"]["DisplayName"]["TableId"] != null)
                            {
                                var allExports2 = provider.LoadObjectExports(uiData[1]["Properties"]["DisplayName"]["TableId"].ToString().Split(".")[0]);
                                var fullJson2 = JsonConvert.SerializeObject(allExports2, Formatting.Indented);
                                var stringTable = JsonNode.Parse(fullJson2);

                                var key = uiData[1]["Properties"]["DisplayName"]["Key"].ToString();

                                JsonObject obj = new JsonObject();
                                obj.Add("uuid", uuid);
                                obj.Add("namespacee", stringTable[0]["StringTable"]["TableNamespace"].ToString());
                                obj.Add("key", key);
                                obj.Add("defaultValue", stringTable[0]["StringTable"]["KeysToMetaData"][uiData[1]["Properties"]["DisplayName"]["Key"].ToString()].ToString());
                                jsonArray.Add(obj);
                            }
                            else
                            {
                                if (uiData[1]["Properties"]["DisplayName"]["Namespace"] != null && uiData[1]["Properties"]["DisplayName"]["Key"] != null)
                                {
                                    JsonObject obj = new JsonObject();
                                    obj.Add("uuid", uuid);
                                    obj.Add("namespacee", uiData[1]["Properties"]["DisplayName"]["Namespace"].ToString());
                                    obj.Add("key", uiData[1]["Properties"]["DisplayName"]["Key"].ToString());
                                    obj.Add("defaultValue", "");
                                    jsonArray.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            foreach (var lang in Program.languageCodes)
            {
                provider.LoadLocalization(lang);
                var output = Regex.Replace(Regex.Unescape(getLanguageStrings(jsonArray, jsonObject).ToJsonString()), @"\r\n?|\n", "\\n");
                File.WriteAllText(String.Format(@"./files/weapons/weapons_{0}.json", provider.GetLanguageCode(lang)), output, Encoding.UTF8);
                if (provider.GetLanguageCode(lang) == "en-US")
                {
                    File.WriteAllText(String.Format(@"./files/weapons.json", provider.GetLanguageCode(lang)), output, Encoding.UTF8);
                }
                Console.WriteLine(String.Format("Weapons data for language {0} has been successfully parsed!", provider.GetLanguageCode(lang)));
            }
            Console.WriteLine("Equippables data has been successfully parsed!");
        }

        private static JsonObject getLanguageStrings(JsonArray jsonArray, JsonObject jsonObject)
        {
            JsonObject languageStrings = new JsonObject();
            for (var i = 0; i < jsonArray.Count; i++)
            {
                var uuid = jsonArray[i]["uuid"].ToString();
                string localized = Program.provider.GetLocalizedString(jsonArray[i]["namespacee"].ToString(), jsonArray[i]["key"].ToString(), jsonArray[i]["defaultValue"].ToString());
                jsonObject[uuid]["displayName"] = localized;
                languageStrings.Add(uuid, jsonObject[uuid].Deserialize<JsonNode>());
            }
            return languageStrings;
        }
    }
}
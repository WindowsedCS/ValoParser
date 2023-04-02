using System;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using CUE4Parse.UE4.Versions;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using File = System.IO.File;
using CUE4Parse.FileProvider;
using System.Threading.Tasks;
using ValoParser.Parsers;

namespace ValoParser.Endpoints
{
    public static class Equippables
    {

        private static JsonArray jsonArray = new JsonArray();
        private static JsonObject jsonObject = new JsonObject();

        public static void Weapons(GameFile file)
        {
            var provider = Program.provider;
            if (file.Path.StartsWith("ShooterGame/Content/Equippables/Guns/") && file.Path.EndsWith("_PrimaryAsset.uasset"))
            {
                if (file.Path.Split("/")[4] != "_Core" && file.Path.Split("/")[4] != "Attachments" && file.Path.Split("/")[4] != "_Archetypes")
                {
                    var splited = file.Path.Split("/");
                    if (splited[7].EndsWith("_PrimaryAsset.uasset"))
                    {
                        if (Program.logDetailed) Console.WriteLine(string.Format("Parsing equippables \"{0}\"...", file.Name.Replace("_PrimaryAsset.uasset", "")));
                        try
                        {
                            // PrimaryAsset
                            var allExports = provider.LoadObjectExports(file.Path);
                            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                            var primaryAsset = JsonNode.Parse(fullJson);
                            // Theme
                            var allExports1 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["Theme"]["AssetPathName"].ToString().Split(".")[0]);
                            var fullJson1 = JsonConvert.SerializeObject(allExports1, Formatting.Indented);
                            var themeAsset = JsonNode.Parse(fullJson1);
                            var uuid = UuidParser.Parse(themeAsset[1]["Properties"]["Uuid"].ToString());
                            // Parse Each
                            var levelsJson = JsonNode.Parse(primaryAsset[1]["Properties"]["Levels"].ToString()).AsArray();
                            var chromasJson = JsonNode.Parse(primaryAsset[1]["Properties"]["Chromas"].ToString()).AsArray();
                            ParseGuns(file.Path, uuid);
                            foreach (var level in levelsJson)
                            {
                                ParseGuns(level["AssetPathName"].ToString().Split(".")[0], uuid);
                            }
                            foreach (var chroma in chromasJson)
                            {
                                ParseGuns(chroma["AssetPathName"].ToString().Split(".")[0], uuid);
                            }
                        } catch (Exception ex)
                        {
                            if (Program.logDetailed) Console.WriteLine(string.Format("Found error when parsing equippables {0}!\n{1}", file.Name.Replace("_PrimaryAsset.uasset", ""), ex));
                        }
                    }
                }
            }
            else if (file.Path.StartsWith("ShooterGame/Content/Equippables/Melee/") && file.Path.EndsWith("_PrimaryAsset.uasset"))
            {
                var splited = file.Path.Split("/");
                if (splited.Length > 5)
                {
                    if (splited[5].EndsWith("_PrimaryAsset.uasset"))
                    {
                        if (Program.logDetailed) Console.WriteLine(string.Format("Parsing equippables \"{0}\"...", file.Name.Replace("_PrimaryAsset.uasset", "")));
                        try
                        {

                            var allExports = provider.LoadObjectExports(file.Path);
                            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
                            var primaryAsset = JsonNode.Parse(fullJson);
                            // Theme
                            var allExports1 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["Theme"]["AssetPathName"].ToString().Split(".")[0]);
                            var fullJson1 = JsonConvert.SerializeObject(allExports1, Formatting.Indented);
                            var themeAsset = JsonNode.Parse(fullJson1);
                            var uuid = UuidParser.Parse(themeAsset[1]["Properties"]["Uuid"].ToString());
                            // Parse Each
                            var levelsJson = JsonNode.Parse(primaryAsset[1]["Properties"]["Levels"].ToString()).AsArray();
                            var chromasJson = JsonNode.Parse(primaryAsset[1]["Properties"]["Chromas"].ToString()).AsArray();
                            ParseGuns(file.Path, uuid);
                            foreach (var level in levelsJson)
                            {
                                ParseGuns(level["AssetPathName"].ToString().Split(".")[0], uuid);
                            }
                            foreach (var chroma in chromasJson)
                            {
                                ParseGuns(chroma["AssetPathName"].ToString().Split(".")[0], uuid);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Program.logDetailed) Console.WriteLine(string.Format("Found error when parsing equippables {0}!\n{1}", file.Name.Replace("_PrimaryAsset.uasset", ""), ex));
                        }
                    }
                }
            }
        }

        public static async Task AddVpCost()
        {
            Task<object> storeOffersRequest = Program.User.Store.GetStoreOffers();
            await storeOffersRequest;

            JsonObject storeOffers = JsonNode.Parse(storeOffersRequest.Result.ToString()).AsObject();
            foreach (var offer in storeOffers["Offers"].AsArray())
            {
                if (jsonObject[offer["OfferID"].ToString()] != null)
                {
                    jsonObject[offer["OfferID"].ToString()]["vpCost"] = int.Parse(offer["Cost"]["85ad13f7-3d1b-5128-9eb2-7cd8ee0b5741"].ToString());
                }
            }
        }

        public static void Localization(ELanguage lang)
        {
            var output = Regex.Replace(Regex.Unescape(GetLanguageStrings(jsonArray, jsonObject).ToJsonString()), @"\r\n?|\n", "\\n").Replace(@"//MARK_", "\\\"");
            File.WriteAllText(string.Format(@"./files/weapons/{0}.json", Program.provider.GetLanguageCode(lang)), output, Encoding.UTF8);
            Console.WriteLine(string.Format("Successfully saved weapons in {0}!", Program.provider.GetLanguageCode(lang)));
        }

        private static JsonObject GetLanguageStrings(JsonArray jsonArray, JsonObject jsonObject)
        {
            JsonObject languageStrings = new JsonObject();
            for (var i = 0; i < jsonArray.Count; i++)
            {
                var uuid = jsonArray[i]["uuid"].ToString();
                string localized = Program.provider.GetLocalizedString(jsonArray[i]["namespacee"].ToString(), jsonArray[i]["key"].ToString(), jsonArray[i]["defaultValue"].ToString());
                jsonObject[uuid]["displayName"] = localized.Replace(@"""", "//MARK_");
                languageStrings.Add(uuid, jsonObject[uuid].Deserialize<JsonNode>());
            }
            return languageStrings;
        }

        private static void ParseGuns(String file, String themeUuid)
        {
            DefaultFileProvider provider = Program.provider;
            // PrimaryAsset
            var allExports = provider.LoadObjectExports(file);
            var fullJson = JsonConvert.SerializeObject(allExports, Formatting.Indented);
            var primaryAsset = JsonNode.Parse(fullJson);
            string uuid = UuidParser.Parse(primaryAsset[1]["Properties"]["Uuid"].ToString());
            // UIData
            var allExports1 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["UIData"]["AssetPathName"].ToString().Split(".")[0]);
            var fullJson1 = JsonConvert.SerializeObject(allExports1, Formatting.Indented);
            var uiData = JsonNode.Parse(fullJson1);
            // Data Json
            JsonObject dataObject = new JsonObject();
            // DisplayIcon
            if (uiData[1]["Properties"]["DisplayIcon"] != null)
            {
                string path = uiData[1]["Properties"]["DisplayIcon"]["ObjectPath"].ToString();
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
                string path = uiData[1]["Properties"]["FullRender"]["ObjectPath"].ToString();
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
                string path = uiData[1]["Properties"]["Swatch"]["ObjectPath"].ToString();
                var split = path.Split(".")[0];
                ImageParser parser = new ImageParser();
                parser.Parse(split, "weapons/" + uuid + "/swatch.png");
                dataObject.Add("swatch", "https://assets.empressival.com/weapons/" + uuid + "/swatch.png");
            }
            else
            {
                dataObject.Add("swatch", null);
            }
            // Theme
            dataObject.Add("theme", "https://media.valorant-api.com/themes/" + themeUuid + "/displayicon.png");
            // StreamedVideo
            if (primaryAsset[1]["Properties"]["StreamedVideo"] != null)
            {
                var allExports2 = provider.LoadObjectExports(primaryAsset[1]["Properties"]["StreamedVideo"]["AssetPathName"].ToString().Split(".")[0]);
                var fullJson2 = JsonConvert.SerializeObject(allExports2, Formatting.Indented);
                var streamedVideo = JsonNode.Parse(fullJson2);
                if (streamedVideo[1]["Properties"]["Uuid"] != null)
                {
                    var uuidVideo = streamedVideo[1]["Properties"]["Uuid"].ToString();
                    var link = string.Format("https://valorant.dyn.riotcdn.net/x/videos/{0}/{1}_default_universal.mp4", Program.version["branch"].ToString(), UuidParser.Parse(uuidVideo));
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
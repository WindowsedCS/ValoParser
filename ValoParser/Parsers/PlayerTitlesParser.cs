using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class PlayerTitlesParser
    {
        public PlayerTitlesParser() { }

        JsonArray array = new JsonArray();

        public void getPlayerTitlesContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Personalization/Titles") && file.Path.EndsWith("_PrimaryAsset.uasset"))
                {
                    JsonObject json = new JsonObject();

                    // Package: PrimaryAsset
                    JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                    JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];
                    JsonNode Strings;

                    // Uuid
                    string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                    json.Add("uuid", uuid);

                    // TitleText
                    if (PrimaryAssetProperties["TitleText"] != null)
                    {
                        Strings = UassetUtil.loadJson(PrimaryAssetProperties["TitleText"]["TableId"].ToString());
                        JsonObject TitleText = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", PrimaryAssetProperties["TitleText"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][PrimaryAssetProperties["TitleText"]["Key"].ToString()].ToString() }
                        };
                        json.Add("titleText", TitleText);
                    } else
                    {
                        json.Add("titleText", null);
                    }

                    // TitleTextAllCaps
                    if (PrimaryAssetProperties["TitleTextAllCaps"] != null)
                    {
                        Strings = UassetUtil.loadJson(PrimaryAssetProperties["TitleTextAllCaps"]["TableId"].ToString());
                        JsonObject TitleTextAllCaps = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", PrimaryAssetProperties["TitleTextAllCaps"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][PrimaryAssetProperties["TitleTextAllCaps"]["Key"].ToString()].ToString() }
                        };
                        json.Add("titleTextAllCaps", TitleTextAllCaps);
                    } else
                    {
                        json.Add("titleTextAllCaps", null);
                    }

                    if (PrimaryAssetProperties["bHideIfNotOwned"] != null)
                    {
                        json.Add("hideIfNotOwned", (bool)PrimaryAssetProperties["bHideIfNotOwned"]);
                    } else
                    {
                        json.Add("hideIfNotOwned", false);
                    }

                    // Package: UIData
                    JsonNode UIData = UassetUtil.loadFullJson(PrimaryAssetProperties["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                    JsonNode UIDataProperties = UIData[1]["Properties"];

                    // DisplayName
                    if (PrimaryAssetProperties["TitleText"] != null)
                    {
                        Strings = UassetUtil.loadJson(UIDataProperties["DisplayName"]["TableId"].ToString());
                        JsonObject DisplayName = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayName"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayName"]["Key"].ToString()].ToString() }
                        };
                        json.Add("displayName", DisplayName);
                    } else
                    {
                        json.Add("displayName", null);
                    }

                    // DisplayNameAllCaps
                    if (PrimaryAssetProperties["TitleText"] != null)
                    {
                        Strings = UassetUtil.loadJson(UIDataProperties["DisplayNameAllCaps"]["TableId"].ToString());
                        JsonObject DisplayNameAllCaps = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAllCaps"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayNameAllCaps"]["Key"].ToString()].ToString() }
                        };
                        json.Add("displayNameAllCaps", DisplayNameAllCaps);
                    } else
                    {
                        json.Add("displayNameAllCaps", null);
                    }

                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/playertitles/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, playertitle =>
            {
                // DisplayName
                if (playertitle["titleText"] != null)
                    playertitle["titleText"] = Program.provider.GetLocalizedString(playertitle["titleText"]["TableId"].ToString(), playertitle["titleText"]["Key"].ToString(), playertitle["titleText"]["Default"].ToString());

                // DisplayNameAllCaps
                if (playertitle["titleTextAllCaps"] != null)
                    playertitle["titleTextAllCaps"] = Program.provider.GetLocalizedString(playertitle["titleTextAllCaps"]["TableId"].ToString(), playertitle["titleTextAllCaps"]["Key"].ToString(), playertitle["titleTextAllCaps"]["Default"].ToString());

                // DisplayName
                if (playertitle["displayName"] != null)
                    playertitle["displayName"] = Program.provider.GetLocalizedString(playertitle["displayName"]["TableId"].ToString(), playertitle["displayName"]["Key"].ToString(), playertitle["displayName"]["Default"].ToString());

                // DisplayNameAllCaps
                if (playertitle["displayNameAllCaps"] != null)
                    playertitle["displayNameAllCaps"] = Program.provider.GetLocalizedString(playertitle["displayNameAllCaps"]["TableId"].ToString(), playertitle["displayNameAllCaps"]["Key"].ToString(), playertitle["displayNameAllCaps"]["Default"].ToString());
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/playertitles/{0}.json", locale));
        }
    }
}

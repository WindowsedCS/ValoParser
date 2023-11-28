using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class ContentTiersParser
    {
        public ContentTiersParser() { }

        JsonArray array = new JsonArray();

        public void getContentTiersContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/ContentTiers") && file.Path.EndsWith("_PrimaryAsset.uasset"))
                {
                    JsonObject json = new JsonObject();

                    // Package: PrimaryAsset
                    JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                    JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];

                    // Uuid
                    string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                    json.Add("uuid", uuid);

                    // TierRank
                    json.Add("tierRank", PrimaryAssetProperties["TierRank"] != null ? (int)PrimaryAssetProperties["TierRank"] : 0);

                    // JuiceValue
                    json.Add("juiceValue", (int)PrimaryAssetProperties["JuiceValue"]);

                    // JuiceCost
                    json.Add("juiceCost", (int)PrimaryAssetProperties["JuiceCost"]);

                    // Package: UIData
                    JsonNode UIData = UassetUtil.loadFullJson(PrimaryAssetProperties["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                    JsonNode UIDataProperties = UIData[1]["Properties"];

                    // HighlightColor
                    json.Add("highlightColor", UIDataProperties["HighlightColor"]["SpecifiedColor"]["Hex"].ToString());

                    if (UIData[1]["Properties"]["DisplayName"]["TableId"] != null)
                    {
                        JsonNode Strings = UassetUtil.loadJson(UIData[1]["Properties"]["DisplayName"]["TableId"].ToString());

                        // DisplayName
                        JsonObject DisplayName = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayName"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayName"]["Key"].ToString()].ToString() }
                        };
                        json.Add("displayName", DisplayName);

                        // DisplayNameAllCaps
                        JsonObject DisplayNameAllCaps = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAllCaps"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayNameAllCaps"]["Key"].ToString()].ToString() }
                        };
                        json.Add("displayNameAllCaps", DisplayNameAllCaps);

                        // DisplayNameAllCaps
                        JsonObject DisplayNameAbbreviatedAllCaps = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAbbreviatedAllCaps"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayNameAbbreviatedAllCaps"]["Key"].ToString()].ToString() }
                        };
                        json.Add("displayNameAbbreviatedAllCaps", DisplayNameAbbreviatedAllCaps);
                    }
                    else 
                    {
                        // DisplayName
                        JsonObject DisplayName = new JsonObject
                        {
                            { "TableId", UIDataProperties["DisplayName"]["Namespace"].ToString() },
                            { "Key", UIDataProperties["DisplayName"]["Key"].ToString() },
                            { "Default", UIDataProperties["DisplayName"]["LocalizedString"].ToString() }
                        };
                        json.Add("displayName", DisplayName);

                        // DisplayNameAllCaps
                        JsonObject DisplayNameAllCaps = new JsonObject
                        {
                            { "TableId", UIDataProperties["DisplayName"]["Namespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAllCaps"]["Key"].ToString() },
                            { "Default", UIDataProperties["DisplayNameAllCaps"]["LocalizedString"].ToString() }
                        };
                        json.Add("displayNameAllCaps", DisplayNameAllCaps);

                        // DisplayNameAllCaps
                        JsonObject DisplayNameAbbreviatedAllCaps = new JsonObject
                        {
                            { "TableId", UIDataProperties["DisplayName"]["Namespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAbbreviatedAllCaps"]["Key"].ToString() },
                            { "Default", UIDataProperties["DisplayNameAbbreviatedAllCaps"]["LocalizedString"].ToString() }
                        };
                        json.Add("displayNameAbbreviatedAllCaps", DisplayNameAbbreviatedAllCaps);
                    }

                    // DisplayIcon
                    UassetUtil.exportImage(UIDataProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/contenttiers/{0}/displayicon.png", uuid));
                    json.Add("displayIcon", string.Format("https://assets.vallianty.com/contenttiers/{0}/displayicon.png", uuid));

                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/contenttiers/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = array;
            Parallel.ForEach(LocalizedArray, ceremony =>
            {
                if (ceremony["displayName"]["TableId"].ToString() != "")
                {
                    // DisplayName
                    ceremony["displayName"] = Program.provider.GetLocalizedString(ceremony["displayName"]["TableId"].ToString(), ceremony["displayName"]["Key"].ToString(), ceremony["displayName"]["Default"].ToString());

                    // DisplayNameAllCaps
                    ceremony["displayNameAllCaps"] = Program.provider.GetLocalizedString(ceremony["displayNameAllCaps"]["TableId"].ToString(), ceremony["displayNameAllCaps"]["Key"].ToString(), ceremony["displayNameAllCaps"]["Default"].ToString());

                    // DisplayNameAbbreviatedAllCaps
                    ceremony["displayNameAbbreviatedAllCaps"] = Program.provider.GetLocalizedString(ceremony["displayNameAbbreviatedAllCaps"]["TableId"].ToString(), ceremony["displayNameAbbreviatedAllCaps"]["Key"].ToString(), ceremony["displayNameAbbreviatedAllCaps"]["Default"].ToString());
                } else
                {
                    // DisplayName
                    ceremony["displayName"] = ceremony["displayName"]["Default"].ToString();

                    // DisplayNameAllCaps
                    ceremony["displayNameAllCaps"] = ceremony["displayNameAllCaps"]["Default"].ToString();

                    // DisplayNameAbbreviatedAllCaps
                    ceremony["displayNameAbbreviatedAllCaps"] = ceremony["displayNameAbbreviatedAllCaps"]["Default"].ToString();
                }
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/contenttiers/{0}.json", locale));
        }
    }
}

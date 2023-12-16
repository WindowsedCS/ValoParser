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
                if (file.Path.StartsWith("ShooterGame/Content/ContentTiers/") && file.Path.EndsWith("_PrimaryAsset.uasset"))
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

                        // DisplayNameAbbreviatedAllCaps
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
                            { "Default", UIDataProperties["DisplayName"]["SourceString"].ToString() }
                        };
                        json.Add("displayName", DisplayName);

                        // DisplayNameAllCaps
                        JsonObject DisplayNameAllCaps = new JsonObject
                        {
                            { "TableId", UIDataProperties["DisplayName"]["Namespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAllCaps"]["Key"].ToString() },
                            { "Default", UIDataProperties["DisplayNameAllCaps"]["SourceString"].ToString() }
                        };
                        json.Add("displayNameAllCaps", DisplayNameAllCaps);

                        // DisplayNameAbbreviatedAllCaps
                        JsonObject DisplayNameAbbreviatedAllCaps = new JsonObject
                        {
                            { "TableId", UIDataProperties["DisplayName"]["Namespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameAbbreviatedAllCaps"]["Key"].ToString() },
                            { "Default", UIDataProperties["DisplayNameAbbreviatedAllCaps"]["SourceString"].ToString() }
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
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, contenttier =>
            {
                // DisplayName
                if (contenttier["displayName"]["TableId"].ToString() != "")
                {
                    contenttier["displayName"] = Program.provider.GetLocalizedString(contenttier["displayName"]["TableId"].ToString(), contenttier["displayName"]["Key"].ToString(), contenttier["displayName"]["Default"].ToString());
                } 
                else
                {
                    contenttier["displayName"] = Program.provider.GetLocalizedString("\"\"", contenttier["displayName"]["Key"].ToString(), contenttier["displayName"]["Default"].ToString());
                }

                // DisplayNameAllCaps
                if (contenttier["displayNameAllCaps"]["TableId"].ToString() != "")
                {
                    contenttier["displayNameAllCaps"] = Program.provider.GetLocalizedString(contenttier["displayNameAllCaps"]["TableId"].ToString(), contenttier["displayNameAllCaps"]["Key"].ToString(), contenttier["displayNameAllCaps"]["Default"].ToString());
                }
                else
                {
                    contenttier["displayNameAllCaps"] = Program.provider.GetLocalizedString("\"\"", contenttier["displayNameAllCaps"]["Key"].ToString(), contenttier["displayNameAllCaps"]["Default"].ToString());
                }

                // DisplayNameAbbreviatedAllCaps
                if (contenttier["displayNameAbbreviatedAllCaps"]["TableId"].ToString() != "")
                {
                    contenttier["displayNameAbbreviatedAllCaps"] = Program.provider.GetLocalizedString(contenttier["displayNameAbbreviatedAllCaps"]["TableId"].ToString(), contenttier["displayNameAbbreviatedAllCaps"]["Key"].ToString(), contenttier["displayNameAbbreviatedAllCaps"]["Default"].ToString());
                }
                else
                {
                    contenttier["displayNameAbbreviatedAllCaps"] = Program.provider.GetLocalizedString("\"\"", contenttier["displayNameAbbreviatedAllCaps"]["Key"].ToString(), contenttier["displayNameAbbreviatedAllCaps"]["Default"].ToString());
                }
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/contenttiers/{0}.json", locale));
        }
    }
}

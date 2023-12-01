using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class ThemesParser
    {
        public ThemesParser() { }

        JsonArray array = new JsonArray();

        public void getThemesContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Themes") && file.Path.EndsWith("_PrimaryAsset.uasset"))
                {
                    JsonObject json = new JsonObject();

                    // Package: PrimaryAsset
                    JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                    JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];

                    // Uuid
                    string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                    json.Add("uuid", uuid);

                    // Package: UIData
                    JsonNode UIData = UassetUtil.loadFullJson(PrimaryAssetProperties["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                    JsonNode UIDataProperties = UIData[1]["Properties"];
                    JsonNode Strings;

                    // DisplayName
                    if (UIDataProperties["DisplayName"] != null)
                    {
                        if (UIDataProperties["DisplayName"]["TableId"] != null)
                        {
                            Strings = UassetUtil.loadJson(UIData[1]["Properties"]["DisplayName"]["TableId"].ToString());
                            JsonObject DisplayName = new JsonObject
                            {
                                { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                                { "Key", UIDataProperties["DisplayName"]["Key"].ToString() },
                                { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayName"]["Key"].ToString()].ToString() }
                            };
                            json.Add("displayName", DisplayName);
                        } else
                        {
                            if (UIDataProperties["DisplayName"]["CultureInvariantString"] != null)
                            {
                                json.Add("displayName", UIDataProperties["DisplayName"]["CultureInvariantString"].ToString());
                            } else
                            {
                                JsonObject DisplayName = new JsonObject
                            {
                                { "TableId", UIDataProperties["DisplayName"]["Namespace"].ToString() == "" ? "\"\"" : UIDataProperties["DisplayName"]["Namespace"].ToString() },
                                { "Key", UIDataProperties["DisplayName"]["Key"].ToString() },
                                { "Default", UIDataProperties["DisplayName"]["SourceString"].ToString() }
                            };
                                json.Add("displayName", DisplayName);
                            }
                        }
                    }

                    // DisplayNameAllCaps
                    if (UIDataProperties["DisplayNameAllCaps"] != null)
                    {
                        Strings = UassetUtil.loadJson(UIData[1]["Properties"]["DisplayNameAllCaps"]["TableId"].ToString());
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

                    // DisplayIcon
                    if (UIDataProperties["DisplayIcon"] != null)
                    {
                        UassetUtil.exportImage(UIDataProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/themes/{0}/displayicon.png", uuid));
                        json.Add("displayIcon", string.Format("https://assets.vallianty.com/themes/{0}/displayicon.png", uuid));
                    } else
                    {
                        json.Add("displayIcon", null);
                    }

                    // StoreFeaturedImage
                    if (UIDataProperties["StoreFeaturedImage"] != null)
                    {
                        UassetUtil.exportImage(UIDataProperties["StoreFeaturedImage"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/themes/{0}/storefeaturedimage.png", uuid));
                        json.Add("storeFeaturedImage", string.Format("https://assets.vallianty.com/themes/{0}/storefeaturedimage.png", uuid));
                    } else
                    {
                        json.Add("storeFeaturedImage", null);
                    }

                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/themes/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, levelborder =>
            {
                // DisplayName
                if (levelborder["displayName"].ToJsonString().StartsWith("{"))
                    levelborder["displayName"] = Program.provider.GetLocalizedString(levelborder["displayName"]["TableId"].ToString(), levelborder["displayName"]["Key"].ToString(), levelborder["displayName"]["Default"].ToString());

                // DisplayNameAllCaps
                if (levelborder["displayNameAllCaps"] != null)
                    levelborder["displayNameAllCaps"] = Program.provider.GetLocalizedString(levelborder["displayNameAllCaps"]["TableId"].ToString(), levelborder["displayNameAllCaps"]["Key"].ToString(), levelborder["displayNameAllCaps"]["Default"].ToString());
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/themes/{0}.json", locale));
        }
    }
}

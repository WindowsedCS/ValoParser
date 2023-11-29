using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class LevelBordersParser
    {
        public LevelBordersParser() { }

        JsonArray array = new JsonArray();

        public void getLevelBordersContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Personalization/LevelBorders") && file.Path.EndsWith("_PrimaryAsset.uasset"))
                {
                    JsonObject json = new JsonObject();

                    // Package: PrimaryAsset
                    JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                    JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];

                    // Uuid
                    string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                    json.Add("uuid", uuid);

                    // StartingLevel
                    json.Add("startingLevel", (int)PrimaryAssetProperties["StartingLevel"]);

                    // Package: UIData
                    JsonNode UIData = UassetUtil.loadFullJson(PrimaryAssetProperties["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                    JsonNode UIDataProperties = UIData[1]["Properties"];
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

                    // LevelNumberAppearance
                    UassetUtil.exportImage(UIDataProperties["LevelNumberAppearance"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/levelborders/{0}/levelnumberappearance.png", uuid));
                    json.Add("levelNumberAppearance", string.Format("https://assets.vallianty.com/levelborders/{0}/levelnumberappearance.png", uuid));

                    // SmallPlayerCardAppearance
                    UassetUtil.exportImage(UIDataProperties["SmallPlayerCardAppearance"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/levelborders/{0}/smallplayercardappearance.png", uuid));
                    json.Add("smallPlayerCardAppearance", string.Format("https://assets.vallianty.com/levelborders/{0}/smallplayercardappearance.png", uuid));

                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/levelborders/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, levelborder =>
            {
                // DisplayName
                levelborder["displayName"] = Program.provider.GetLocalizedString(levelborder["displayName"]["TableId"].ToString(), levelborder["displayName"]["Key"].ToString(), levelborder["displayName"]["Default"].ToString());

                // DisplayNameAllCaps
                levelborder["displayNameAllCaps"] = Program.provider.GetLocalizedString(levelborder["displayNameAllCaps"]["TableId"].ToString(), levelborder["displayNameAllCaps"]["Key"].ToString(), levelborder["displayNameAllCaps"]["Default"].ToString());
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/levelborders/{0}.json", locale));
        }
    }
}

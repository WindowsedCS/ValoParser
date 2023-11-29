using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class CurrenciesParser
    {
        public CurrenciesParser() { }

        JsonArray array = new JsonArray();

        public void getCurrenciesContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Currencies") && file.Path.EndsWith("_DataAsset.uasset") && file.Path.Split("/")[3].StartsWith("Currency_"))
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

                    // DisplayNameSingular
                    JsonObject DisplayNameSingular = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameSingular"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayNameSingular"]["Key"].ToString()].ToString() }
                        };
                    json.Add("displayNameSingular", DisplayNameSingular);

                    // DisplayNameSingularAllCaps
                    JsonObject DisplayNameSingularAllCaps = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["DisplayNameSingularAllCaps"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["DisplayNameSingularAllCaps"]["Key"].ToString()].ToString() }
                        };
                    json.Add("displayNameSingularAllCaps", DisplayNameSingularAllCaps);

                    // DisplayIcon
                    UassetUtil.exportImage(UIDataProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/currencies/{0}/displayicon.png", uuid));
                    json.Add("displayIcon", string.Format("https://assets.vallianty.com/currencies/{0}/displayicon.png", uuid));

                    // LargeIcon
                    UassetUtil.exportImage(UIDataProperties["LargeIcon"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/currencies/{0}/largeicon.png", uuid));
                    json.Add("largeIcon", string.Format("https://assets.vallianty.com/currencies/{0}/largeicon.png", uuid));

                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/currencies/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, currency =>
            {
                // DisplayName
                currency["displayName"] = Program.provider.GetLocalizedString(currency["displayName"]["TableId"].ToString(), currency["displayName"]["Key"].ToString(), currency["displayName"]["Default"].ToString());

                // DisplayNameAllCaps
                currency["displayNameAllCaps"] = Program.provider.GetLocalizedString(currency["displayNameAllCaps"]["TableId"].ToString(), currency["displayNameAllCaps"]["Key"].ToString(), currency["displayNameAllCaps"]["Default"].ToString());

                // DisplayNameSingular
                currency["displayNameSingular"] = Program.provider.GetLocalizedString(currency["displayNameSingular"]["TableId"].ToString(), currency["displayNameSingular"]["Key"].ToString(), currency["displayNameSingular"]["Default"].ToString());

                // DisplayNameSingularAllCaps
                currency["displayNameSingularAllCaps"] = Program.provider.GetLocalizedString(currency["displayNameSingularAllCaps"]["TableId"].ToString(), currency["displayNameSingularAllCaps"]["Key"].ToString(), currency["displayNameSingularAllCaps"]["Default"].ToString());
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/currencies/{0}.json", locale));
        }
    }
}

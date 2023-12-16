using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class SeasonsParser
    {
        public SeasonsParser() { }

        JsonArray array = new JsonArray();

        public void getSeasonsContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Seasons/") && file.Path.EndsWith("_DataAsset.uasset") && file.Path.Split("/").Length == 4)
                {
                    JsonObject json = new JsonObject();

                    // Package: PrimaryAsset
                    JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                    JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];

                    // Uuid
                    string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                    json.Add("uuid", uuid);

                    // Type
                    if (PrimaryAssetProperties["Type"] != null)
                    {
                        json.Add("type", PrimaryAssetProperties["Type"].ToString());
                    }
                    else
                    {
                        json.Add("type", null);
                    }

                    // StartTime
                    json.Add("startTime", PrimaryAssetProperties["StartTime"]["Ticks"].ToJsonString());

                    // EndTime
                    json.Add("endTime", PrimaryAssetProperties["EndTime"]["Ticks"].ToJsonString());

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

                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/seasons/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, season =>
            {
                // DisplayName
                season["displayName"] = Program.provider.GetLocalizedString(season["displayName"]["TableId"].ToString(), season["displayName"]["Key"].ToString(), season["displayName"]["Default"].ToString());
            });
            UassetUtil.exportJson(LocalizedArray, string.Format("data/seasons/{0}.json", locale));
        }
    }
}

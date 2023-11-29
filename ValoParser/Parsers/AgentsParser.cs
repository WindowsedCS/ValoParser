using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class AgentsParser
    {
        public AgentsParser() { }

        JsonArray array = new JsonArray();

        public void getAgentsContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Characters") && (file.Path.EndsWith("_PrimaryAsset.uasset") || file.Path.EndsWith("_BasePrimaryAsset.uasset")))
                {
                    if (file.Path.Split("/").Length == 5 && (file.Path.Split("/")[4] == string.Format("{0}_PrimaryAsset.uasset", file.Path.Split("/")[3]) || file.Path.Split("/")[4] == string.Format("{0}_BasePrimaryAsset.uasset", file.Path.Split("/")[3])))
                    {
                        JsonObject json = new JsonObject();

                        // Package: PrimaryAsset
                        JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                        JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];

                        // Uuid
                        string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                        json.Add("uuid", uuid);

                        // DeveloperName
                        json.Add("developerName", PrimaryAssetProperties["DeveloperName"].ToString());

                        // bIsPlayableCharacter
                        json.Add("isPlayableCharacter", (bool)PrimaryAssetProperties["bIsPlayableCharacter"]);

                        // bAvailableForTest
                        if (PrimaryAssetProperties["bAvailableForTest"] != null) 
                        {
                            json.Add("availableForTest", (bool)PrimaryAssetProperties["bAvailableForTest"]);
                        }

                        // FullPortrait
                        UassetUtil.exportImage(PrimaryAssetProperties["FullPortrait"]["AssetPathName"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/agents/{0}/fullportrait.png", uuid));
                        json.Add("fullPortrait", string.Format("https://assets.vallianty.com/agents/{0}/fullportrait.png", uuid));

                        // CharacterBackground
                        UassetUtil.exportImage(PrimaryAssetProperties["CharacterBackground"]["AssetPathName"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/agents/{0}/characterbackground.png", uuid));
                        json.Add("characterBackground", string.Format("https://assets.vallianty.com/agents/{0}/characterbackground.png", uuid));

                        // Package: UIData
                        JsonNode UIData = UassetUtil.loadFullJson(PrimaryAssetProperties["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                        JsonNode UIDataProperties = UIData[1]["Properties"];
                        JsonNode Strings = UassetUtil.loadJson(UIData[1]["Properties"]["Description"]["TableId"].ToString());

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

                        // Description
                        JsonObject Description = new JsonObject
                        {
                            { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                            { "Key", UIDataProperties["Description"]["Key"].ToString() },
                            { "Default", Strings["StringTable"]["KeysToMetaData"][UIDataProperties["Description"]["Key"].ToString()].ToString() }
                        };
                        json.Add("description", Description);

                        // DisplayIcon
                        UassetUtil.exportImage(UIDataProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/agents/{0}/displayicon.png", uuid));
                        json.Add("displayIcon", string.Format("https://assets.vallianty.com/agents/{0}/displayicon.png", uuid));

                        // KillfeedPortrait
                        UassetUtil.exportImage(UIDataProperties["KillfeedPortrait"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/agents/{0}/killfeedportrait.png", uuid));
                        json.Add("killfeedPortrait", string.Format("https://assets.vallianty.com/agents/{0}/killfeedportrait.png", uuid));

                        // TopHUDPortrait
                        if (UIDataProperties["TopHUDPortrait"] != null)
                        {
                            UassetUtil.exportImage(UIDataProperties["TopHUDPortrait"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/agents/{0}/tophudportrait.png", uuid));
                            json.Add("topHUDPortrait", string.Format("https://assets.vallianty.com/agents/{0}/tophudportrait.png", uuid));
                        }

                        // BackgroundGradientColors
                        JsonArray BackgroundGradientColors = new JsonArray
                        {
                            { UIDataProperties["BackgroundGradientColor1"]["Hex"].ToString() },
                            { UIDataProperties["BackgroundGradientColor2"]["Hex"].ToString() },
                            { UIDataProperties["BackgroundGradientColor3"]["Hex"].ToString() },
                            { UIDataProperties["BackgroundGradientColor4"]["Hex"].ToString() }
                        };
                        json.Add("backgroundGradientColors", BackgroundGradientColors);

                        // Abilities
                        JsonArray Abilities = new JsonArray();
                        for (int i = 0; i < UIDataProperties["Abilities"].AsArray().Count; i++)
                        {
                            string slot = "";
                            if (UIDataProperties["Abilities"].AsArray()[i]["ECharacterAbilitySlot::Ability1"] != null)
                            {
                                slot = "ECharacterAbilitySlot::Ability1";
                            } 
                            else if (UIDataProperties["Abilities"].AsArray()[i]["ECharacterAbilitySlot::Ability2"] != null)
                            {
                                slot = "ECharacterAbilitySlot::Ability2";
                            }
                            else if (UIDataProperties["Abilities"].AsArray()[i]["ECharacterAbilitySlot::Grenade"] != null)
                            {
                                slot = "ECharacterAbilitySlot::Grenade";
                            }
                            else if (UIDataProperties["Abilities"].AsArray()[i]["ECharacterAbilitySlot::Ultimate"] != null)
                            {
                                slot = "ECharacterAbilitySlot::Ultimate";
                            }
                            else
                            {
                                slot = "ECharacterAbilitySlot::Passive";
                            }

                            // Ability
                            JsonObject Ability = new JsonObject();
                            if (UIDataProperties["Abilities"].AsArray()[i][slot] == null) return;
                            JsonNode AbilityProperties = UIData[int.Parse(UIDataProperties["Abilities"].AsArray()[i][slot]["ObjectPath"].ToString().Split(".")[1])]["Properties"];

                            // DisplayName
                            JsonObject AbilityDisplayName = new JsonObject
                            {
                                { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                                { "Key", AbilityProperties["DisplayName"]["Key"].ToString() },
                                { "Default", Strings["StringTable"]["KeysToMetaData"][AbilityProperties["DisplayName"]["Key"].ToString()].ToString() }
                            };
                            Ability.Add("displayName", AbilityDisplayName);

                            // Description
                            JsonObject AbilityDescription = new JsonObject
                            {
                                { "TableId", Strings["StringTable"]["TableNamespace"].ToString() },
                                { "Key", AbilityProperties["Description"]["Key"].ToString() },
                                { "Default", Strings["StringTable"]["KeysToMetaData"][AbilityProperties["Description"]["Key"].ToString()].ToString() }
                            };
                            Ability.Add("description", AbilityDescription);

                            // DisplayIcon
                            if (AbilityProperties["DisplayIcon"] != null)
                            {
                                string iconPath = AbilityProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0];
                                if (iconPath.EndsWith("TX_BountyHunter_Seize"))
                                {
                                    iconPath = AbilityProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0] + ".tx_ability_bountyhunter_seize";
                                }
                                UassetUtil.exportImage(iconPath, string.Format(Program.exportRoot + "/assets/agents/{0}/{1}.png", uuid, slot.Replace("ECharacterAbilitySlot::", "").ToLower()));
                                Ability.Add("displayIcon", string.Format("https://assets.vallianty.com/agents/{0}/{1}.png", uuid, slot.Replace("ECharacterAbilitySlot::", "").ToLower()));
                            }

                            Abilities.Add(Ability);
                        }
                        json.Add("abilities", Abilities);
                        json.Add("assetPath", file.Path);

                        array.Add(json);
                    }
                }
            });
            UassetUtil.exportJson(array, string.Format("data/agents/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, agent =>
            {
                // DisplayName
                agent["displayName"] = Program.provider.GetLocalizedString(agent["displayName"]["TableId"].ToString(), agent["displayName"]["Key"].ToString(), agent["displayName"]["Default"].ToString());
                // DisplayNameAllCaps
                agent["displayNameAllCaps"] = Program.provider.GetLocalizedString(agent["displayNameAllCaps"]["TableId"].ToString(), agent["displayNameAllCaps"]["Key"].ToString(), agent["displayNameAllCaps"]["Default"].ToString());
                // Description
                agent["description"] = Program.provider.GetLocalizedString(agent["description"]["TableId"].ToString(), agent["description"]["Key"].ToString(), agent["description"]["Default"].ToString());

                // Abilities
                Parallel.ForEach(agent["abilities"].AsArray(), ability =>
                {
                    // DisplayName
                    ability["displayName"] = Program.provider.GetLocalizedString(ability["displayName"]["TableId"].ToString(), ability["displayName"]["Key"].ToString(), ability["displayName"]["Default"].ToString());
                    // Description
                    ability["description"] = Program.provider.GetLocalizedString(ability["description"]["TableId"].ToString(), ability["description"]["Key"].ToString(), ability["description"]["Default"].ToString());
                });
            });

            UassetUtil.exportJson(LocalizedArray, string.Format("data/agents/{0}.json", locale));
        }
    }
}

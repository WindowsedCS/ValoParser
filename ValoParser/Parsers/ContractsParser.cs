using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class ContractsParser
    {
        public ContractsParser() { }

        JsonArray array = new JsonArray();

        public void getContractsContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Contracts/") && file.Path.EndsWith("_DataAssetV2.uasset"))
                {
                    JsonObject json = new JsonObject();

                    // Package: PrimaryAsset
                    JsonNode PrimaryAsset = UassetUtil.loadFullJson(file.Path);
                    JsonNode PrimaryAssetProperties = PrimaryAsset[1]["Properties"];
                    JsonNode PrimaryAsset2Properties = PrimaryAsset[2]["Properties"];

                    // Uuid
                    string uuid = StringUtil.uuidConvert(PrimaryAssetProperties["Uuid"].ToString());
                    json.Add("uuid", uuid);

                    // bShipIt
                    if (PrimaryAssetProperties["bShipIt"] != null)
                    {
                        json.Add("shipIt", (bool)PrimaryAssetProperties["bShipIt"]);
                    }

                    // Uuid
                    string FreeRewardScheduleID = StringUtil.uuidConvert(PrimaryAssetProperties["FreeRewardScheduleID"].ToString());
                    json.Add("freeRewardScheduleID", FreeRewardScheduleID);

                    // Package: UIData
                    JsonNode UIData = UassetUtil.loadFullJson(PrimaryAssetProperties["UIData"]["AssetPathName"].ToString().Split(".")[0]);
                    JsonNode UIDataProperties = UIData[1]["Properties"];
                    JsonNode Strings;

                    // DisplayName
                    if (UIDataProperties["DisplayName"] != null)
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
                    if (UIDataProperties["DisplayNameAllCaps"] != null)
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

                    // DisplayIcon
                    if (UIDataProperties["DisplayIcon"] != null)
                    {
                        UassetUtil.exportImage(UIDataProperties["DisplayIcon"]["ObjectPath"].ToString().Split(".")[0], string.Format(Program.exportRoot + "/assets/contracts/{0}/displayicon.png", uuid));
                        json.Add("displayIcon", string.Format("https://assets.vallianty.com/contracts/{0}/displayicon.png", uuid));
                    } else
                    {
                        json.Add("displayIcon", null);
                    }

                    JsonObject content = new JsonObject();

                    // RelationType: Season
                    if (PrimaryAsset2Properties["Season"] != null)
                    {
                        JsonNode Season = UassetUtil.loadFullJson(PrimaryAsset2Properties["Season"]["AssetPathName"].ToString().Split(".")[0]);
                        content.Add("relationType", "Season");
                        content.Add("relationUuid", StringUtil.uuidConvert(Season[1]["Properties"]["Uuid"].ToString()));
                    }
                    // RelationType: Agent
                    if (PrimaryAsset2Properties["RelatedCharacter"] != null)
                    {
                        JsonNode RelatedCharacter = UassetUtil.loadFullJson(PrimaryAsset2Properties["RelatedCharacter"]["AssetPathName"].ToString().Split(".")[0]);
                        content.Add("relationType", "Agent");
                        content.Add("relationUuid", StringUtil.uuidConvert(RelatedCharacter[1]["Properties"]["Uuid"].ToString()));
                    }
                    // RelationType: Event
                    if (PrimaryAsset2Properties["Event"] != null)
                    {
                        JsonNode Event = UassetUtil.loadFullJson(PrimaryAsset2Properties["Event"]["AssetPathName"].ToString().Split(".")[0]);
                        content.Add("relationType", "Event");
                        content.Add("relationUuid", StringUtil.uuidConvert(Event[1]["Properties"]["Uuid"].ToString()));
                    }
                    if (content["relationType"] == null)
                    {
                        content.Add("relationType", "NPE");
                        content.Add("relationUuid", null);
                    }

                    // PremiumRewardScheduleID
                    if (PrimaryAsset2Properties["PremiumRewardScheduleID"] != null)
                    {
                        content.Add("premiumRewardScheduleID", PrimaryAsset2Properties["PremiumRewardScheduleID"].ToString());
                    } else
                    {
                        content.Add("premiumRewardScheduleID", null);
                    }

                    // PremiumVPCost
                    if (PrimaryAsset2Properties["PremiumVPCost"] != null)
                    {
                        content.Add("premiumVPCost", int.Parse(PrimaryAsset2Properties["PremiumVPCost"].ToString()));
                    } else
                    {
                        content.Add("premiumVPCost", 0);
                    }

                    // Chapters
                    JsonArray Chapters = new JsonArray();
                    for (int i = 0; i < PrimaryAsset2Properties["Chapters"].AsArray().Count; i++)
                    {
                        JsonObject LevelsData = new JsonObject();

                        // bIsEpilogue
                        if (PrimaryAsset2Properties["Chapters"][i]["bIsEpilogue"] != null)
                        {
                            LevelsData.Add("isEpilogue", (bool)PrimaryAsset2Properties["Chapters"][i]["bIsEpilogue"]);
                        }

                        // Levels
                        JsonArray Levels = new JsonArray();
                        for (int j = 0; j < PrimaryAsset2Properties["Chapters"][i]["Levels"].AsArray().Count; j++)
                        {
                            JsonObject Level = new JsonObject();

                            // XP
                            if (PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["XP"] != null)
                            {
                                Level.Add("xp", int.Parse(PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["XP"].ToString()));
                            }

                            // VPCost
                            if (PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["VPCost"] != null)
                            {
                                Level.Add("vpCost", int.Parse(PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["VPCost"].ToString()));
                            }

                            // bPurchasableWithVP
                            if (PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["bPurchasableWithVP"] != null)
                            {
                                Level.Add("purchasableWithVP", (bool)PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["bPurchasableWithVP"]);
                            }

                            // DoughCost
                            if (PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["DoughCost"] != null)
                            {
                                Level.Add("doughCost", int.Parse(PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["DoughCost"].ToString()));
                            }

                            // bPurchasableWithDough
                            if (PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["bPurchasableWithDough"] != null)
                            {
                                Level.Add("purchasableWithDough", (bool)PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["bPurchasableWithDough"]);
                            }

                            // Package: Reward
                            JsonObject Reward = new JsonObject();
                            JsonObject RewardProperties = PrimaryAsset[int.Parse(PrimaryAsset2Properties["Chapters"][i]["Levels"][j]["Reward"]["ObjectPath"].ToString().Split(".")[1])].AsObject();
                            
                            foreach (var item in RewardProperties["Properties"].AsObject())
                            {
                                if (item.Key != "bHighlighted" && item.Key != "Amount")
                                {
                                    // Type
                                    Reward.Add("type", item.Key);

                                    // Uuid
                                    JsonNode AssetPath = UassetUtil.loadFullJson(item.Value["AssetPathName"].ToString().Split(".")[0]);
                                    Reward.Add("uuid", StringUtil.uuidConvert(AssetPath[1]["Properties"]["Uuid"].ToString()));
                                }
                            }

                            // Amount
                            if (RewardProperties["Properties"]["Amount"] != null)
                            {
                                Reward.Add("amount", int.Parse(RewardProperties["Properties"]["Amount"].ToString()));
                            } else
                            {
                                Reward.Add("amount", 1);
                            }

                            // bHighlighted
                            if (RewardProperties["Properties"]["bHighlighted"] != null)
                            {
                                Reward.Add("highlighted", (bool)RewardProperties["Properties"]["bHighlighted"]);
                            } else
                            {
                                Reward.Add("highlighted", false);
                            }

                            Level.Add("reward", Reward);

                            Levels.Add(Level);
                        }
                        LevelsData.Add("levels", Levels);

                        // FreeChapterRewards
                        if (PrimaryAsset2Properties["Chapters"][i]["FreeChapterRewards"] != null)
                        {
                            JsonArray FreeRewards = new JsonArray();
                            for (int j = 0; j < PrimaryAsset2Properties["Chapters"][i]["FreeChapterRewards"].AsArray().Count; j++)
                            {
                                JsonArray Level = new JsonArray();


                                // Package: Reward
                                JsonObject Reward = new JsonObject();
                                JsonObject RewardProperties = PrimaryAsset[int.Parse(PrimaryAsset2Properties["Chapters"][i]["FreeChapterRewards"][j]["ObjectPath"].ToString().Split(".")[1])].AsObject();

                                foreach (var item in RewardProperties["Properties"].AsObject())
                                {
                                    if (item.Key != "bHighlighted" && item.Key != "Amount")
                                    {
                                        // Type
                                        Reward.Add("type", item.Key);

                                        // Uuid
                                        JsonNode AssetPath = UassetUtil.loadFullJson(item.Value["AssetPathName"].ToString().Split(".")[0]);
                                        Reward.Add("uuid", StringUtil.uuidConvert(AssetPath[1]["Properties"]["Uuid"].ToString()));
                                    }
                                }

                                // Amount
                                if (RewardProperties["Properties"]["Amount"] != null)
                                {
                                    Reward.Add("amount", int.Parse(RewardProperties["Properties"]["Amount"].ToString()));
                                }
                                else
                                {
                                    Reward.Add("amount", 1);
                                }

                                // bHighlighted
                                if (RewardProperties["Properties"]["bHighlighted"] != null)
                                {
                                    Reward.Add("highlighted", (bool)RewardProperties["Properties"]["bHighlighted"]);
                                }
                                else
                                {
                                    Reward.Add("highlighted", false);
                                }

                                Level.Add(Reward);

                                FreeRewards.Add(Level);
                            }
                            if (FreeRewards.Count > 0)
                            {
                                LevelsData.Add("freeChapterRewards", FreeRewards);
                            }
                        }

                        Chapters.Add(LevelsData);
                    }
                    content.Add("chapters", Chapters);

                    json.Add("content", content);
                    json.Add("assetPath", file.Path);

                    array.Add(json);
                }
            });
            UassetUtil.exportJson(array, string.Format("data/contracts/{0}.json", "raw"));
        }

        public void Localization(string locale)
        {
            JsonArray LocalizedArray = JsonNode.Parse(array.ToJsonString()).AsArray();
            Parallel.ForEach(LocalizedArray, contract =>
            {
                // DisplayName
                contract["displayName"] = Program.provider.GetLocalizedString(contract["displayName"]["TableId"].ToString(), contract["displayName"]["Key"].ToString(), contract["displayName"]["Default"].ToString());
                // DisplayNameAllCaps
                contract["displayNameAllCaps"] = Program.provider.GetLocalizedString(contract["displayNameAllCaps"]["TableId"].ToString(), contract["displayNameAllCaps"]["Key"].ToString(), contract["displayNameAllCaps"]["Default"].ToString());
            });

            UassetUtil.exportJson(LocalizedArray, string.Format("data/contracts/{0}.json", locale));
        }
    }
}

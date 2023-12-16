using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class LocresParser
    {
        public LocresParser() { }

        public List<ELanguage> AvailableLocres = new List<ELanguage>();

        public void getLocresContent()
        {
            foreach (var file in Program.provider.Files.Values.ToList())
            {
                if (file.Path.StartsWith("ShooterGame/Content/Localization/Game/") && file.Path.EndsWith(".locres"))
                {
                    string locale = file.Path.Split("/")[4];

                    Program.languageCodes.ForEach(code =>
                    {
                        if (Program.provider.GetLanguageCode(code) == locale)
                        {
                            Program.provider.LoadLocalization(code);
                            JsonNode json = JsonNode.Parse(JsonConvert.SerializeObject(Program.provider.LocalizedResources).ToString());
                            UassetUtil.exportJson(json, string.Format("data/locres/{0}.json", locale));
                            AvailableLocres.Add(code);
                        }
                    });
                }
            }
        }
    }
}

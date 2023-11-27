using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ValoParser.Utils;

namespace ValoParser.Parsers
{
    public class LocresParser
    {
        public LocresParser() { }

        public JsonArray AvailableLocres = new JsonArray();

        public void getLocresContent()
        {
            Parallel.ForEach(Program.provider.Files.Values, file =>
            {
                if (file.Path.StartsWith("ShooterGame/Content/Localization/Game") && file.Path.EndsWith(".locres"))
                {
                    string locale = file.Path.Split("/")[4];

                    Program.languageCodes.ForEach(code =>
                    {
                        if (Program.provider.GetLanguageCode(code) == locale)
                        {
                            Program.provider.LoadLocalization(code);
                            JsonNode json = JsonNode.Parse(JsonConvert.SerializeObject(Program.provider.LocalizedResources).ToString());
                            UassetUtil.exportJson(json, string.Format("data/locres/{0}.json", locale));
                            AvailableLocres.Add(locale);
                        }
                    });
                }
            });
        }
    }
}

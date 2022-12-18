namespace ValoParser.Parsers
{
    public static class UuidParser
    {
        public static string Parse(string uuid)
        {
            uuid = uuid.ToLower();
            uuid = uuid.Replace(@"-", "");
            uuid = uuid.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
            return uuid;
        }
    }
}
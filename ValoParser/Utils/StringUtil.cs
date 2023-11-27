namespace ValoParser.Utils
{
    internal class StringUtil
    {
        public static string uuidConvert(string uuid)
        {
            uuid = uuid.ToLower();
            uuid = uuid.Replace(@"-", "");
            uuid = uuid.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
            return uuid;
        }
    }
}

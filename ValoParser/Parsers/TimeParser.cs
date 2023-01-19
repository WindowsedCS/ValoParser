using System;

namespace ValoParser.Parsers
{
    public static class TimeParser
    {
        public static string Parse(string tick)
        {
            var time = new DateTime(long.Parse(tick)).GetDateTimeFormats('s')[0].ToString() + ".000Z";
            return time;
        }

        public static string Parse(long tick)
        {
            var time = new DateTime(tick).GetDateTimeFormats('s')[0].ToString() + ".000Z";
            return time;
        }
    }
}
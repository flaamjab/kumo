using System;

namespace Kumo
{
    static class Schema
    {
        public static Uri Namespace { get; } = new Uri("https://kumo.org");

        public static string Prefix { get; } = "kumo";

        public static string RefersTo { get; } = "RefersTo";

        public static string ShortName(string predicate)
        {
            return $"{Prefix}:{predicate}";
        }

        public static string Prefixed(int id)
        {
            return Prefix + "_" + id.ToString();
        }

        public static int Unprefixed(string id)
        {
            string cleanId = id
                .Replace("_", "")
                .Replace(":", "")
                .Replace(Prefix, "");

            return int.Parse(cleanId);
        }
    }
}

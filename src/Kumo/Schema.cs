using System;

namespace Kumo
{
    abstract class Schema
    {
        public static Uri Namespace { get; } = new Uri("https://kumo.org");

        public static string Prefix { get; } = "kumo";

        public static string RefersTo { get; } = "RefersTo";

        public static string QName(string predicate)
        {
            return $"{Prefix}:{predicate}";
        }

        public static Uri Uri(string predicate)
        {
            return new Uri($"{Namespace.OriginalString}/{predicate}");
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

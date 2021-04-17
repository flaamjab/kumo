using System;

namespace Kumo
{
    abstract class Schema
    {
        public static Uri Namespace { get; } = new Uri("https://kumo.org");

        public static string Prefix { get; } = "kumo";

        public static string RefersTo { get; } = "references";

        public static string QName(string predicate)
        {
            return $"{Namespace.OriginalString}/{predicate}";
        }

        public static Uri Uri(string predicate)
        {
            return new Uri($"{Namespace.OriginalString}/{predicate}");
        }
    }
}

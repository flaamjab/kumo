using System;

namespace Kumo
{
    public record Property
    {
        public Uri Name { get; }
        public Uri Value { get; }

        public Property(Uri name, Uri value)
        {
            Name = name;
            Value = value;
        }
    }
}

namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;

    public sealed class TaggedEntry<TTag, T>: IEquatable<TaggedEntry<TTag, T>>
    {
        public TTag Tag { get; set; }
        public T Value { get; set; }

        bool IEquatable<TaggedEntry<TTag, T>>.Equals(TaggedEntry<TTag, T> other)
        {
            if (other == null)
                return false;

            return EqualityComparer<TTag>.Default.Equals(this.Tag, other.Tag);
        }
    }
}

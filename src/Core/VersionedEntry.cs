namespace LostTech.NKeyValue
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents versioned store entry, which contains value and its version
    /// </summary>
    /// <typeparam name="TVersion">Type of version information</typeparam>
    /// <typeparam name="T">Type of the value</typeparam>
    public sealed class VersionedEntry<TVersion, T>: IEquatable<VersionedEntry<TVersion, T>>
    {
        public TVersion Version { get; set; }
        public T Value { get; set; }

        bool IEquatable<VersionedEntry<TVersion, T>>.Equals(VersionedEntry<TVersion, T> other)
        {
            if (other == null)
                return false;

            return EqualityComparer<TVersion>.Default.Equals(this.Version, other.Version);
        }

        public VersionedEntry<TVersion, TNew> Select<TNew>(Func<T, TNew> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new VersionedEntry<TVersion, TNew>
            {
                Value = selector(this.Value),
                Version = this.Version,
            };
        }
    }
}

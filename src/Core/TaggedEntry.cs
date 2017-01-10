namespace LostTech.NKeyValue
{
    public sealed class TaggedEntry<TTag, T>
    {
        public TTag Tag { get; set; }
        public T Value { get; set; }
    }
}

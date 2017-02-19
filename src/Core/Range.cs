namespace LostTech.NKeyValue
{
    public struct Range<T>
    {
        public Range(T start, T end)
        {
            this.Start = start;
            this.End = end;
        }
        public T Start { get; }
        public T End { get; }
    }
}

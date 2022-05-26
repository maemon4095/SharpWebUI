interface IDomStructure
{
    public long Count { get; }
    public DomNode this[long index] { get; }
    public void Append(in DomNode node);
    public void Insert(long index, in DomNode node);
    public void Clear();
    public void Export(TextWriter writer);
}

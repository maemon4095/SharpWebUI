struct Dom
{
    public Dom(IDomStructure structure)
    {
        this.structure = structure;
        this.CurrentDepth = structure.Count <= 0 ? 0 : structure[structure.Count - 1].Depth;
    }

    readonly IDomStructure structure;

    public Dom Append(in DomNode node)
    {
        this.structure.Append(node.WithDepth(this.CurrentDepth));
        return this;
    }
    public Dom Children()
    {
        this.CurrentDepth++;
        return this;
    }
    public Dom Parent()
    {
        this.CurrentDepth--;
        return this;
    }

    public int CurrentDepth
    {
        get; private set;
    }

    public void ExportAndClear(TextWriter writer)
    {
        this.structure.Export(writer);
        this.structure.Clear();
    }
}

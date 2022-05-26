struct Dom
{
    public Dom(IDomStructure structure)
    {
        this.structure = structure;
    }

    readonly IDomStructure structure;

    public Dom Append(in DomNode node)
    {
        this.structure.Append(node.WithDepth(this.CurrentDepth));
        return this;
    }
    public Dom AppendChild(in DomNode node)
    {
        this.structure.Append(node.WithDepth(this.CurrentDepth + 1));
        return this;
    }

    public int CurrentDepth
    {
        get
        {
            var structure = this.structure;
            var count = structure.Count;
            if (count <= 0) return 0;
            return structure[count - 1].Depth;
        }
    }

    public void ExportAndClear(TextWriter writer)
    {
        this.structure.Export(writer);
        this.structure.Clear();
    }
}

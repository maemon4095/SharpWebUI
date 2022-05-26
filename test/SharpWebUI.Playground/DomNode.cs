
//<Tag> value </Tag> みたいな時どうする
readonly struct DomNode
{
    public static DomNode Create(DomNodeDefinition definition)
    {
        return new DomNode(definition, 0, DomNodeAttributeList.Empty, null);
    }

    DomNode(DomNodeDefinition definition, int depth, DomNodeAttributeList attributes, object? value)
    {
        this.Definition = definition;
        this.Attributes = attributes;
        this.Depth = depth;
        this.Value = value;
    }

    public DomNodeDefinition Definition { get; }
    public string Name => this.Definition.Name;
    public DomNodeCategoryFlags Category => this.Definition.Category;
    public DomNodeAttributeList Attributes { get; init; }
    public object? Value { get; init; }
    public int Depth { get; }

    public DomNode WithValue(object value)
    {
        return new DomNode(this.Definition, this.Depth, this.Attributes, value);
    }
    public DomNode WithAttribute(in DomNodeAttribute attribute)
    {
        return new DomNode(this.Definition, this.Depth, this.Attributes.Add(attribute), this.Value);
    }
    public DomNode WithAttribute(string name)
    {
        return this.WithAttribute(new DomNodeAttribute(name, string.Empty));
    }
    public DomNode WithAttribute(string name, string value)
    {
        return this.WithAttribute(new DomNodeAttribute(name, value));
    }
    public DomNode WithDepth(int depth)
    {
        return new DomNode(this.Definition, depth, this.Attributes, this.Value);
    }
}

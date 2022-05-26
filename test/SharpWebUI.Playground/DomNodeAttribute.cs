readonly record struct DomNodeAttribute(string Name, string Value)
{
    public bool IsSingle => string.IsNullOrEmpty(this.Value);
    public override string ToString()
    {
        if(this.IsSingle) return this.Name;
        return $"{this.Name} = {this.Value}";
    }
}
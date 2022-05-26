
//ストリームにエミットする形にする？
//単なる文字に関してはName=""、Categry = Singleとし、GetOpeningでテキストを得る。
abstract class DomNodeDefinition
{
    protected DomNodeDefinition(string name, DomNodeCategoryFlags category)
    {
        this.Name = name;
        this.Category = category;
    }
    public string Name { get; }
    public DomNodeCategoryFlags Category { get; }
    //TODO: use better writer 
    public abstract void WriteOpening(TextWriter writer, DomNode node);
    public abstract void WriteClosing(TextWriter writer, DomNode node);
}

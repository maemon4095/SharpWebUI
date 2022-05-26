
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
    public abstract string GetOpening(DomNode node);
    public abstract string GetClosing(DomNode node);
}

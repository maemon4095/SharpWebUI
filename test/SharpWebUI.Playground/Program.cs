using System.IO;
var dom = new Dom(new DomStructure());

dom.Append(HtmlTags.DOCTYPE);
dom.Append(HtmlTags.Html);
dom.AppendChild(HtmlTags.Div);
dom.AppendChild(HtmlTags.PlainText("text"));
dom.ExportAndClear(Console.Out);


class DomStructure : IDomStructure
{
    //TODO: sequence impl
    readonly List<DomNode> _nodes = new();

    public DomNode this[long index] => this._nodes[(int)index];

    public long Count => this._nodes.Count;

    public void Append(in DomNode node) => this._nodes.Add(node);
    public void Clear() => this._nodes.Clear();
    public void Insert(long index, in DomNode node) => this._nodes.Insert((int)index, node);
    public void Export(TextWriter writer)
    {
        const string tabString = "    ";
        var nodes = this._nodes;
        export(writer, nodes.GetEnumerator(), 0);
        static void writeTab(TextWriter writer, int depth)
        {
            while (depth > 0)
            {
                writer.Write(tabString);
                depth--;
            }
        }
        static void export(TextWriter writer, IEnumerator<DomNode> enumerator, int depth)
        {
            var prev = default(DomNode?);
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current.Depth < depth)
                    return;
                writer.WriteLine(current.Category.ToString() + ":");
                writeTab(writer, current.Depth);
                writer.WriteLine(current.GetOpening());
                if (current.Depth != depth)
                {
                    export(writer, enumerator, depth + 1);
                }
                writer.WriteLine("prev: " + prev?.Name ?? "");
                if (prev is DomNode p && p.Category.HasFlag(DomNodeCategoryFlags.Paired))
                {
                    writeTab(writer, p.Depth);
                    writer.WriteLine(p.GetClosing());
                }
                prev = current;
            }
        }
    }
}

struct ViewContext<T>
{
    public T State;
    public Dom Dom { get; }
}
interface IView<T>
{
    void View(ref ViewContext<T> context);
}

static class HtmlTagDefinitions
{
    class GenericTag : DomNodeDefinition
    {
        public GenericTag(string name, DomNodeCategoryFlags category)
            : base(name, category)
        {

        }

        public override string GetOpening(DomNode node)
        {
            if (this.Name == "plaintext")
            {
                return node.Value?.ToString() ?? "";
            }
            else
            {

                return $"<{this.Name}{(node.Attributes.Any() ? " " : "")}{string.Join(" ", node.Attributes)}>";
            }
        }

        public override string GetClosing(DomNode node)
        {
            return $"<{this.Name}>";
        }
    }

    public static DomNodeDefinition DOCTYPE { get; } = new GenericTag("!DOCTYPE", DomNodeCategoryFlags.Single);
    public static DomNodeDefinition PlainText { get; } = new GenericTag("plaintext", DomNodeCategoryFlags.Single);
    public static DomNodeDefinition Html { get; } = new GenericTag("html", DomNodeCategoryFlags.Paired);
    public static DomNodeDefinition Div { get; } = new GenericTag("div", DomNodeCategoryFlags.Paired);
}

static class HtmlTags
{
    public static DomNode DOCTYPE => DomNode.Create(HtmlTagDefinitions.DOCTYPE).WithAttribute("html");
    public static DomNode PlainText(string str) => DomNode.Create(HtmlTagDefinitions.PlainText).WithValue(str);
    public static DomNode Html => DomNode.Create(HtmlTagDefinitions.Html);
    public static DomNode Div => DomNode.Create(HtmlTagDefinitions.Div);
}

struct Model
{

}

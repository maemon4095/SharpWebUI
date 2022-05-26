using System.IO;
var dom = new Dom(new DomStructure());

dom.Append(HtmlTags.DOCTYPE);
dom.Append(HtmlTags.Html);
dom.AppendChild(HtmlTags.Div);
dom.AppendChild(HtmlTags.PlainText("text"));
dom.Append(HtmlTags.Div);
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
        var enumerator = nodes.GetEnumerator();
        if (!enumerator.MoveNext()) return;
        open(writer, enumerator.Current);
        export(writer, enumerator);

        static void writeTab(TextWriter writer, int depth)
        {
            while (depth > 0)
            {
                writer.Write(tabString);
                depth--;
            }
        }
        static void open(TextWriter writer, in DomNode node)
        {
            writer.Write("o: ");
            writeTab(writer, node.Depth);
            node.Definition.WriteOpening(writer, node);
        }
        static void close(TextWriter writer, in DomNode node)
        {
            writer.Write("c: ");
            if (!node.Category.HasFlag(DomNodeCategoryFlags.Paired))
            {
                writer.WriteLine($"[{node.Name}]");
                return;
            }
            writeTab(writer, node.Depth);
            node.Definition.WriteClosing(writer, node);
        }
        //depth毎に再帰
        static void export(TextWriter writer, IEnumerator<DomNode> enumerator)
        {
            var prev = enumerator.Current;
            var depth = prev.Depth;
            while (enumerator.MoveNext())//シーケンスの最後の時closeが呼ばれない。
            {
                var current = enumerator.Current;
                if (current.Depth < depth)
                    return;
                if (current.Depth == depth)
                {
                    close(writer, prev);
                }
                open(writer, current);
                if (current.Depth != depth)
                {
                    export(writer, enumerator);
                    close(writer, prev);
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

        public override void WriteOpening(TextWriter writer, DomNode node)
        {
            if (this.Name == "plaintext")
            {
                writer.WriteLine(node.Value);
            }
            else
            {
                writer.Write('<');
                writer.Write(this.Name);
                foreach (var attr in node.Attributes)
                {
                    writer.Write(' ');
                    writer.Write(attr.ToString());
                }
                writer.WriteLine('>');
            }
        }

        public override void WriteClosing(TextWriter writer, DomNode node)
        {
            writer.Write('<');
            writer.Write('/');
            writer.Write(this.Name);
            writer.WriteLine('>');
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

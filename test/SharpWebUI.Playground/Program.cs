using System.Buffers;
using System.IO;


Console.ReadLine();

abstract class DomNodeHandler<T>
{
    public abstract DomNodeCategoryFlags GetCategory(T node);
    public abstract void WriteOpening(ref TextBufferWriter writer, T node);
    public abstract void WriteClosing(ref TextBufferWriter writer, T node);
}

interface IDomStructure<T> : IEnumerable<(T Node, int Depth)>
{
    DomNodeHandler<T> Handler { get; }

    public long Count { get; }
    public T this[long index] { get; }
    public void Append(T node);
    public void Insert(long index, T node);
    public void Clear();
    public void Export(ref TextBufferWriter writer);
}



//T: DomNode, DomNodeHandler<T>みたいなAPIにする？ 
//現状はDomNodeがunmanagedじゃないし，htmlによりすぎ. texは属性とかない．
//depthとidだけ持って，データはハンドラないで扱ってもらったほうがよさそう． ECS的な．
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
        var enumerator = this._nodes.GetEnumerator();
        enumerator.MoveNext();
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
            writeTab(writer, node.Depth);
            node.Definition.WriteOpening(writer, node);
        }
        static void close(TextWriter writer, in DomNode node)
        {
            if (!node.Category.HasFlag(DomNodeCategoryFlags.Paired)) return;

            writeTab(writer, node.Depth);
            node.Definition.WriteClosing(writer, node);
        }
        //depth毎に再帰
        static bool export(TextWriter writer, IEnumerator<DomNode> enumerator)
        {
            var current = enumerator.Current;
            var depth = current.Depth;
            do
            {
                open(writer, current);
                var terminal = !enumerator.MoveNext();
                if (terminal) goto EXIT;
                var next = enumerator.Current;
                if (next.Depth < depth) goto EXIT;
                if (next.Depth != depth)
                {
                    terminal = export(writer, enumerator);
                    if (terminal) goto EXIT;
                    next = enumerator.Current;
                }
                close(writer, current);
                current = next;
                continue;

                EXIT:
                close(writer, current);
                return terminal;
            }
            while (true);
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

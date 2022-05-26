using System.Collections;
using System.Collections.Immutable;
using System.IO;

var dom = new Dom(new DomStructure());
dom.Append(HtmlTags.DOCTYPE);
dom.Append(HtmlTags.Html);
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
        var index = 0;
        var nodes = this._nodes;
        export(writer, nodes, 0, ref index);

        static void export(TextWriter writer, List<DomNode> nodes, int depth, ref int index)
        {
            while (index < nodes.Count)
            {
                var current = nodes[index];
                if (current.Depth < depth)
                    return;
                writer.WriteLine(current.GetOpening());
                if (current.Depth == depth)
                {
                    index++;
                }
                else
                {
                    export(writer, nodes, depth + 1, ref index);
                }
                if (current.Category.HasFlag(DomNodeCategoryFlags.Paired))
                {
                    writer.WriteLine(current.GetClosing());
                }
            }
        }
    }
}

interface IDomStructure
{
    public long Count { get; }
    public DomNode this[long index] { get; }
    public void Append(in DomNode node);
    public void Insert(long index, in DomNode node);
    public void Clear();
    public void Export(TextWriter writer);
}

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

[Flags]
enum DomNodeCategoryFlags
{
    Single = 0,
    Paired = 1,
}
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

readonly record struct DomNodeAttribute(string Name, string Value)
{
}

readonly struct DomNodeAttributeList : IEnumerable<DomNodeAttribute>
{
    public struct Enumerator : IEnumerator<DomNodeAttribute>
    {
        public Enumerator(in DomNodeAttributeList list)
        {
            this._enumerator = list._attributes.GetEnumerator();
        }

        ImmutableArray<DomNodeAttribute>.Enumerator _enumerator;
        public DomNodeAttribute Current => this._enumerator.Current;
        object IEnumerator.Current => this.Current;
        public bool MoveNext() => this._enumerator.MoveNext();
        public void Dispose() { }
        public void Reset() => throw new NotSupportedException();
    }

    public static DomNodeAttributeList Empty => default;

    private DomNodeAttributeList(ImmutableArray<DomNodeAttribute> attributes)
    {
        this._attributes = attributes;
    }

    readonly ImmutableArray<DomNodeAttribute> _attributes;//TODO: use array pool

    public DomNodeAttributeList Add(in DomNodeAttribute attribute)
    {
        var inserted = this._attributes.Add(attribute);
        return new DomNodeAttributeList(inserted);
    }
    public DomNodeAttributeList Add(string name)
    {
        return this.Add(new DomNodeAttribute(name, string.Empty));
    }
    public DomNodeAttributeList Add(string name, string value)
    {
        return this.Add(new DomNodeAttribute(name, value));
    }


    IEnumerator<DomNodeAttribute> IEnumerable<DomNodeAttribute>.GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

//<Tag> value </Tag> みたいな時どうする
readonly record struct DomNode
{
    public static DomNode Create(DomNodeDefinition definition)
    {
        return new DomNode(definition, 0);
    }

    DomNode(DomNodeDefinition definition, int depth)
    {
        this.Definition = definition;
        this.Attributes = DomNodeAttributeList.Empty;
        this.Depth = depth;
        this.Value = null;
    }

    public DomNodeDefinition Definition { get; }
    public string Name => this.Definition.Name;
    public DomNodeCategoryFlags Category => this.Definition.Category;
    public DomNodeAttributeList Attributes { get; init; }
    public object? Value { get; init; }
    public int Depth { get; }

    public string GetOpening()
    {
        return this.Definition.GetOpening(this);
    }
    public string GetClosing()
    {
        return this.Definition.GetClosing(this);
    }

    public DomNode WithValue(object value)
    {
        return this with
        {
            Value = value
        };
    }
    public DomNode WithAttribute(in DomNodeAttribute attribute)
    {
        return this with
        {
            Attributes = this.Attributes.Add(attribute)
        };
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
        return new DomNode(this.Definition, depth);
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
                return $"<{this.Name} {string.Join(" ", node.Attributes.Select(pair => $"{pair.Name} = {pair.Value}"))}>";
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
}

static class HtmlTags
{
    public static DomNode DOCTYPE => DomNode.Create(HtmlTagDefinitions.DOCTYPE).WithAttribute("html");
    public static DomNode PlainText(string str) => DomNode.Create(HtmlTagDefinitions.PlainText).WithValue(str);
    public static DomNode Html => DomNode.Create(HtmlTagDefinitions.Html);
}

struct Model
{

}

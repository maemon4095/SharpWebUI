// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Collections.Immutable;

Console.WriteLine("Hello, World!");


class DomStructure : IDomStructure
{
    //TODO: sequence impl
    readonly List<DomNode> _nodes = new();

    public DomNode this[int index] => this._nodes[index];

    public int Count => this._nodes.Count;

    public void Append(in DomNode node) => this._nodes.Add(node);
    public void Clear() => this._nodes.Clear();
    public void Insert(int index, in DomNode node) => this._nodes.Insert(index, node);
    public void Export(TextWriter writer)
    {

    }
}

interface IDomStructure
{
    public int Count { get; }
    public DomNode this[int index] { get; }
    public void Append(in DomNode node);
    public void Insert(int index, in DomNode node);
    public void Clear();
    public void Export(TextWriter writer);
}

struct Dom
{
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
//texとかhtmlに対応したい．
abstract class DomNodeDefinition
{
    public abstract string Name { get; }
    public abstract DomNodeCategoryFlags Category { get; }
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

readonly record struct DomNode
{
    public static DomNode Create(DomNodeDefinition definition)
    {
        return new DomNode(definition, 0, DomNodeAttributeList.Empty);
    }

    DomNode(DomNodeDefinition definition, int depth, DomNodeAttributeList attributes)
    {
        this.Definition = definition;
        this.Attributes = attributes;
        this.Depth = depth;
    }

    public DomNodeDefinition Definition { get; }
    public string Name => this.Definition.Name;
    public DomNodeCategoryFlags Category => this.Definition.Category;
    public DomNodeAttributeList Attributes { get; init; }
    public int Depth { get; }

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
        return new DomNode(this.Definition, depth, this.Attributes);
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
        {
            this._name = name;
            this._category = category;
        }

        private readonly string _name;
        private readonly DomNodeCategoryFlags _category;

        public override string Name => this._name;
        public override DomNodeCategoryFlags Category => this._category;
    }

    public static DomNodeDefinition DOCTYPE { get; } = new GenericTag("!DOCTYPE", DomNodeCategoryFlags.Single);
}

static class HtmlTags
{
    public static DomNode DOCTYPE => DomNode.Create(HtmlTagDefinitions.DOCTYPE).WithAttribute("html");
}

struct Model
{

}

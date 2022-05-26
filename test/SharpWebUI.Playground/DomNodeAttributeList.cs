using System.Collections;
using System.Collections.Immutable;

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

    public static DomNodeAttributeList Empty => new DomNodeAttributeList(ImmutableArray.Create<DomNodeAttribute>());

    private DomNodeAttributeList(ImmutableArray<DomNodeAttribute> attributes)
    {
        this._attributes = attributes;
    }

    readonly ImmutableArray<DomNodeAttribute> _attributes;//TODO: use array pool

    public DomNodeAttributeList Add(in DomNodeAttribute attribute)
    {
        return new DomNodeAttributeList(ImmutableArray.CreateRange(this._attributes.Append(attribute)));
    }
    public DomNodeAttributeList Add(string name)
    {
        return this.Add(new DomNodeAttribute(name, string.Empty));
    }
    public DomNodeAttributeList Add(string name, string value)
    {
        return this.Add(new DomNodeAttribute(name, value));
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<DomNodeAttribute> IEnumerable<DomNodeAttribute>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

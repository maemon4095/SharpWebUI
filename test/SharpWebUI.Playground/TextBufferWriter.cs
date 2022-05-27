using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

ref struct TextBufferWriter
{
    [InterpolatedStringHandler]
    public ref struct InterpolatedStringHandler
    {
#pragma warning disable IDE0060
        public InterpolatedStringHandler(int literalLength, int formattedCount, TextBufferWriter writer, IFormatProvider? provider = null)
        {
            this.writer = writer.writer;
            this.provider = provider;
        }
#pragma warning restore IDE0060

        readonly IBufferWriter<char> writer;
        readonly IFormatProvider? provider;

        public void AppendLiteral(string s)
        {
            this.writer.Write(s);
        }
        public void AppendFormatted<T>(T x, string? format = null) where T : IFormattable
        {
            Console.WriteLine("formattable");
            this.writer.Write(x.ToString(format, this.provider));
        }

        public void AppendFormatted<T>([NotNull] T x, int alignment = 0)
        {
            ArgumentNullException.ThrowIfNull(x);
            var str = x.ToString();
            if (str is null) return;
            if (alignment > 0)
            {
                var rest = alignment - str.Length;
                if (rest > 0)
                {
                    Span<char> span = stackalloc char[rest];
                    span.Fill(' ');
                    this.writer.Write(span);
                }
            }
            this.writer.Write(str);
            if (alignment < 0)
            {
                var rest = -alignment - str.Length;
                if (rest > 0)
                {
                    Span<char> span = stackalloc char[rest];
                    span.Fill(' ');
                    this.writer.Write(span);
                }
            }
        }

        public void AppendFormatted<T>(T x, int alignment = 0, string? format = null) where T : ISpanFormattable
        {
            var span = this.writer.GetSpan();

            if (x.TryFormat(span, out var writtern, format, this.provider))
            {
                this.writer.Advance(writtern);
            }
            else
            {
                this.writer.Write(x.ToString(format, this.provider));
            }
        }
    }

    public TextBufferWriter(IBufferWriter<char> writer)
    {
        this.writer = writer;
    }

    readonly IBufferWriter<char> writer;

    private void ClearState()
    {

    }
    public void Write(string str)
    {
        var buffer = this.writer.GetSpan(str.Length);
        str.CopyTo(buffer);
        this.writer.Advance(str.Length);
    }

#pragma warning disable IDE0060
    public void Write([InterpolatedStringHandlerArgument("")] InterpolatedStringHandler handler)
    {
        this.ClearState();
    }
#pragma warning restore IDE0060

}

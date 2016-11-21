using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Postman.Blocks
{
    public interface HandlesAll:
        Header.Handles, List.Handles, StartsWithRightAngleBracket.Handles,
        BlankLine.Handles, Plain.Handles, EndOfFile.Handles, Comment.Handles        
    { }

    public interface Produces<T>
    {
        Production<T> Produce();
    }

    public abstract class Production<T>            
    {
        public T Value
        {
            get
            {
                if (!Successful)
                    throw new InvalidOperationException();
                else
                    return _Value;
            }
        }

        protected abstract T _Value { get; }
        public abstract bool Successful { get; }
        public abstract string SwitchableString { get; }

        public Production<U> Select<U>(Func<T, U> f) => new None<U>(f(_Value));

        public Production<V> SelectMany<U, V>(Func<T, Production<U>> f, Func<T, U, V> g)            
        {
            var u = f(_Value);

            var v = g(_Value, u._Value);
            return (v as Produces<V>)?.Produce() ?? new None<V>(v);
        }
        sealed class None<U> : Production<U>            
        {
            readonly U x;
            public None(U x) { this.x = x; }

            protected override U _Value => x;
            public override bool Successful => false;
            public override string SwitchableString => Guid.NewGuid().ToString();
        }

        public static T operator /(Production<T> prod, T x) =>  prod.Successful ? prod._Value : x;

        public static implicit operator string (Production<T> prod) => prod.SwitchableString;
        public static implicit operator bool (Production<T> prod) =>  prod.Successful;
    }

    sealed class RegexMatchProduction : Production<MatchEx>
    {
        readonly MatchEx match;

        public RegexMatchProduction(MatchEx match)
        {
            this.match = match;
        }

        public override bool Successful => match.Successful;

        public override string SwitchableString => match.Successful ? match.PatternID : null;

        protected override MatchEx _Value => match;
    }

    public struct MatchEx : Produces<MatchEx>
    {
        public string OriginalString { get; }
        public Match Match { get; }
        public string PatternID { get; }

        public MatchEx(string original, Match m, string pattern_id)
        {
            this.OriginalString = original;
            this.Match = m;
            this.PatternID = pattern_id;
        }

        public bool Successful => Match.Success;

        public string Substring
        {
            get
            {
                return OriginalString.Substring(Match.Index + Match.Length);
            }
        }

        public static MatchEx operator /(MatchEx x, MatchEx y) => x.Match?.Success == true ? x : y;
        public static implicit operator string (MatchEx match) => match.PatternID;

        public Production<MatchEx> Produce() => new RegexMatchProduction(this);
    }

    public static class Utils
    {
        static readonly Dictionary<string, Regex> regex_memo = new Dictionary<string, Regex>();

        public static Production<MatchEx> Get(this string line, string pattern)
        {
            Regex regex;

            if (!regex_memo.TryGetValue(pattern, out regex))
                regex_memo.Add(pattern, regex = new Regex(pattern));

            return new RegexMatchProduction(new MatchEx(line, regex.Match(line), pattern));
        }
    }

    public sealed class Tokenized : IEnumerable<Token>
    {
        readonly IEnumerable<Token> tokens;

        public Tokenized(string s) : this(new MemoryStream(Encoding.Unicode.GetBytes(s))) { }

        public Tokenized(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                var stack = new TokenStack();

                if (!sr.EndOfStream)
                {
                    var start = new Token.FromLine(sr.ReadLine());

                    stack.Push(start);

                    while(!sr.EndOfStream)
                    {
                        var t = stack.Pop();

                        foreach (var result in t.Consider(sr.ReadLine()))
                            stack.Push(result);
                    }
                        
                }

                tokens = stack.Push(new EndOfFile()).Reverse();
            }
        }

        public IEnumerator<Token> GetEnumerator() => tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => tokens.GetEnumerator();

        sealed class TokenStack : Stack<Token> { public new TokenStack Push(Token t) { base.Push(t); return this; } }
    }

    public abstract class Scanned
    {
        public abstract IEnumerator<Token> GetEnumerator();
    }

    public abstract class Token : Produces<Token>
    {
        readonly string line;

        public Token(string line)
        {
            this.line = line;
        }

        public virtual int Identation => line.TakeWhile(c => char.IsWhiteSpace(c)).Select(c => c == '\t' ? 4 : 1).Sum();

        public virtual Scanned Consider(string line) => this * new FromLine(line);

        sealed class Production : Production<Token>
        {
            readonly Token obj;

            public Production(Token t) { this.obj = t; }

            public override bool Successful => obj != null;

            public override string SwitchableString => "";

            protected override Token _Value => obj;
        }

        public abstract SyntaxNode GetSyntaxNode(HandlesAll node);

        Production<Token> Produces<Token>.Produce() => new Production(this);

        public static Token operator /(Token x, Token y) => x ?? y;

        public struct FromLine
        {
            readonly Token token;

            public FromLine(string line)
            {
                var product =
                                from list in new List.Production(line)
                                from plain in new Plain.Production(line)
                                from header in new Header.Production(line)
                                from comment in new Comment.Production(line)
                                from blank in new BlankLine.Production(line)
                                from blockquote in new StartsWithRightAngleBracket.Production(line)

                                select list / header / blank / plain / blockquote / comment;

                token = product.Value;
            }

            public static implicit operator Token(FromLine line) => line.token;
        }

        public struct IntermediateScanResult
        {
            readonly Token x, y;
            readonly List<Token> xs;

            private IntermediateScanResult(Token x, Token y, List<Token> xs)
            {
                this.x = x;
                this.y = y;
                this.xs = xs;
            }

            public IntermediateScanResult(Token x, Token y) : this(x, y, null) { }
            public IntermediateScanResult(List<Token> xs) : this(null, null, xs) { }

            public static IntermediateScanResult operator *(IntermediateScanResult sr, Token t)
            {
                if (sr.xs == null)
                    return new IntermediateScanResult(new List<Token> { sr.x, sr.y, t });
                else
                {
                    sr.xs.Add(t);
                    return new IntermediateScanResult(sr.xs);
                }
            }

            public static implicit operator Scanned(IntermediateScanResult sr) => sr.xs == null ? new Double(sr.x, sr.y) as Scanned : new Many(sr.xs);

            sealed class Double : Scanned
            {
                readonly Token x, y;

                public Double(Token x, Token y)
                {
                    this.x = x;
                    this.y = y;
                }

                public override IEnumerator<Token> GetEnumerator()
                {
                    yield return x;
                    yield return y;
                }
            }

            sealed class Many : Scanned
            {
                readonly List<Token> xs;

                public Many(List<Token> xs)
                {
                    this.xs = xs;
                }

                public override IEnumerator<Token> GetEnumerator() => xs.GetEnumerator();
            }
        }

        public static IntermediateScanResult operator *(Token x, Token y) => new IntermediateScanResult(x, y);

        public static implicit operator Scanned(Token t) => new Single(t);

        sealed class Single : Scanned
        {
            readonly Token token;

            public Single(Token token) { this.token = token; }

            public override IEnumerator<Token> GetEnumerator() { yield return token; }
        }
    }
}
using System;
using System.Text.RegularExpressions;

namespace Postman.Blocks
{
    static class ListUtils
    {
        public static Production<MatchEx> GetGroup(this MatchEx m, string name)
        {
            return new RegexMatchProduction(new MatchEx(m.OriginalString, m.Match, name));
        }

        public static MatchEx MatchEx(this Regex regex, string line)
        {
            var match = regex.Match(line);

            return new MatchEx(line, match, regex.ToString());
        }
    }

    sealed class GroupEx
    {
        readonly Group g;
        readonly string name;

        public GroupEx(Group g, string name)
        {
            this.g = g;
            this.name = name;
        }

        public static implicit operator string (GroupEx g) => g.name;

        public static GroupEx operator + (GroupEx x, GroupEx y)
        {
            var z = x ?? y;

            return new GroupEx(z.g, z.name);
        }
    }

    public abstract class List : Token, Produces<List>
    {
        List(Match m, string line) : base(line) { }


        public interface Handles
        {
            SyntaxNode HandleBulleted(List token);
            SyntaxNode HandleLettered(List token);
            SyntaxNode HandleNumbered(List token);
        }

        public sealed class Production : Production<List>
        {
            public static readonly Regex list_patterns
                = new Regex(@"^\s*((?'bulleted'\*\s+)|(?'lettered'[a-z]+\.\s+)|(?'numbered'\d+\.\s+)).+[^\s]+$", RegexOptions.Compiled);

            readonly List list_like;

            protected override List _Value => list_like;

            public override bool Successful => list_like != null;

            public override string SwitchableString => "";

            public Production(List list)
            {
                this.list_like = list;
            }

            public Production(string line)
            {
                list_like = null;

                var match = list_patterns.MatchEx(line);

                var r = from bulleted in match.GetGroup("bulleted")
                        from numbered in match.GetGroup("numbered")
                        from lettered in match.GetGroup("lettered")

                        

                        select bulleted / numbered / lettered;

                switch (r)
                {
                    case "bulleted": { list_like = new Bulleted(r); break; }
                    case "numbered": { list_like = new Numbered(r); break; }
                    case "lettered": { list_like = new Lettered(r); break; }
                    default: { list_like = null; break; }
                }
            }
        }

        public enum Type
        {
            Bulleted,
            Numbered,
            Lettered
        }

        abstract class SomeList<T> : List
            where T :  SomeList<T>
        {
            public SomeList(Production<MatchEx> p) : base(p.Value.Match, p.Value.OriginalString) { }

            new sealed class Production : Production<SomeList<T>>
            {
                readonly SomeList<T> obj;

                public Production(SomeList<T> obj)
                {
                    this.obj = obj;
                }


                public override bool Successful => obj != null;

                public override string SwitchableString => "";

                protected override SomeList<T> _Value => obj;
            }
        }

        sealed class Bulleted : SomeList<Bulleted>,  Produces<Bulleted>
        {
            public Bulleted(Production<MatchEx> p) : base(p) { }

            new sealed class Production : Production<Bulleted>
            {
                readonly Bulleted obj;

                public Production(Bulleted b) { this.obj = b; }

                public override bool Successful => obj != null;

                public override string SwitchableString => "";

                protected override Bulleted _Value => obj;
            }

            public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleBulleted(this);

            Production<Bulleted> Produces<Bulleted>.Produce() => new Production(this);
        }

        sealed class Numbered : SomeList<Numbered>, Produces<Numbered>
        {
            public Numbered(Production<MatchEx> p) : base(p) { }

            new sealed class Production : Production<Numbered>
            {
                readonly Numbered obj;

                public Production(Numbered b) { this.obj = b; }

                public override bool Successful => obj != null;

                public override string SwitchableString => "";

                protected override Numbered _Value => obj;
            }

            public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleNumbered(this);

            Production<Numbered> Produces<Numbered>.Produce() => new Production(this);
        }

        sealed class Lettered : SomeList<Lettered>, Produces<Lettered>
        {
            public Lettered(Production<MatchEx> p) : base(p) { }

            new sealed class Production : Production<Lettered>
            {
                readonly Lettered obj;

                public Production(Lettered b) { this.obj = b; }

                public override bool Successful => obj != null;

                public override string SwitchableString => "";

                protected override Lettered _Value => obj;
            }

            public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleLettered(this);

            Production<Lettered> Produces<Lettered>.Produce() => new Production(this);
        }

        Production<List> Produces<List>.Produce() => new Production(this);
    }
}
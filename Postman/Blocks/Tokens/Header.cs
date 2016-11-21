using System;

namespace Postman.Blocks
{
    public abstract class Header : Token, Produces<Header>
    {
        Header(string line) : base(line) { }

        protected abstract string Pattern { get; }

        Header(Production<MatchEx> match) : this(match.Value.Substring) { }

        public interface Handles
        {
            SyntaxNode HandleLevel1(Header token);
            SyntaxNode HandleLevel2(Header token);
            SyntaxNode HandleLevel3(Header token);
        }

        public sealed class Production : Production<Header>
        {
            const string
                H1 = "^(=){3,}$",
                H2 = "^(-){3,}$",
                H3 = "^(_){3,}$";

            readonly Header header;

            protected override Header _Value => header;

            public Production(Header header)
            {
                this.header = header;
            }

            public Production(string line)
            {
                var x =
                        from h1 in line.Get(H1)
                        from h2 in line.Get(H2)
                        from h3 in line.Get(H3)
                        select h1 / h2 / h3;

                switch (x)
                {
                    case H1: { header = new Level1(x); break; }
                    case H2: { header = new Level2(x); break; }
                    case H3: { header = new Level3(x); break; }
                    default: { header = null; break; }
                }
            }

            public override bool Successful => header != null;
            public override string SwitchableString => header.Pattern;

            sealed class Level1 : Header
            {
                public Level1(Production<MatchEx> p) : base(p) { }
                protected override string Pattern => H1;

                public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleLevel1(this);
            }

            sealed class Level2 : Header
            {
                public Level2(Production<MatchEx> p) : base(p) { }
                protected override string Pattern => H2;

                public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleLevel2(this);
            }

            sealed class Level3 : Header
            {
                public Level3(Production<MatchEx> p) : base(p) { }
                protected override string Pattern => H3;

                public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleLevel3(this);
            }
        }

        Production<Header> Produces<Header>.Produce() => new Production(this);
    }
}
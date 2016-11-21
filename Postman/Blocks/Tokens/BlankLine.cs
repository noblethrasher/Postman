using System;

namespace Postman.Blocks
{
    public abstract class BlankLine : Token, Produces<BlankLine>
    {
        BlankLine() : base("") { }

        public sealed class Production : Production<BlankLine>
        {
            readonly BlankLine blank_line;

            public Production(BlankLine line) { this.blank_line = line; }
            public Production(string line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    this.blank_line = new Single();
            }

            protected override BlankLine _Value => blank_line;

            public override bool Successful => blank_line != null;
            public override string SwitchableString => "";
        }

        public interface Handles
        {
            SyntaxNode HandleSingleBlank(BlankLine token);
            SyntaxNode HandleDoubleBlank(BlankLine token);
        }

        public sealed override int Identation => 0;

        sealed class Single : BlankLine, Produces<Single>
        {
			new sealed class Production : Production<Single>
            {
                readonly Single obj;

				public Production(Single obj) { this.obj = obj; }

                public override bool Successful => obj != null;

                public override string SwitchableString => "";

                protected override Single _Value => obj;
            }

            public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleSingleBlank(this);            

            Production<Single> Produces<Single>.Produce() => new Production(this);

            public override Scanned Consider(string line) => string.IsNullOrWhiteSpace(line) ? new Multiple() : base.Consider(line);            
        }

        sealed class Multiple : BlankLine, Produces<Multiple>
        {
            new sealed class Production : Production<Multiple>
            {
                readonly Multiple obj;

                public Production(Multiple obj) { this.obj = obj; }

                public override bool Successful => obj != null;

                public override string SwitchableString => "";

                protected override Multiple _Value => obj;
            }

            public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.HandleDoubleBlank(this);

            Production<Multiple> Produces<Multiple>.Produce() => new Production(this);
        }

        Production<BlankLine> Produces<BlankLine>.Produce() => new Production(this);        
    }
}
using System;

namespace Postman.Blocks
{
    public sealed class Plain : Token, Produces<Plain>
    {
        public Plain(string line) :  base(line)
        {
            
        }

        public interface Handles
        {
            SyntaxNode Handle(Plain token);
        }

        public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.Handle(this);

        public sealed class Production : Production<Plain>
        {
            readonly Plain line;

            public Production(Plain line)
            {
                this.line = line;
            }

            public Production(string line) : this(new Plain(line))
            {

            }

            public override bool Successful => line != null;

            public override string SwitchableString => "";

            protected override Plain _Value => line;
        }

        Production<Plain> Produces<Plain>.Produce() => new Production(this);
    }
}
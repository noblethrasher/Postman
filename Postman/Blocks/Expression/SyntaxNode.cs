using System;
using System.Collections.Generic;

namespace Postman.Blocks
{
    public abstract class SyntaxNode :  HandlesAll
    {
        readonly Stack<SyntaxNode> children = new Stack<SyntaxNode>();

        public abstract SyntaxNode Parse(Token t);
        public abstract SyntaxNode Handle(Plain token);
        public abstract SyntaxNode Handle(Comment token);
        public abstract SyntaxNode Handle(EndOfFile token);
        public abstract SyntaxNode HandleBulleted(List token);
        public abstract SyntaxNode HandleLettered(List token);
        public abstract SyntaxNode HandleLevel1(Header token);
        public abstract SyntaxNode HandleLevel2(Header token);
        public abstract SyntaxNode HandleLevel3(Header token);
        public abstract SyntaxNode HandleNumbered(List token);
        public abstract SyntaxNode HandleDoubleBlank(BlankLine token);
        public abstract SyntaxNode HandleSingleBlank(BlankLine token);
        public abstract SyntaxNode Handle(StartsWithRightAngleBracket token);
    }

    public sealed class Root : SyntaxNode
    {
        public override SyntaxNode Handle(Comment token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode Handle(EndOfFile token) => this;

        public override SyntaxNode Handle(StartsWithRightAngleBracket token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode Handle(Plain token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleBulleted(List token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleDoubleBlank(BlankLine token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleLettered(List token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleLevel1(Header token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleLevel2(Header token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleLevel3(Header token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleNumbered(List token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode HandleSingleBlank(BlankLine token)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode Parse(Token t)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class NonRoot : SyntaxNode
    {
        protected readonly SyntaxNode parent;
        protected readonly int scope;


        public NonRoot(SyntaxNode node)
        {
            if (object.ReferenceEquals(this, node))
                throw new InvalidOperationException("A node  is not allowed to be a parent of itself");

            this.parent = node;
        }
    }

    public abstract class ShadowRoot : NonRoot
    {
        public ShadowRoot(SyntaxNode node) : base(node) { }
    }

    public abstract class NotShadowRoot :  NonRoot
    {
        public NotShadowRoot(SyntaxNode node) : base(node) { }

        public override SyntaxNode Parse(Token t)
        {
            if (t.Identation <= scope)
                return parent.Parse(t);
            else
                throw new NotImplementedException();
        }
    }
}
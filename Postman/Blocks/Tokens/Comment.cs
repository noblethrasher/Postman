using System;
using System.Text.RegularExpressions;

namespace Postman.Blocks
{
    public sealed class Comment : Token
    {
        readonly Match match;

        public Comment(Match m, string line) : base(line)
        {
            this.match = m;
        }

        public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.Handle(this);

        public interface Handles { SyntaxNode Handle(Comment token); }

        public sealed class Production :Production<Comment>
        {
            static readonly Regex pattern = new Regex(@"^(\s*//\s*).*$");

            readonly Comment token;

            public Production(string line)
            {
                var match = pattern.Match(line);

                if (match.Success)
                    token = new Comment(match, line);
            }

            public override bool Successful => token != null;

            public override string SwitchableString => "";

            protected override Comment _Value => token;
        }
    }
}
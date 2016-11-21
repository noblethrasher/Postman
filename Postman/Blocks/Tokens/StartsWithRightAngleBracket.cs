using System;
using System.Text.RegularExpressions;

namespace Postman.Blocks
{
    public sealed class StartsWithRightAngleBracket : Token
    {
        readonly Match match;

        public StartsWithRightAngleBracket(Match m, string line) : base(line)
        {
            match = m;
        }

        public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.Handle(this);

        public interface Handles { SyntaxNode Handle(StartsWithRightAngleBracket token); }

        public sealed class Production : Production<StartsWithRightAngleBracket>
        {
            static readonly Regex pattern = new Regex(@"^(\s*>\s*).*$", RegexOptions.Compiled);

            readonly StartsWithRightAngleBracket token;

            public Production(string line)
            {
                var match = pattern.Match(line);

                if(match.Success)
                    token = new StartsWithRightAngleBracket(match, line);                
            }

            public override bool Successful => token != null;

            public override string SwitchableString => "";

            protected override StartsWithRightAngleBracket _Value => token;
        }
    }
}
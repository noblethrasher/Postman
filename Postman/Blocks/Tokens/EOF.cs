namespace Postman.Blocks
{
    public sealed class EndOfFile : Token
    {
        public EndOfFile() :  base(null)
        {

        }

        public override SyntaxNode GetSyntaxNode(HandlesAll node) => node.Handle(this);

        public interface Handles { SyntaxNode Handle(EndOfFile token); }

        public override int Identation => -1;
    }
}
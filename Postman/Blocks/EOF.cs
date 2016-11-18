namespace Postman.Blocks
{
    public sealed class EndOfFile : Token
    {
        public EndOfFile() :  base(null)
        {

        }

        public override int Identation => -1;
    }
}
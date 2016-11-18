using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Postman.Blocks;
using System.IO;

using static System.Console;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fs = new FileStream(@"C:\Users\Rodrick Chapman\Desktop\postman.txt", FileMode.Open))
            {
                foreach (var token in new Tokenized(fs))
                    WriteLine(token);
            }

            Console.ReadLine();
        }
    }
}

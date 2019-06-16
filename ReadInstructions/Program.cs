using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chip8.Core;

namespace Chip8.ReadInstructions
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);

            Processor processor = new Processor(args[0]);
            processor.Run();

            Console.ReadLine();
        }
    }
}

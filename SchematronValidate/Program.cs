using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SchematronValidate
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguments");
                PrintHelp();
            }


        }

        private static void PrintHelp()
        {
            Console.WriteLine("{0} xsd-file input-file", Assembly.GetExecutingAssembly().GetName().Name);
        }
    }
}

using System;
using WMAExcel;

namespace testExcel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("please provide the input file name");
                return;
            }
            string fn = args[0];
            ExcelMain excel = new ExcelMain();
            bool result = excel.LoadFromFile(fn);
            Console.WriteLine($"parse result: {result}");
        }
    }
}

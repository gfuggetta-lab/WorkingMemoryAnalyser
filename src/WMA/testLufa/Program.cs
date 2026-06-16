using System;
using System.Threading.Tasks;
using LufaUSB;
using WMAData;

namespace testLufa
{
    class Program
    {

        static string GetPortFromInput()
        {
            Console.WriteLine("please type in the serial port to try (i.e. COM1)");
            return Console.ReadLine();
        }

        static async Task Main(string[] args)
        {
            string ser = "";
            int delay = 500;
            if (args.Length > 0)
                ser = args[0];
            if (string.IsNullOrEmpty(ser))
                ser = GetPortFromInput();
            
            if (string.IsNullOrEmpty(ser))
            {
                Console.WriteLine("the serial port is not specified");
                return;
            }
            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out var nval))
                {
                    delay = Math.Abs(nval);
                }
            }
            Console.WriteLine($"delay between steps: {delay} ms");

            using (LufaNotifier lf = new LufaNotifier(ser))
            {
                lf.logResponse = true;
                lf.Logger += (string msg) =>
                {
                    Console.WriteLine($"< {msg}");
                };
                Console.WriteLine("> starting trial...");
                await lf.StartTrial();
                await Task.Delay(delay);

                Console.WriteLine("> marker s1");
                await lf.MarkerS1(0);
                await Task.Delay(delay);

                Console.WriteLine("> marker s2");
                await lf.MarkerS2(0);
                await Task.Delay(delay);

                Console.WriteLine("> marker s3");
                await lf.MarkerS3(0);
                await Task.Delay(delay);

                Console.WriteLine("> marker s4");
                await lf.MarkerS4(0);
                await Task.Delay(delay);

                Console.WriteLine("> feedback incorrect");
                await lf.Feedback(false);
                //await Task.Delay(500);
            }
            Console.WriteLine("completed");
        }
    }
}

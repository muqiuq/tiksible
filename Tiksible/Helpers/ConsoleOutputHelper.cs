using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Helpers
{
    public static class ConsoleOutputHelper
    {

        public static string MakeDeviderLine(string input)
        {
            int totalWidth = Console.WindowWidth;
            int inputLength = input.Length + 5; // 3 for "=== " and 2 for " "
            int remainingWidth = totalWidth - inputLength;
            if (remainingWidth < 0) remainingWidth = 0;

            return $"=== {input} {new string('=', remainingWidth)}";
        }

        public static void PrintStatusLine(string input, bool isSuccess)
        {
            string status = isSuccess ? "OK" : "FAIL";
            Console.Write($"{input}: ");

            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(status);
            Console.ForegroundColor = currentColor;
        }
    }
}

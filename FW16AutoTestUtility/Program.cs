using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW16AutoTestUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AutoTestUtility v 0.5.0.1";
            Tests fw16;
            int serialPort;
            switch (args.Length)
            {
                case 0:
                    fw16 = new Tests();
                    break;
                case 1:
                    Int32.TryParse(args[0], out serialPort);
                    fw16 = new Tests(serialPort);
                    break;
                case 2:
                    Int32.TryParse(args[0], out serialPort);
                    Int32.TryParse(args[1], out int baudRate);
                    fw16 = new Tests(serialPort, baudRate);
                    break;
            }
            Console.WriteLine("(press any key)");
            Console.ReadKey();
        }
    }
}
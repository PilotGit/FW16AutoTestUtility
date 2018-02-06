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

            Console.Title= "AutoTestUtility v 0.3.3.3";
            int serialPort, baudRate;
            Tests fw16 = new Tests();
            Console.WriteLine("(press any key)");
            Console.ReadKey();
        }
    }
}

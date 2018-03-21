using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW16AutoTestUtility
{
    class Program
    {
        public static bool[] testing = new bool[30];
        static void Main(string[] args)
        {
            Console.Title = "AutoTestUtility v 0.5.2.0";
            Tests fw16;
            int serialPort = -1;
            int baudRate = -1;
            testing[20] = true;
            try
            {
                foreach (var item in args)
                {

                    switch (item[0])
                    {

                        case 'P':
                            try
                            {
                                serialPort = Int32.Parse(item.Substring(1));
                            }
                            catch
                            {
                                throw new Exception($"Введён некорректный параметр. {item}");
                            }
                            continue;
                        case 'R':
                            try
                            {
                                baudRate = Int32.Parse(item.Substring(1));
                            }
                            catch
                            {
                                throw new Exception($"Введён некорректный параметр. {item}");
                            }
                            continue;
                    }
                    switch (item)
                    {
                        case "+S":
                            testing[0] = true;
                            testing[1] = true;
                            testing[2] = true;
                            testing[3] = true;
                            testing[4] = true;
                            testing[6] = true;
                            testing[7] = true;
                            testing[8] = true;
                            testing[11] = true;
                            testing[13] = true;
                            testing[14] = true;
                            //testing[16] = true;
                            //testing[17] = true;
                            //testing[18] = true;
                            break;
                        case "+R": testing[1] = true; testing[0] = true; break;
                        case "-R": testing[1] = false; break;
                        case "+C": testing[2] = true; testing[0] = true; break;
                        case "-C": testing[2] = false; break;
                        case "+N": testing[3] = true; testing[0] = true; break;
                        case "-N": testing[3] = false; break;
                        case "+P": testing[4] = true; testing[0] = true; break;
                        case "-P": testing[4] = false; break;
                        case "+Rm": testing[6] = true; testing[0] = true; testing[1] = true; break;
                        case "-Rm": testing[6] = false; break;
                        case "+Cm": testing[7] = true; testing[0] = true; testing[2] = true; break;
                        case "-Cm": testing[7] = false; break;
                        case "+Nm": testing[8] = true; testing[0] = true; testing[3] = true; break;
                        case "-Nm": testing[8] = false; break;
                        case "+r": testing[11] = true; testing[0] = true; break;
                        case "-r": testing[11] = false; break;
                        case "+n": testing[13] = true; testing[0] = true; break;
                        case "-n": testing[13] = false; break;
                        //case "+p": testing[14] = true; break; не реализованно
                        //case "-p": testing[14] = false; break; не реализованно
                        //case "+rm": testing[16] = true; testing[0] = true; break;
                        //case "-rm": testing[16] = false; break;
                        //case "+cm": testing[17] = true; testing[0] = true; break;
                        //case "-cm": testing[17] = false; break;
                        //case "+nm": testing[18] = true; testing[0] = true; break;
                        //case "-nm": testing[18] = false; break;
                        case "-check":testing[20] = false; break;
                        default:
                            throw new Exception($"Введён некорректный параметр. {item}");
                    }
                }
                if (testing[0])
                {
                    if (serialPort != -1)
                    {
                        if (baudRate != -1)
                        {
                            fw16 = new Tests(serialPort, baudRate);
                        }
                        fw16 = new Tests(serialPort);
                    }
                    else
                    {
                        fw16 = new Tests();
                    }
                }
                else
                {
                    Console.Write($"Возможные параметры запуска\n" +
                        $"P<Номер_порта> - установить порт подключения\n" +
                        $"R<Частота> - установить частоту порта\n" +
                        $"+S - Включить simpletest в тестирование со всеми модулями\n\n" +
                        $"      Последующие параметры должны иметь префикс + или -\n" +
                        $"      Что означает включить или отключить определённый модуль теста\n\n" +
                        $"R  - тестирование чека на больших данных\n" +
                        $"C  - тестирование чека коррекции на больших данных\n" +
                        $"N  - тестирование нефискальных документов на больших данных\n" +
                        $"P  - тестирование чеков с оплатой кредита\n" +
                        $"Rm - тестирование чеков с минимальным колчисеством входных данных для локализации ошибочных регистров\n" +
                        $"      в тест будет автоматически включён модуль тестирования чека на больших данных\n" +
                        $"Cm - тестирование чеков коррекции с минимальным колчисеством входных данных для локализации ошибочных регистров\n" +
                        $"      в тест будет автоматически включён модуль тестирования чека коррекции на больших данных\n" +
                        $"Nm - тестирование нефискальных документов с минимальным колчисеством входных данных для локализации ошибочных регистров\n" +
                        $"      в тест будет автоматически включён модуль тестирования нефискального на больших данных\n" +
                        $"r  - тестирование чека на больших данных c отменой документа\n" +
                        $"n  - тестирование чека на больших данных с отменой документа\n" +
                        $"-check  -  отключить промежуточные роверки регистров\n" +
                        $"           (автоматически перебираются все варианты для включенных документов с минимальным количеством данных)");
                }
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
                Console.Write($"Возможные параметры запуска\n" +
                        $"P<Номер_порта> - установить порт подключения\n" +
                        $"R<Частота> - установить частоту порта\n" +
                        $"+S - Включить simpletest в тестирование со всеми модулями\n\n" +
                        $"      Последующие параметры должны иметь префикс + или -\n" +
                        $"      Что означает включить или отключить определённый модуль теста\n\n" +
                        $"R  - тестирование чека на больших данных\n" +
                        $"C  - тестирование чека коррекции на больших данных\n" +
                        $"N  - тестирование нефискальных документов на больших данных\n" +
                        $"P  - тестирование чеков с оплатой кредита\n" +
                        $"Rm - тестирование чеков с минимальным колчисеством входных данных для локализации ошибочных регистров\n" +
                        $"      в тест будет автоматически включён модуль тестирования чека на больших данных\n" +
                        $"Cm - тестирование чеков коррекции с минимальным колчисеством входных данных для локализации ошибочных регистров\n" +
                        $"      в тест будет автоматически включён модуль тестирования чека коррекции на больших данных\n" +
                        $"Nm - тестирование нефискальных документов с минимальным колчисеством входных данных для локализации ошибочных регистров\n" +
                        $"      в тест будет автоматически включён модуль тестирования нефискального на больших данных\n" +
                        $"r  - тестирование чека на больших данных c отменой документа\n" +
                        $"n  - тестирование чека на больших данных с отменой документа\n" +
                        $"-check  -  отключить промежуточные роверки регистров\n" +
                        $"           (автоматически перебираются все варианты для включенных документов с минимальным количеством данных)");

            }
            //Console.WriteLine("(press any key)");
            //Console.ReadKey();
        }
    }
}
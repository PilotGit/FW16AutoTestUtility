using System;
using System.Collections.Generic;
using System.Text;
using Fw16;
using Fw16.Model;

namespace FWAutoTestUtility
{
    class Class1
    {
        /// <summary>
        /// Количество цен
        /// </summary>
        const int countCoasts = 2;
        /// <summary>
        /// Количество вариантов количеств
        /// </summary>
        const int countCounts = 4;
        /// <summary>
        /// Количество типов оплаты
        /// </summary>
        const int countPaymentKind = 6;
        /// <summary>
        /// Количество типов чеков
        /// </summary>
        const int countReceiptKind = 4;
        /// <summary>
        /// Количество типов оплаты
        /// </summary>
        const int countTenderCode = 8;
        /// <summary>
        /// Количество ставок НДС
        /// </summary>
        const int countVatCode = 6;


        public EcrCtrl ecrCtrl;                                     //подключение к ККТ
        public int[] counters = new int[23];                        //массив счётчиков
        public decimal[] registers = new decimal[236];              //массив регистров
        public List<int> inaccessibleRegisters = new List<int>();
        decimal[] registersTmp = new decimal[236];                  //массив временных регистров
        string nameOerator = "test program";                        //имя касира 
        decimal[] coasts = new decimal[] { 217m, 193.7m };          //варианты цен
        decimal[] counts = new decimal[] { 1m, 5m, 0.17m, 1.73m };  //варианты колличества


        public Class1()
        {
            ecrCtrl = new EcrCtrl();
            ConnectToFW();
            BeginTest();
        }

        /// <summary>
        /// Подключение к ККТ
        /// </summary>
        /// <param name="serialPort">Порт по покотору производится поключение к ККТ</param>
        /// <param name="baudRate">Частота подключения</param>
        void ConnectToFW(int serialPort = 1, int baudRate = 57600)
        {
            try
            {
                ecrCtrl.Init(serialPort, baudRate);             //Подключчение по порту и частоте
                ShowInformation();
            }
            catch (EcrException excep)
            {
                ecrCtrl.Reconnect();                            //Переподключение в случае попытки повторного подключения
                System.Diagnostics.Debug.Write(excep.Message);
            }
            catch (System.IO.IOException excep)
            {
                Console.WriteLine(excep.Message);                 //вывод ошибки неверного порта
            }
            catch (System.UnauthorizedAccessException excep)
            {
                Console.WriteLine(excep.Message);                 //вывод ошибки доступа порта
            }

        }

        void ShowInformation()
        {
            Console.WriteLine("ККТ: подключено");
            Console.WriteLine("Версия прошивки: " + ecrCtrl.Info.FactoryInfo.FwBuild);
            Console.WriteLine("Код firmware: " + ecrCtrl.Info.FactoryInfo.FwType);
            Console.WriteLine("Серийный номер ККТ: " + ecrCtrl.Info.EcrInfo.Id);
            Console.WriteLine("Модель: " + ecrCtrl.Info.EcrInfo.Model);
        }

        /// <summary>
        /// Начать тест
        /// </summary>
        private void BeginTest()
        {
            ConnectToFW();
            Preparation();
            SimpleTest();
        }

        /// <summary>
        /// Подготовка к корректному выполнению тестов. Отключение печати, отмена всех документов, закрытие смен, получение соответствий номера платежа к типу платежа.
        /// </summary>
        public void Preparation()
        {
            ecrCtrl.Service.SetParameter(Native.CmdExecutor.ParameterCode.AbortDocFontSize, "51515");               //отключение печати чека
            if ((ecrCtrl.Info.Status & Fw16.Ecr.GeneralStatus.DocOpened) > 0)
            {
                ecrCtrl.Service.AbortDoc();                                                                         //закрыть документ если открыт
            }
            if ((ecrCtrl.Info.Status & Fw16.Ecr.GeneralStatus.ShiftOpened) > 0)
            {
                ecrCtrl.Shift.Close(nameOerator);                                                                   //закрыть смену если открыта
            }
        }

        public void SimpleTest()                            //функция прогона по всем видам чеков и чеков коррекции
        {
            ecrCtrl.Shift.Open(nameOerator);                //открытие смены для этого теста
            GetRegisters();
            GetCounters();
            TestReceipt();                                  //вызов функции тестирования чека
            TestCorrection();                               //вызов функции тестирования чека коррекции
            TestNonFiscal();                                //вызов функции нефискального документа
            TestReceipt(true);                              //вызов функции тестирования чека c отменой.
            TestNonFiscal(true);                            //вызов функции нефискального документа с отменой
            ecrCtrl.Shift.Close(nameOerator);               //закрытие смены этого теста

            RequestRegisters();
            RequestCounters();

            Console.WriteLine("Завершено тестирование SimpleTest ");     //логирование

            //TestCorrection(true);                         //вызов функции тестирования чека коррекции с отменой
            //отключено в связи с тем что чек коррекции не возможно отменить, потому что он отправляется одним пакетом
        }

        /// <summary>
        /// Тест нефискального документа
        /// </summary>
        /// <param name="abort">Отменить создание нефискального документа</param>
        private void TestNonFiscal(bool abort = false)
        {
            for (int nfdType = 1; nfdType < 4; nfdType++)                                           //Перебор типов нефиксальных документов
            {
                var document = ecrCtrl.Shift.BeginNonFiscal((Native.CmdExecutor.NFDocType)nfdType); //открытие нефиксального документа
                SetValue(registersTmp, 0);
                for (int i = 0; i < countCoasts*countTenderCode && nfdType != 3; i++)                                         //
                {
                    var tender = new Tender
                    {
                        Amount = coasts[i / countTenderCode],
                        Code = (Native.CmdExecutor.TenderCode)(i % countTenderCode)
                    };
                    //document.AddTender(tender);
                    
                   AddTender(document, (Native.CmdExecutor.NFDocType)nfdType, (Native.CmdExecutor.TenderCode)(i / countCoasts % countTenderCode), coasts[i %countCoasts]);
                }
                document.PrintText("Тестовый текст теста текстовго нефиксального документа");
                if (abort)
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);                                            //отмена нефискального документа
                    Console.WriteLine("Отменён нефиксальный документ типа " + (Native.CmdExecutor.NFDocType)nfdType + "");      //логирование
                    counters[nfdType + 8 + 11]++;
                }
                else
                {
                    document.Complete(Native.CmdExecutor.DocEndMode.Default);                                                   //закрытие нефиксального документа
                    Console.WriteLine("Оформлен нефиксальный документ типа " + (Native.CmdExecutor.NFDocType)nfdType + "");     //логирование
                    counters[nfdType + 8]++;
                    AddRegistersTmp();
                }
                RequestRegisters(111, 120);
            }
        }

        /// <summary>
        /// Тест чека коррекции
        /// </summary>
        /// <param name="abort">Отменить создание чека коррекции</param>
        private void TestCorrection(bool abort = false)
        {
            for (int receiptKind = 1; receiptKind < 4; receiptKind += 2)
            {
                var document = ecrCtrl.Shift.BeginCorrection(nameOerator, (ReceiptKind)receiptKind);
                SetValue(registersTmp, 0);
                decimal sum = 0;

                for (int i = 0; i < countCoasts * countTenderCode; i++)         //перебор возврата средств всеми способами, целове и дробная суммы
                {
                    AddTender(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)(i / countCoasts % countTenderCode), coasts[i % countCoasts]);
                    sum += coasts[i % countCoasts];
                }
                decimal sumPaid = 0m;
                for (ushort i = 1; i <= countVatCode; i++)                      //перебор налоговых ставок
                {
                    sumPaid = Math.Round(sum / ((countVatCode+1) - i), 2);
                    AddAmount(document, (ReceiptKind)receiptKind, (VatCode)i, sumPaid);
                    sum = sum - sumPaid;
                }

                if (abort)
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);
                    Console.WriteLine("Отменён чек коррекции типа " + (ReceiptKind)receiptKind + "");         //логирование
                    counters[receiptKind + 4 + 11]++;
                }
                else
                {
                    document.Complete();                                                                                //закрытие чека коррекции
                    Console.WriteLine("Оформлен чек коррекции типа " + (ReceiptKind)receiptKind + "");        //логирование
                    counters[receiptKind + 4]++;
                    AddRegistersTmp();
                }
            }
        }

        /// <summary>
        /// Тест чека    
        /// </summary>
        /// <param name="abort">Отменить создание чека</param>
        private void TestReceipt(bool abort = false)
        {
            for (int receiptKind = 1; receiptKind < 5; receiptKind++)
            {
                var document = ecrCtrl.Shift.BeginReceipt(nameOerator, (ReceiptKind)receiptKind, new
                {
                    Taxation = Fs.Native.TaxationType.Agro,         //налогообложение по умолчанию
                    CustomerAddress = "qwe@ewq.xxx",                //адрес получателя
                    SenderAddress = "ewq@qwe.yyy"                   //адрес отправтеля
                });
                SetValue(registers, 0, 160, 182);
                SetValue(registersTmp, 0);
                bool coast = true;
                for (int i = 0; i < (countCounts * countVatCode * countCoasts * countPaymentKind); i++)
                {

                    AddEntry(document,
                        (ReceiptKind)receiptKind,
                        "Tovar" + i.ToString(),
                        counts[i / countVatCode / countCoasts / countPaymentKind % countCounts],
                        (Native.CmdExecutor.VatCodeType)((i / countCoasts / countPaymentKind % countVatCode) + 1),
                        coast,
                        coasts[i / countPaymentKind % countCoasts],
                        (ItemPaymentKind)((i % countPaymentKind) + 1));  //создание товара
                }
                decimal sum = 0m;
                for (int tenderCode = 1; tenderCode < countTenderCode; tenderCode++)
                {
                    sum = Math.Round(document.Total / 9 - tenderCode, 2);
                    sum += (decimal)(new Random().Next(-1 * (int)sum * (5 / 100), (int)sum * (5 / 100)));
                    AddPayment(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)tenderCode, sum);
                    sum = document.Total - document.TotalaPaid;
                }
                AddPayment(document, (ReceiptKind)receiptKind, Native.CmdExecutor.TenderCode.Cash, sum);
                //
                if (abort)
                {
                    document.Abort();                                                                           //отмена документа
                    Console.WriteLine("Отменён чек типа " + (ReceiptKind)receiptKind + "");                     //логирование
                    counters[receiptKind + 11]++;                                                               //увеличение счётчика (12-15) отмены соотвествующего типа чека

                }
                else
                {
                    document.Complete();                                                                        //закрытие чека
                    Console.WriteLine("Оформлен чек типа " + (ReceiptKind)receiptKind + "");                    //логирование
                    counters[receiptKind]++;                                                                    //учеличение счётчика (1-4) оформленного соответсвющего типа чека
                    SetValue(registers, 0, 160, 182);
                    AddRegistersTmp();
                }
                RequestRegisters(160, 182);                                                                     //запрос регистров по открытому документу
            }
        }

        /// <summary>
        /// Сверяет регистры с массивом регистров в указанном диапозоне
        /// </summary>
        /// <param name="startIndex">Начальный индекс</param>
        /// <param name="endIndex">Конечный индекс, не включительно</param>
        public void RequestRegisters(ushort startIndex = 1, ushort endIndex = 0)
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)236;                                                           //проверка конечного значения если 0, то до конца
            string err = $"+-------+------------------+-------------------+\n" +
                $"|   #   |       test       |        ККТ        |\n" +
                $"+-------+------------------+-------------------+\n";                                                                                            //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i < endIndex; i++)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1)
                {
                    try
                    {
                        decimal tmp = ecrCtrl.Info.GetRegister(i);
                        if (tmp != registers[i]) { err += $"|{i,7:F}|{registers[i],18:F}|{tmp,19:F}|\n"; }//заполнение ошибки несоотвествия регистров
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не удолось получить доступ к регистру №" + i + "");
                    }
                }
            }
            Console.WriteLine("Запрошены данные с регистров с " + startIndex + " по " + endIndex + "\n" + ((err.Length>150)?err:""));           //логирование
        }

        /// <summary>
        /// Сверяет счётчики с массивом счтчиков в указанном диапозоне
        /// </summary>
        /// <param name="startIndex">Начальный индекс</param>
        /// <param name="endIndex">Конечный индекс, не включительно</param>
        public void RequestCounters(ushort startIndex = 1, ushort endIndex = 0)
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)23;                                                            //проверка конечного значения если 0, то до конца
            string err = "";                                                                                              //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i < endIndex; i++)
            {
                try
                {
                    int tmp = ecrCtrl.Info.GetCounter(i);
                    if (tmp != counters[i]) { err += $"Счётчик {i} имеет расхождеие с ККТ {counters[i]} != {tmp}\n"; }    //Зполнение ошибки несоотвествия счётчиков
                }
                catch (Exception)
                {
                    Console.WriteLine("Не удолось получить доступ к счётчику №" + i + "");                              //ошибка доступа к регистру
                }
            }
            Console.WriteLine("Запрошены данные с счётчиков с " + startIndex + " по " + endIndex + "\n" + err);           //логирование
        }

        /// <summary>
        /// считывает все регистры в массив регистров
        /// </summary>
        public void GetRegisters()
        {
            ushort endIndex = 236;
            ushort startIndex = 1;
            for (ushort i = startIndex; i < endIndex; i++)
            {
                try
                {
                    registers[i] = ecrCtrl.Info.GetRegister(i);             //запрос значений регистров из ККТ
                }
                catch (Exception)
                {
                    Console.WriteLine("Не удолось получить доступ к регистру №" + i + " за стартовое значение принят 0");
                    inaccessibleRegisters.Add(i);
                }
            }
            Console.WriteLine("Запрошены данные с регистров получены");     //логирование
        }

        /// <summary>
        /// Считывает все счтчики в массив счётчиков
        /// </summary>
        public void GetCounters() 
        {
            ushort endIndex = 23;
            ushort startIndex = 1;
            for (ushort i = startIndex; i < endIndex; i++)
            {
                try
                {
                    counters[i] = ecrCtrl.Info.GetCounter(i);               //запрос значений регистров из ККТ
                }
                catch (Exception)
                {
                    Console.WriteLine("Не удолось получить доступ к счётчику №" + i + " за стартовое значение принят 0");
                }
            }
            Console.WriteLine("Данные с счётчиков получены");               //логирование
        }

        /// <summary>
        /// Применяет изменения врменного регистра в основной
        /// </summary>
        public void AddRegistersTmp()
        {
            ushort endIndex = 236;
            ushort startIndex = 1;
            for (int i = startIndex; i < endIndex; i++)
            {
                registers[i] += registersTmp[i];                                                        //применение временного массива к конечному
            }
        }

        /// <summary>
        /// Утсановка значения  для каждого элемента или в заданном диапозоне
        /// </summary>
        /// <param name="arr">Массив</param>
        /// <param name="value">Значение</param>
        /// <param name="startIndex">Индекс с которого надо заполнять массив значениям</param>
        /// <param name="endIndex">Конечный индекс заполнения, не включается</param>
        void SetValue(decimal[] arr, decimal value, ushort startIndex = 0, ushort endIndex = 0)
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)arr.Length;
            for (int i = startIndex; i < endIndex; i++)
            {
                arr[i] = value;
            }
        }

    }
}
/*
регистры 111-119 нигде не учитываются
     */
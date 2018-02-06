using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Fw16;
using Fw16.Model;

namespace FW16AutoTestUtility
{
    class Tests
    {
        TestingInterfaceFW16 TestingInterfaceFW16 = null;
        public EcrCtrl ecrCtrl;                                     //подключение к ККТ
        string nameOperator = "test program";                        //имя касира 
        decimal[] costs = new decimal[] { 217m, 193.7m };          //варианты цен
        decimal[] counts = new decimal[] { 1m, 5m, 0.17m, 1.73m };  //варианты колличества
        Random random = new Random();

        public Tests()
        {
            TestingInterfaceFW16 = new TestingInterfaceFW16(out ecrCtrl);
            if (ecrCtrl.Fw16 != null)
            {
                BeginTest();
            }
            else
            {
                Console.WriteLine("Не удалось провести тестирование");
            }
        }

        /// <summary>
        /// Начать тест
        /// </summary>
        private void BeginTest()
        {
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
                ecrCtrl.Shift.Close(nameOperator);                                                                   //закрыть смену если открыта
            }
        }

        public void SimpleTest()                            //функция прогона по всем видам чеков и чеков коррекции
        {
            TestingInterfaceFW16.OpenShift(nameOperator);   //открытие смены для этого теста
            TestingInterfaceFW16.GetRegisters();
            TestingInterfaceFW16.GetCounters();
            if (TestReceiptMax() != 0)                          //вызов функции тестирования чека
            {
                Console.WriteLine($"+------------+-----------------+-----------------+--------+---------------+---------------+\n" +
                    $"|{"Тип чека",12}|{"Процентная ставка",17}|{"Тип оплаты",17}|{"Товар по",8}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
                    $"+------------+-----------------+-----------------+--------+---------------+---------------+\n" + TestReceiptMin());
            }
            if (TestCorrectionMax() != 0)                       //вызов функции тестирования чека коррекции
            {
                Console.WriteLine($"+------------+-----------------+---------------+---------------+\n" +
                    $"|{"Тип чека",12}|{"Процентная ставка",17}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
                    $"+------------+-----------------+---------------+---------------+\n" + TestCorrectionMin());
            }
            if (TestNonFiscalMax() != 0)                        //вызов функции нефискального документа
            {
                Console.WriteLine($"+------------+---------------+---------------+\n" +
                    $"|{"Тип чека",12}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
                    $"+------------+---------------+---------------+\n" + TestNonFiscalMin());
            }
            TestReceiptMax(true);                               //вызов функции тестирования чека c отменой.
            TestNonFiscalMax(true);                             //вызов функции нефискального документа с отменой
                                                                //закрытие смены этого теста
            TestingInterfaceFW16.CloseShift(nameOperator);      //Закрытие смены для этого теста

            TestingInterfaceFW16.RequestRegisters();
            TestingInterfaceFW16.RequestCounters();

            Console.WriteLine("Завершено тестирование SimpleTest ");     //логирование

            //TestCorrection(true);                         //вызов функции тестирования чека коррекции с отменой
            //отключено в связи с тем что чек коррекции не возможно отменить, потому что он отправляется одним пакетом
        }

        /// <summary>
        /// Тест нефискального документа
        /// </summary>
        /// <param name="abort">Отменить создание нефискального документа</param>
        private int TestNonFiscalMax(bool abort = false)
        {
            int ret = 0;
            int countNFDoc = TestingInterfaceFW16.countNFDocType;
            int i = 1;
            for (int nfDocType = 1; nfDocType <= TestingInterfaceFW16.countNFDocType; nfDocType++)                                           //Перебор типов нефиксальных документов
            {
                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.NonFiscalBase document, (Native.CmdExecutor.NFDocType)nfDocType);
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode && nfDocType != 3; tenderCode++)
                {
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                         //
                    {
                        var tender = new Tender
                        {
                            Amount = costs[cost],
                            Code = (Native.CmdExecutor.TenderCode)(tenderCode)
                        };
                        TestingInterfaceFW16.AddTender(document, (Native.CmdExecutor.NFDocType)nfDocType, (Native.CmdExecutor.TenderCode)tenderCode, costs[cost]);
                    }
                }
                document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                Console.Write($"({i++}/{countNFDoc}) ");

                ret += TestingInterfaceFW16.DocumentComplete(document, (Native.CmdExecutor.NFDocType)nfDocType, abort);
            }

            return ret;
        }

        /// <summary>
        /// Тест нефискального документа
        /// </summary>
        /// <param name="abort">Отменить создание нефискального документа</param>
        private string TestNonFiscalMin(bool abort = false)
        {
            string err = null;
            int countNFDoc = TestingInterfaceFW16.countNFDocType * TestingInterfaceFW16.countTenderCode;
            int i = 1;

            for (int nfDocType = 1; nfDocType < TestingInterfaceFW16.countNFDocType; nfDocType++)                                           //Перебор типов нефиксальных документов
            {
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode && nfDocType != 3; tenderCode++)                                         //
                {
                    TestingInterfaceFW16.StartDocument(out Fw16.Ecr.NonFiscalBase document, (Native.CmdExecutor.NFDocType)nfDocType);
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)
                    {
                        var tender = new Tender
                        {
                            Amount = costs[cost],
                            Code = (Native.CmdExecutor.TenderCode)(tenderCode)
                        };
                        TestingInterfaceFW16.AddTender(document, (Native.CmdExecutor.NFDocType)nfDocType, (Native.CmdExecutor.TenderCode)(tenderCode), costs[cost]);
                    }

                    document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                    Console.Write($"({i++}/{countNFDoc}) ");

                    if (TestingInterfaceFW16.DocumentComplete(document, (Native.CmdExecutor.NFDocType)nfDocType, abort) != 0)
                    {
                        err += $"|{(Native.CmdExecutor.NFDocType)nfDocType,12}|{(Native.CmdExecutor.TenderCode)tenderCode,15}|{(Native.CmdExecutor.TenderType)TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode],15}|\n";
                        TestingInterfaceFW16.GetRegisters(new int[] { 191, 192, 193, 194 });
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Тест чека коррекции
        /// </summary>
        /// <param name="abort">Отменить создание чека коррекции</param>
        private int TestCorrectionMax(bool abort = false)
        {
            int ret = 0;
            int i = 1;
            int countCorrections = TestingInterfaceFW16.countReceiptKind;

            for (int receiptKind = 1; receiptKind < 4; receiptKind += 2)
            {
                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, (ReceiptKind)receiptKind);
                decimal sum = 0;
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                   //перебор видов платежей
                {
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                  //перебор сумм
                    {
                        TestingInterfaceFW16.AddTender(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)tenderCode, costs[cost]);
                        sum += costs[cost];
                    }
                }
                decimal sumPaid = 0m;
                for (ushort vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                             //перебор налоговых ставок
                {
                    sumPaid = Math.Round(sum / ((TestingInterfaceFW16.countVatCode + 1) - vatCode), 2);
                    TestingInterfaceFW16.AddAmount(document, (ReceiptKind)receiptKind, (VatCode)vatCode, sumPaid);
                    sum = sum - sumPaid;
                }

                Console.Write($"({i++}/{countCorrections}) ");

                ret += TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort);
            }
            return ret;
        }

        /// <summary>
        /// Тест чека коррекции
        /// </summary>
        /// <param name="abort">Отменить создание чека коррекции</param>
        private string TestCorrectionMin(bool abort = false)
        {
            string err = null;
            int i = 1;
            int countCorrections = TestingInterfaceFW16.countReceiptKind * TestingInterfaceFW16.countTenderCode * TestingInterfaceFW16.countVatCode;

            for (int receiptKind = 1; receiptKind < TestingInterfaceFW16.countReceiptKind; receiptKind += 2)
            {
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                   //перебор видов платежей
                {
                    for (ushort vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                             //перебор налоговых ставок
                    {
                        TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, (ReceiptKind)receiptKind);
                        decimal sum = 0;
                        for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                  //перебор сумм
                        {
                            TestingInterfaceFW16.AddTender(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)tenderCode, costs[cost]);
                            sum += costs[cost];
                        }
                        TestingInterfaceFW16.AddAmount(document, (ReceiptKind)receiptKind, (VatCode)vatCode, sum);

                        Console.Write($"({i++}/{countCorrections}) ");

                        if (TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort) != 0)
                        {
                            err += $"|{(ReceiptKind)receiptKind,12}|{(Native.CmdExecutor.VatCodeType)vatCode,17}|{(Native.CmdExecutor.TenderCode)tenderCode,15}|{(Native.CmdExecutor.TenderType)TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode],15}|\n";
                            TestingInterfaceFW16.GetRegisters(TestingInterfaceFW16.RegistersСumulative);
                        }
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Тестирование чека с перебором большого количества товаров
        /// </summary>
        /// <param name="abort">Отменить создание чека</param>
        private int TestReceiptMax(bool abort = false)
        {
            int ret = 0;
            int countReciepts = TestingInterfaceFW16.countReceiptKind * TestingInterfaceFW16.countItemBy;
            int i = 1;

            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)                              //перебор типов чеков
            {
                for (int itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)                                               //перебор типов добавления товара
                {
                    TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, (ReceiptKind)receiptKind);
                    for (int vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                      //перебор типов налоговой ставки
                    {
                        for (int itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)   //перебор типов оплаты товара
                        {
                            for (int item = 0; item < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countcosts); item++)              //перебор комбинаций стоиости и количества
                            {
                                TestingInterfaceFW16.AddEntry(document,
                                    (ReceiptKind)receiptKind,
                                    "Item " + vatCode + "" + itemBy + "" + itemPaymentKind + "" + item,
                                    counts[item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                                    (Native.CmdExecutor.VatCodeType)vatCode,
                                    (TestingInterfaceFW16.ItemBy)itemBy,
                                    costs[item % TestingInterfaceFW16.countcosts],
                                    (ItemPaymentKind)itemPaymentKind);  //создание товара
                            }
                        }
                    }

                    decimal sum = 0m;
                    for (int tenderCode = 1; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                           //перебор видов платежей
                    {
                        sum = Math.Round(document.Total / 9 - tenderCode, 2);
                        sum += (decimal)(random.Next(-1 * (int)sum * (10 / 100), (int)sum * (10 / 100)));
                        TestingInterfaceFW16.AddPayment(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)tenderCode, sum);
                        sum = document.Total - document.TotalaPaid;
                    }

                    TestingInterfaceFW16.AddPayment(document, (ReceiptKind)receiptKind, Native.CmdExecutor.TenderCode.Cash, sum + (random.Next(-1 * (int)sum * (10 / 100), (int)sum * (10 / 100))));       //оплата наличными
                    Console.Write($"({i++}/{countReciepts}) ");
                    ret += TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort);
                }
            }
            return ret;
        }

        /// <summary>
        /// Тестирование чека с перебором множества небольших чеков и формирование таблицы ошибочных чеков
        /// </summary>
        /// <param name="abort">Булево значение отмены чека</param>
        /// <returns>Строка формирующая таблицу ошибочных чеков</returns>
        private string TestReceiptMin(bool abort = false)
        {
            string err = null;
            int countReciepts = TestingInterfaceFW16.countReceiptKind * TestingInterfaceFW16.countVatCode * TestingInterfaceFW16.countItemPaymentKind * TestingInterfaceFW16.countItemBy * TestingInterfaceFW16.countTenderCode;
            int i = 1;

            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)                                      //перебор типов чеков
            {
                for (int vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                                  //перебор типов налоговой ставки
                {
                    for (int itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)              //перебор типов оплаты товара
                    {
                        for (int itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)                                               //перебор типов добавления товара
                        {
                            for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                           //перебор видов платежей
                            {
                                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, (ReceiptKind)receiptKind);
                                for (int item = 0; item < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countcosts); item++)         //перебор комбинаций стоиости и количества
                                {
                                    TestingInterfaceFW16.AddEntry(document,
                                        (ReceiptKind)receiptKind,
                                        "Item " + vatCode + "" + itemBy + "" + itemPaymentKind + "" + item,
                                        counts[item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                                        (Native.CmdExecutor.VatCodeType)vatCode,
                                        (TestingInterfaceFW16.ItemBy)itemBy,
                                        costs[item % TestingInterfaceFW16.countcosts],
                                        (ItemPaymentKind)itemPaymentKind);  //создание товара
                                }
                                TestingInterfaceFW16.AddPayment(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)tenderCode, document.Total + ((Native.CmdExecutor.TenderCode)tenderCode == Native.CmdExecutor.TenderCode.Cash ? (random.Next(-1 * (int)document.Total * (10 / 100), (int)document.Total * (10 / 100))) : 0));

                                Console.Write($"({i++}/{countReciepts}) ");

                                if (TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort) != 0)
                                {
                                    err += $"|{(ReceiptKind)receiptKind,12}|{(Native.CmdExecutor.VatCodeType)vatCode,17}|{(ItemPaymentKind)itemPaymentKind,17}|{(TestingInterfaceFW16.ItemBy)itemBy,8}|{(Native.CmdExecutor.TenderCode)tenderCode,15}|{(Native.CmdExecutor.TenderType)TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode],15}|\n";
                                    TestingInterfaceFW16.GetRegisters(TestingInterfaceFW16.RegistersСumulative);
                                }
                            }

                        }
                    }
                }
            }
            return err;
        }
    }
}
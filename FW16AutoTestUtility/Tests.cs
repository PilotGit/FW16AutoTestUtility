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
        TestingInterfaceFW16 testingInterfaceFW16 = null;
        public EcrCtrl ecrCtrl;                                     //подключение к ККТ
        string nameOperator = "test program";                        //имя касира 
        decimal[] costs = new decimal[] { 217m, 193.7m };          //варианты цен
        decimal[] counts = new decimal[] { 1m, 5m, 0.17m, 1.73m };  //варианты колличества
        Random random = new Random();
        List<TestDataReceipt> testDataReceiptList = new List<TestDataReceipt>();
        List<TestDataCorrection> testDataCorrectionList = new List<TestDataCorrection>();
        List<TestDataNFDoc> testDataNFDocList = new List<TestDataNFDoc>();

        public Tests()
        {
            testingInterfaceFW16 = new TestingInterfaceFW16(out ecrCtrl);
            if (ecrCtrl.Fw16 != null)
            {
                BeginTest();
            }
            else
            {
                Console.WriteLine("Не удалось провести тестирование");
            }
        }

        public Tests(int serialPort, int baudRate = 57600)
        {
            testingInterfaceFW16 = new TestingInterfaceFW16(out ecrCtrl, serialPort, baudRate);
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
                testingInterfaceFW16.CloseShift(nameOperator);                                                                   //закрыть смену если открыта
            }
        }

        public void SimpleTest()                            //функция прогона по всем видам чеков и чеков коррекции
        {
            testingInterfaceFW16.OpenShift(nameOperator);   //открытие смены для этого теста
            testingInterfaceFW16.GetRegisters();
            testingInterfaceFW16.GetCounters();
            string errRegisters = "";
            errRegisters += TestReceiptMax();
            errRegisters += TestCorrectionMax();
            errRegisters += TestNonFiscalMax();
            CreateMinReceiptTest(errRegisters);
            TestReceiptDataCollection();
            TestCorrectionDataCollection();
            TestNonFiscalDataCollection();
            /*
            if (TestReceiptMax().Length != 0)                          //вызов функции тестирования чека
            {
                Console.WriteLine($"+------------+-----------------+-----------------+--------+---------------+---------------+\n" +
                    $"|{"Тип чека",12}|{"Процентная ставка",17}|{"Тип оплаты",17}|{"Товар по",8}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
                    $"+------------+-----------------+-----------------+--------+---------------+---------------+\n" + TestReceiptMin());
            }
            if (TestCorrectionMax().Length != 0)                       //вызов функции тестирования чека коррекции
            {
                Console.WriteLine($"+------------+-----------------+---------------+---------------+\n" +
                    $"|{"Тип чека",12}|{"Процентная ставка",17}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
                    $"+------------+-----------------+---------------+---------------+\n" + TestCorrectionMin());
            }
            if (TestNonFiscalMax().Length != 0)                        //вызов функции нефискального документа
            {
                Console.WriteLine($"+------------+---------------+---------------+\n" +
                    $"|{"Тип чека",12}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
                    $"+------------+---------------+---------------+\n" + TestNonFiscalMin());
            }
            */
            TestReceiptMax(true);                               //вызов функции тестирования чека c отменой.
            TestNonFiscalMax(true);                             //вызов функции нефискального документа с отменой
                                                                //закрытие смены этого теста
            testingInterfaceFW16.CloseShift(nameOperator);      //Закрытие смены для этого теста

            testingInterfaceFW16.RequestRegisters();
            testingInterfaceFW16.RequestCounters();

            Console.WriteLine("Завершено тестирование SimpleTest ");     //логирование

            //TestCorrection(true);                         //вызов функции тестирования чека коррекции с отменой
            //отключено в связи с тем что чек коррекции не возможно отменить, потому что он отправляется одним пакетом
        }

        /// <summary>
        /// Тест нефискального документа
        /// </summary>
        /// <param name="abort">Отменить создание нефискального документа</param>
        private string TestNonFiscalMax(bool abort = false)
        {
            string ret = "";
            int countNFDoc = TestingInterfaceFW16.countNFDocType;
            int i = 1;
            for (int nfDocType = 1; nfDocType <= TestingInterfaceFW16.countNFDocType; nfDocType++)                                           //Перебор типов нефиксальных документов
            {
                testingInterfaceFW16.StartDocument(out Fw16.Ecr.NonFiscalBase document, TestingInterfaceFW16.nfDocType[nfDocType]);
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode && nfDocType != 3; tenderCode++)
                {
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                         //
                    {
                        testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.nfDocType[nfDocType], (Native.CmdExecutor.TenderCode)tenderCode, costs[cost]);
                    }
                }
                document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                Console.Write($"({i++}/{countNFDoc}) ");

                ret += testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.nfDocType[nfDocType], abort);
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
                    testingInterfaceFW16.StartDocument(out Fw16.Ecr.NonFiscalBase document, TestingInterfaceFW16.nfDocType[nfDocType]);
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)
                    {
                        testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.nfDocType[nfDocType], (Native.CmdExecutor.TenderCode)(tenderCode), costs[cost]);
                    }

                    document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                    Console.Write($"({i++}/{countNFDoc}) ");

                    if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.nfDocType[nfDocType], abort).Length != 0)
                    {
                        err += $"|{TestingInterfaceFW16.nfDocType[nfDocType],12}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
                        testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Тест нефискального документа
        /// </summary>
        /// <param name="abort">Отменить создание нефискального документа</param>
        private string TestNonFiscalDataCollection(bool abort = false)
        {
            string err = null;
            int i = 1;

            foreach (var testData in testDataNFDocList)
            {
                testingInterfaceFW16.StartDocument(out Fw16.Ecr.NonFiscalBase document, TestingInterfaceFW16.nfDocType[testData.nfDocType]);
                for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)
                {
                    testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.nfDocType[testData.nfDocType], (Native.CmdExecutor.TenderCode)(testData.tenderCode), costs[cost]);
                }

                document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                Console.Write($"({i++}/{testDataNFDocList.Count}) ");

                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.nfDocType[testData.nfDocType], abort).Length != 0)
                {
                    err += testData.ToString();
                    testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                }

            }
            return err;
        }

        /// <summary>
        /// Тест чека коррекции
        /// </summary>
        /// <param name="abort">Отменить создание чека коррекции</param>
        private string TestCorrectionMax(bool abort = false)
        {
            string ret = "";
            int i = 1;
            int countCorrections = TestingInterfaceFW16.countReceiptKind;

            for (int receiptKind = 1; receiptKind < 4; receiptKind += 2)
            {
                testingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, TestingInterfaceFW16.receiptKind[receiptKind]);
                decimal sum = 0;
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                   //перебор видов платежей
                {
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                  //перебор сумм
                    {
                        testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, costs[cost]);
                        sum += costs[cost];
                    }
                }
                decimal sumPaid = 0m;
                for (ushort vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                             //перебор налоговых ставок
                {
                    sumPaid = Math.Round(sum / ((TestingInterfaceFW16.countVatCode + 1) - vatCode), 2);
                    testingInterfaceFW16.AddAmount(document, TestingInterfaceFW16.receiptKind[receiptKind], TestingInterfaceFW16.vatCodeCorr[vatCode], sumPaid);
                    sum = sum - sumPaid;
                }

                Console.Write($"({i++}/{countCorrections}) ");

                ret += testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[receiptKind], abort);
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
                        testingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, TestingInterfaceFW16.receiptKind[receiptKind]);
                        decimal sum = 0;
                        for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                  //перебор сумм
                        {
                            testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, costs[cost]);
                            sum += costs[cost];
                        }
                        testingInterfaceFW16.AddAmount(document, TestingInterfaceFW16.receiptKind[receiptKind], TestingInterfaceFW16.vatCodeCorr[vatCode], sum);

                        Console.Write($"({i++}/{countCorrections}) ");

                        if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[receiptKind], abort).Length != 0)
                        {
                            err += $"|{TestingInterfaceFW16.receiptKind[receiptKind],12}|{TestingInterfaceFW16.vatCode[vatCode],17}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
                            testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                        }
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Тест чека коррекции
        /// </summary>
        /// <param name="abort">Отменить создание чека коррекции</param>
        private string TestCorrectionDataCollection(bool abort = false)
        {
            string err = null;
            int i = 1;

            foreach (var testData in testDataCorrectionList)
            {
                testingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, TestingInterfaceFW16.receiptKind[testData.receiptKind]);
                decimal sum = 0;
                for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                  //перебор сумм
                {
                    testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], (Native.CmdExecutor.TenderCode)testData.tenderCode, costs[cost]);
                    sum += costs[cost];
                }
                testingInterfaceFW16.AddAmount(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], TestingInterfaceFW16.vatCodeCorr[testData.vatCode], sum);

                Console.Write($"({i++}/{testDataCorrectionList.Count}) ");

                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], abort).Length != 0)
                {
                    err += testData.ToString();
                    testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                }
            }
            return err;
        }

        /// <summary>
        /// Тестирование чека с перебором большого количества товаров
        /// </summary>
        /// <param name="abort">Отменить создание чека</param>
        private string TestReceiptMax(bool abort = false)
        {
            string ret = "";
            int countReciepts = TestingInterfaceFW16.countReceiptKind * TestingInterfaceFW16.countItemBy;
            int i = 1;

            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)                              //перебор типов чеков
            {
                for (int itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)                                               //перебор типов добавления товара
                {
                    testingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, TestingInterfaceFW16.receiptKind[receiptKind]);
                    for (int vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                      //перебор типов налоговой ставки
                    {
                        for (int itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)   //перебор типов оплаты товара
                        {
                            for (int item = 0; item < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countcosts); item++)              //перебор комбинаций стоиости и количества
                            {
                                testingInterfaceFW16.AddEntry(document,
                                    TestingInterfaceFW16.receiptKind[receiptKind],
                                    "Item " + vatCode + "" + itemBy + "" + itemPaymentKind + "" + item,
                                    counts[item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                                    TestingInterfaceFW16.vatCode[vatCode],
                                    (TestingInterfaceFW16.ItemBy)itemBy,
                                    costs[item % TestingInterfaceFW16.countcosts],
                                    TestingInterfaceFW16.itemPaymentKind[itemPaymentKind]);  //создание товара
                            }
                        }
                    }

                    decimal sum = 0m;
                    for (int tenderCode = 1; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                           //перебор видов платежей
                    {
                        sum = Math.Round(document.Total / 9 - tenderCode, 2);
                        //sum += (decimal)(random.Next((int)(-1 * sum * (10m / 100m)), (int)(sum * (10m / 100m))));
                        testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, sum);
                        sum = document.Total - document.TotalaPaid;
                    }

                    testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], Native.CmdExecutor.TenderCode.Cash, sum + (random.Next(0, (int)(sum * (10m / 100m)))));       //оплата наличными
                    Console.Write($"({i++}/{countReciepts}) ");
                    ret += testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[receiptKind], abort);
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
                                testingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, TestingInterfaceFW16.receiptKind[receiptKind]);
                                for (int item = 0; item < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countcosts); item++)         //перебор комбинаций стоиости и количества
                                {
                                    testingInterfaceFW16.AddEntry(document,
                                        TestingInterfaceFW16.receiptKind[receiptKind],
                                        "Item " + vatCode + "" + itemBy + "" + itemPaymentKind + "" + item,
                                        counts[item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                                        TestingInterfaceFW16.vatCode[vatCode],
                                        (TestingInterfaceFW16.ItemBy)itemBy,
                                        costs[item % TestingInterfaceFW16.countcosts],
                                        TestingInterfaceFW16.itemPaymentKind[itemPaymentKind]);  //создание товара
                                }
                                testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, document.Total + ((Native.CmdExecutor.TenderCode)tenderCode == Native.CmdExecutor.TenderCode.Cash ? (random.Next(0, (int)(document.Total * (10m / 100m)))) : 0));

                                Console.Write($"({i++}/{countReciepts}) ");

                                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[receiptKind], abort).Length != 0)
                                {
                                    err += $"|{TestingInterfaceFW16.receiptKind[receiptKind],12}|{TestingInterfaceFW16.vatCode[vatCode],17}|{TestingInterfaceFW16.itemPaymentKind[itemPaymentKind],17}|{(TestingInterfaceFW16.ItemBy)itemBy,8}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
                                    testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                                }
                            }

                        }
                    }
                }
            }
            return err;
        }

        private string TestReceiptDataCollection()
        {
            string err = null;
            int i = 1;
            foreach (var testData in testDataReceiptList)
            {
                testingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, TestingInterfaceFW16.receiptKind[testData.receiptKind]);
                for (int item = 0; item < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countcosts); item++)         //перебор комбинаций стоиости и количества
                {
                    testingInterfaceFW16.AddEntry(document,
                        TestingInterfaceFW16.receiptKind[testData.receiptKind],
                        "Item " + testData.vatCode + "" + testData.itemBy + "" + testData.itemPaymentKind + "" + item,
                        counts[item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                        TestingInterfaceFW16.vatCode[testData.vatCode],
                        (TestingInterfaceFW16.ItemBy)testData.itemBy,
                        costs[item % TestingInterfaceFW16.countcosts],
                        TestingInterfaceFW16.itemPaymentKind[testData.itemPaymentKind]);  //создание товара
                }
                testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], (Native.CmdExecutor.TenderCode)testData.tenderCode, document.Total + ((Native.CmdExecutor.TenderCode)testData.tenderCode == Native.CmdExecutor.TenderCode.Cash ? (random.Next(0, (int)(document.Total * (10m / 100m)))) : 0));

                Console.Write($"({i++}/{testDataReceiptList.Count}) ");

                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], false).Length != 0)
                {
                    err += testData.ToString();
                    testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                }
            }
            return err;
        }

        public void CreateMinReceiptTest(string registers)
        {
            List<string> regList = new List<string>(registers.Split(','));
            List<TestDataReceipt> listReceiptTmp = new List<TestDataReceipt>();
            List<TestDataCorrection> listCorrectionTmp = new List<TestDataCorrection>();
            List<TestDataNFDoc> listNFDocTmp = new List<TestDataNFDoc>();
            regList.Remove("");
            for (int i = 0; i < regList.Count; i++)
            {
                if (regList.IndexOf(regList[i]) != i) { regList.RemoveAt(i); i--; }
            }
            foreach (var item in regList)
            {
                int receiptKind;
                int nfDocType;
                int vatCode;
                int itemPaymentKind;
                int itemBy;
                int tenderCode;
                int numberRegister = Int32.Parse(item);
                if (numberRegister > 0 && numberRegister < 5)                                                                                                                                                           //Создаёт тестовые данные для проврки ошибки в 1-4 регистрах
                {
                    receiptKind = numberRegister;
                    for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                    {
                        for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                        {
                            for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                            {
                                for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                }
                            }
                        }
                    }
                    continue;
                }
                if (numberRegister > 10 && numberRegister < 19 || numberRegister > 20 && numberRegister < 29 || numberRegister > 30 && numberRegister < 39 || numberRegister > 40 && numberRegister < 49)               //Создаёт тестовые данные для проврки ошибки в 11-18, 21-28, 31-38, 41-48 регистрах
                {
                    receiptKind = numberRegister / 10;
                    tenderCode = numberRegister % 10;
                    for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                    {
                        for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                        {
                            for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                            }
                        }
                    }
                    continue;
                }
                if (numberRegister == 19 || numberRegister == 29 || numberRegister == 39 || numberRegister == 49)                                                                                                       //Создаёт тестовые данные для проврки ошибки в 19,29,39,49 регистрах
                {
                    receiptKind = numberRegister / 10;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash))
                        {
                            for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                            {
                                for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                                {
                                    for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                                    {
                                        listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }
                if (119 < numberRegister && numberRegister < 126 || 129 < numberRegister && numberRegister < 136 || 139 < numberRegister && numberRegister < 146 || 149 < numberRegister && numberRegister < 156)       //Создаёт тестовые данные для проврки ошибки в 120-125, 130-135, 140-145, 150-155 регистрах
                {
                    receiptKind = (numberRegister - 110) / 10;
                    vatCode = numberRegister % 10 + 1;
                    for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                    {
                        for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                        {
                            for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                            }
                        }
                    }
                    continue;
                }
                if (125 < numberRegister && numberRegister < 130 || 135 < numberRegister && numberRegister < 140 || 145 < numberRegister && numberRegister < 150 || 155 < numberRegister && numberRegister < 160)       //Создаёт тестовые данные для проврки ошибки в 126-129, 136-139, 146-149, 156-159 регистрах
                {
                    receiptKind = (numberRegister - 110) / 10;
                    switch (numberRegister % 10)
                    {
                        case 6: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat18; break;
                        case 7: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat10; break;
                        case 8: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat18Included; break;
                        case 9: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat10Included; break;
                        default:
                            vatCode = 1;
                            break;
                    }
                    for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                    {
                        for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                        {
                            for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                            }
                        }
                    }
                    continue;
                }
                if (numberRegister == 160 || numberRegister == 171)                                                                                                                                                     //Создаёт тестовые данные для проврки ошибки в 160,171 регистрах
                {
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                            {
                                for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                                {
                                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                    {
                                        listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }
                if (160 < numberRegister && numberRegister < 167)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 161-165 регистрах
                {
                    vatCode = numberRegister % 10;
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                        {
                            for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                            {
                                for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                }
                            }
                        }
                    }
                    continue;
                }
                if (166 < numberRegister && numberRegister < 171)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 167-170 регистрах
                {
                    receiptKind = (numberRegister - 110) / 10;
                    switch (numberRegister % 10)
                    {
                        case 7: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat18; break;
                        case 8: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat10; break;
                        case 9: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat18Included; break;
                        case 0: vatCode = (int)Native.CmdExecutor.VatCodeType.Vat10Included; break;
                        default:
                            vatCode = 1;
                            break;
                    }
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                        {
                            for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                            {
                                for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                }
                            }
                        }
                    }
                    continue;
                }
                if (171 < numberRegister && numberRegister < 180)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 172-179 регистрах
                {
                    tenderCode = numberRegister - 172;
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                            {
                                for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                }
                            }
                        }
                    }
                    continue;
                }
                if (180 == numberRegister || numberRegister == 181)                                                                                                                                                     //Создаёт тестовые данные для проврки ошибки в 180, 181 регистрах
                {
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                            {
                                for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                                {
                                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                    {
                                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.Cash) && numberRegister == 180) listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash) && numberRegister == 181) listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }
                /*182-189 пропущены*/
                /*191-194 не участвуют в минимальных тестах*/
                if (199 < numberRegister && numberRegister < 207 || 209 < numberRegister && numberRegister < 217 || 219 < numberRegister && numberRegister < 227 || 229 < numberRegister && numberRegister < 237)       //Создаёт тестовые данные для проврки ошибки в 200-206, 210-216, 220-226, 230-236 регистрах
                {
                    receiptKind = (numberRegister - 190) / 10;
                    itemPaymentKind = numberRegister % 10;
                    for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                    {
                        for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                        {
                            for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                            }
                        }
                    }
                    continue;
                }
                /*------------------------------------------------------------------------------------------*/
                if (numberRegister == 5 || numberRegister == 7)                                                                                                                                                         //Создаёт тестовые данные для проврки ошибки в 5, 7 регистрах
                {
                    receiptKind = numberRegister - 4;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));
                        }
                    }
                    continue;
                }
                if (50 < numberRegister && numberRegister < 56 || 70 < numberRegister && numberRegister < 76)                                                                                                           //Создаёт тестовые данные для проврки ошибки в 51-55, 71-75 регистрах
                {
                    receiptKind = (numberRegister - 40) / 10;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == numberRegister % 10) { listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode)); }
                        }
                    }
                    continue;
                }

                if (59 < numberRegister && numberRegister < 66 || 79 < numberRegister && numberRegister < 86)                                                                                                           //Создаёт тестовые данные для проврки ошибки в 60-65, 80-85 регистрах
                {
                    receiptKind = (numberRegister - 50) / 10;
                    vatCode = numberRegister % 10 + 1;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));
                    }
                    continue;
                }
                if (65 < numberRegister && numberRegister < 70 || 85 < numberRegister && numberRegister < 90)                                                                                                           //Создаёт тестовые данные для проврки ошибки в 66-69, 86-89 регистрах
                {
                    receiptKind = (numberRegister - 50) / 10;
                    switch (numberRegister % 10)
                    {
                        case 6: vatCode = (int)VatCode.Vat18; break;
                        case 7: vatCode = (int)VatCode.Vat10; break;
                        case 8: vatCode = (int)VatCode.Vat18Included; break;
                        case 9: vatCode = (int)VatCode.Vat10Included; break;
                        default:
                            vatCode = 1;
                            break;
                    }
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));
                    }
                    continue;
                }
                /*------------------------------------------------------------------------------------------*/
                if (numberRegister == 9 || numberRegister == 10)                                                                                                                                                        //Создаёт тестовые данные для проврки ошибки в 9, 10 регистрах                
                {
                    nfDocType = numberRegister - 8;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode && nfDocType != 3; tenderCode++)
                    {
                        listNFDocTmp.Add(new TestDataNFDoc(nfDocType, tenderCode));
                    }

                    continue;
                }
                if (90 < numberRegister && numberRegister < 99 || 100 < numberRegister && numberRegister < 109)                                                                                                         //Создаёт тестовые данные для проврки ошибки в 91-98, 101-108 регистрах
                {
                    nfDocType = (numberRegister - 80) / 10;
                    tenderCode = numberRegister % 10 - 1;
                    continue;
                }
                if (numberRegister == 99 || numberRegister == 109)                                                                                                                                                      //Создаёт тестовые данные для проврки ошибки в 99, 109 регистрах
                {
                    nfDocType = (numberRegister - 80) / 10;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode && nfDocType != 3; tenderCode++)
                    {
                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash))
                            listNFDocTmp.Add(new TestDataNFDoc(nfDocType, tenderCode)); ;
                    }
                    continue;
                }
                /*----------------------------------------------------------------------------------------------------*/
                if (110 < numberRegister && numberRegister < 119)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 111-118 регистрах
                {
                    tenderCode = numberRegister % 10 - 1;
                    for (nfDocType = 1; nfDocType < TestingInterfaceFW16.countNFDocType; nfDocType++)
                    {
                        listNFDocTmp.Add(new TestDataNFDoc(nfDocType, tenderCode));
                    }
                    for (receiptKind = 1; receiptKind < TestingInterfaceFW16.countReceiptKind; receiptKind += 2)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));
                        }
                    }
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                            {
                                for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                }
                            }
                        }
                    }
                    continue;
                }
                if (numberRegister == 119)                                                                                                                                                                              //Создаёт тестовые данные для проврки ошибки в 119 регистрах
                {
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash))
                        {
                            for (nfDocType = 1; nfDocType < TestingInterfaceFW16.countNFDocType; nfDocType++)
                            {
                                listNFDocTmp.Add(new TestDataNFDoc(nfDocType, tenderCode));
                            }
                            for (receiptKind = 1; receiptKind < TestingInterfaceFW16.countReceiptKind; receiptKind += 2)
                            {
                                for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                                {
                                    listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));
                                }
                            }
                            for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                            {
                                for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                                {
                                    for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                                    {
                                        for (itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                                        {
                                            listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, itemBy, tenderCode));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }
            }
            foreach (var testData in listReceiptTmp)
            {
                if (!this.testDataReceiptList.Contains(testData)) { this.testDataReceiptList.Add(testData); }
            }
            foreach (var testData in listCorrectionTmp)
            {
                if (!testDataCorrectionList.Contains(testData)) { testDataCorrectionList.Add(testData); }
            }
            foreach (var testData in listNFDocTmp)
            {
                if (!testDataNFDocList.Contains(testData)) { testDataNFDocList.Add(testData); }
            }
        }
    }


    class TestDataReceipt
    {
        public int receiptKind;
        public int vatCode;
        public int itemPaymentKind;
        public int itemBy;
        public int tenderCode;

        public override string ToString()
        {
            return $"|{TestingInterfaceFW16.receiptKind[receiptKind],12}|{TestingInterfaceFW16.vatCode[vatCode],17}|{TestingInterfaceFW16.itemPaymentKind[itemPaymentKind],17}|{(TestingInterfaceFW16.ItemBy)itemBy,8}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
        }

        public TestDataReceipt(int receiptKind, int vatCode, int itemPaymentKind, int itemBy, int tenderCode)
        {
            this.receiptKind = receiptKind;
            this.vatCode = vatCode;
            this.itemPaymentKind = itemPaymentKind;
            this.itemBy = itemBy;
            this.tenderCode = tenderCode;
        }

        public override bool Equals(object obj)
        {
            var date = obj as TestDataReceipt;
            return date != null &&
                   receiptKind == date.receiptKind &&
                   vatCode == date.vatCode &&
                   itemPaymentKind == date.itemPaymentKind &&
                   itemBy == date.itemBy &&
                   tenderCode == date.tenderCode;
        }

        public override int GetHashCode()
        {
            var hashCode = 257427813;
            hashCode = hashCode * -1521134295 + receiptKind.GetHashCode();
            hashCode = hashCode * -1521134295 + vatCode.GetHashCode();
            hashCode = hashCode * -1521134295 + itemPaymentKind.GetHashCode();
            hashCode = hashCode * -1521134295 + itemBy.GetHashCode();
            hashCode = hashCode * -1521134295 + tenderCode.GetHashCode();
            return hashCode;
        }
    }

    class TestDataCorrection
    {
        public int receiptKind;
        public int vatCode;
        public int tenderCode;

        public override string ToString()
        {
            return $"|{TestingInterfaceFW16.receiptKind[receiptKind],12}|{TestingInterfaceFW16.vatCode[vatCode],17}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
        }

        public TestDataCorrection(int receiptKind, int vatCode, int tenderCode)
        {
            this.receiptKind = receiptKind;
            this.vatCode = vatCode;
            this.tenderCode = tenderCode;
        }

        public override bool Equals(object obj)
        {
            var correction = obj as TestDataCorrection;
            return correction != null &&
                   receiptKind == correction.receiptKind &&
                   vatCode == correction.vatCode &&
                   tenderCode == correction.tenderCode;
        }

        public override int GetHashCode()
        {
            var hashCode = 205760920;
            hashCode = hashCode * -1521134295 + receiptKind.GetHashCode();
            hashCode = hashCode * -1521134295 + vatCode.GetHashCode();
            hashCode = hashCode * -1521134295 + tenderCode.GetHashCode();
            return hashCode;
        }
    }

    class TestDataNFDoc
    {
        public int nfDocType;
        public int tenderCode;

        public override string ToString()
        {
            return $"|{TestingInterfaceFW16.nfDocType[nfDocType],12}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
        }

        public TestDataNFDoc(int nfDocType, int tenderCode)
        {
            this.nfDocType = nfDocType;
            this.tenderCode = tenderCode;
        }

        public override bool Equals(object obj)
        {
            var doc = obj as TestDataNFDoc;
            return doc != null &&
                   nfDocType == doc.nfDocType &&
                   tenderCode == doc.tenderCode;
        }

        public override int GetHashCode()
        {
            var hashCode = 261668315;
            hashCode = hashCode * -1521134295 + nfDocType.GetHashCode();
            hashCode = hashCode * -1521134295 + tenderCode.GetHashCode();
            return hashCode;
        }
    }
}
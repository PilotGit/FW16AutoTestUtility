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
        /// <summary>
        /// Интерфейс взаимодействия с длл, отслеживающий изменение в регистрах
        /// </summary>
        TestingInterfaceFW16 testingInterfaceFW16 = null;
        public EcrCtrl ecrCtrl;                                     //подключение к ККТ
        /// <summary>
        /// Имя кассира
        /// </summary>
        string nameOperator = "test program";                        //имя касира 
        /// <summary>
        /// Варианты сумм
        /// </summary>
        decimal[,] costs = new decimal[,] { { 217m, 193.7m }, { 30m, 18.36m }, { 147m, 5.63m }, { 961m, 101.25m } };
        /// <summary>
        /// Варианты количеств
        /// </summary>
        decimal[,] counts = new decimal[4, 4] { { 1m, 5m, 0.17m, 1.73m }, { 7m, 3m, 0.44m, 2.89m }, { 10m, 4m, 0.38m, 9.37m }, { 8m, 2m, 0.55m, 5.22m } };
        Random random = new Random();   //рандом
        /// <summary>
        /// Список тестовых данных для чека
        /// </summary>
        List<TestDataReceipt> testDataReceiptList = new List<TestDataReceipt>();
        /// <summary>
        /// Список тестовых данных для чека коррекции
        /// </summary>
        List<TestDataCorrection> testDataCorrectionList = new List<TestDataCorrection>();
        /// <summary>
        /// Список тестовых данных для нефискального документа
        /// </summary>
        List<TestDataNFDoc> testDataNFDocList = new List<TestDataNFDoc>();

        /// <summary>
        /// Создаёт объект класса с значением параметров по умолчанию
        /// </summary>
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

        /// <summary>
        /// Создаёт объект класса исходя из переданных параметров
        /// </summary>
        /// <param name="serialPort">Число, означающее порт</param>
        /// <param name="baudRate">Число, частота</param>
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

        /// <summary>
        /// Тестирует все виды документов, в случае обнаружения ошибок тестирует на минимальных данных
        /// </summary>
        public void SimpleTest()
        {
            testingInterfaceFW16.OpenShift(nameOperator);   //открытие смены для этого теста
            testingInterfaceFW16.GetRegisters();
            testingInterfaceFW16.GetCounters();

            CreateReceiptDataCollection(TestReceiptBigData());
            CreateCorrectionDataCollection(TestCorrectionBigData());
            CreateNFDocDataCollection(TestNonFiscalBigData());

            TestReceiptPayCredit();

            TestReceiptDataCollection();
            TestCorrectionDataCollection();
            TestNonFiscalDataCollection();

            TestReceiptBigData(true);                               //вызов функции тестирования чека c отменой.
            TestNonFiscalBigData(true);                             //вызов функции нефискального документа с отменой

            testingInterfaceFW16.CloseShift(nameOperator);      //Закрытие смены для этого теста

            testingInterfaceFW16.RequestRegisters(testingInterfaceFW16.ControlRegisters);
            testingInterfaceFW16.RequestCounters();

            Console.WriteLine("Завершено тестирование SimpleTest ");     //логирование

            //TestCorrection(true);                         //вызов функции тестирования чека коррекции с отменой
            //отключено в связи с тем что чек коррекции не возможно отменить, потому что он отправляется одним пакетом
        }

        /// <summary>
        /// Тестирует нефискальный документ на большом количестве данных и с перебором всех вариантов.
        /// </summary>
        /// <param name="abort">Отменить создание нефискального документа</param>
        private string TestNonFiscalBigData(bool abort = false)
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
                        testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.nfDocType[nfDocType], (Native.CmdExecutor.TenderCode)tenderCode, costs[nfDocType - 1, cost]);
                    }
                }
                document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                Console.Write($"({i++}/{countNFDoc}) ");

                ret += testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.nfDocType[nfDocType], abort);
            }
            return ret;
        }

        /// <summary>
        /// Тестирует нефискальный документ с использованием заранее сформирванных тестовых данных
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
                    testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.nfDocType[testData.nfDocType], (Native.CmdExecutor.TenderCode)(testData.tenderCode), costs[testData.nfDocType - 1, cost]);
                }

                document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                Console.Write($"({i++}/{testDataNFDocList.Count}) {testData.ToString()}");

                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.nfDocType[testData.nfDocType], abort).Length != 0)
                {
                    err += testData.ToString();
                }
            }
            return err;
        }

        /// <summary>
        /// Тестирует чек коррекции на большом количестве данных и с перебором всех вариантов.
        /// </summary>
        /// <param name="abort">Отменить создание чека коррекции</param>
        private string TestCorrectionBigData(bool abort = false)
        {
            string ret = "";
            int i = 1;
            int countCorrections = TestingInterfaceFW16.countReceiptKind / 2;

            for (int receiptKind = 1; receiptKind < 4; receiptKind += 2)
            {
                testingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, TestingInterfaceFW16.receiptKind[receiptKind]);
                decimal sum = 0;
                for (int tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                   //перебор видов платежей
                {
                    for (int cost = 0; cost < TestingInterfaceFW16.countcosts; cost++)                                  //перебор сумм
                    {
                        testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, costs[receiptKind - 1, cost]);
                        sum += costs[receiptKind - 1, cost];
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
        /// Тестирует чек коррекции с использованием заранее сформирванных тестовых данных
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
                    testingInterfaceFW16.AddTender(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], (Native.CmdExecutor.TenderCode)testData.tenderCode, costs[testData.receiptKind - 1, cost]);
                    sum += costs[testData.receiptKind - 1, cost];
                }
                testingInterfaceFW16.AddAmount(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], TestingInterfaceFW16.vatCodeCorr[testData.vatCode], sum);

                Console.Write($"({i++}/{testDataCorrectionList.Count}) {testData.ToString()}");

                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], abort).Length != 0)
                {
                    err += testData.ToString();
                    //testingInterfaceFW16.GetRegisters(testingInterfaceFW16.RegistersСumulative);
                }
            }
            return err;
        }

        /// <summary>
        /// Тестирует чек на большом количестве данных и с перебором всех вариантов.
        /// </summary>
        /// <param name="abort">Отменить создание чека</param>
        private string TestReceiptBigData(bool abort = false)
        {
            string ret = "";
            int countReciepts = TestingInterfaceFW16.countReceiptKind * TestingInterfaceFW16.countAdjustment;
            int i = 1;
            int itemBy = 0;
            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)                              //перебор типов чеков
            {
                for (int adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)                                               //перебор типов добавления товара
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
                                    "Item " + vatCode + "" + adjustment + "" + itemPaymentKind + "" + item,
                                    counts[receiptKind - 1, item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                                    TestingInterfaceFW16.vatCode[vatCode],
                                    (TestingInterfaceFW16.ItemBy)itemBy,
                                    costs[receiptKind - 1, item % TestingInterfaceFW16.countcosts],
                                    TestingInterfaceFW16.itemPaymentKind[itemPaymentKind]);  //создание товара
                            }
                        }
                    }
                    decimal sumCorr = adjustment == 0 ? -1 * ((testingInterfaceFW16.RegistersTmp[160] * 100) % 100) / 100m : 0.99m - ((testingInterfaceFW16.RegistersTmp[160] * 100) % 100) / 100m;
                    testingInterfaceFW16.SetAdjustment(document, TestingInterfaceFW16.receiptKind[receiptKind], sumCorr);
                    decimal sum = 0m;
                    decimal totalaPaid = 0;
                    for (int tenderCode = 1; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                           //перебор видов платежей
                    {
                        totalaPaid += sum = Math.Round(testingInterfaceFW16.RegistersTmp[160] / 9 - tenderCode, 2);
                        testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, sum);
                    }

                    sum = testingInterfaceFW16.RegistersTmp[160] - totalaPaid;
                    testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], Native.CmdExecutor.TenderCode.Cash, sum + (random.Next(0, (int)(sum * (10m / 100m)))));       //оплата наличными
                    Console.Write($"({i++}/{countReciepts}) ");
                    ret += testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[receiptKind], abort);
                }
            }
            return ret;
        }

        /// <summary>
        /// Тестирует чек с использованием заранее сформирванных тестовых данных
        /// </summary>
        /// <returns></returns>
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
                        counts[testData.receiptKind - 1, item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                        TestingInterfaceFW16.vatCode[testData.vatCode],
                        (TestingInterfaceFW16.ItemBy)testData.itemBy,
                        costs[testData.receiptKind - 1, item % TestingInterfaceFW16.countcosts],
                        TestingInterfaceFW16.itemPaymentKind[testData.itemPaymentKind]);  //создание товара
                }

                decimal sumCorr = testData.adjustment == 0 ? -1 * ((testingInterfaceFW16.RegistersTmp[160] * 100) % 100) / 100m : 0.99m - ((testingInterfaceFW16.RegistersTmp[160] * 100) % 100) / 100m;
                testingInterfaceFW16.SetAdjustment(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], sumCorr);

                testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], (Native.CmdExecutor.TenderCode)testData.tenderCode, testingInterfaceFW16.RegistersTmp[160] + ((Native.CmdExecutor.TenderCode)testData.tenderCode == Native.CmdExecutor.TenderCode.Cash ? (random.Next(0, (int)(testingInterfaceFW16.RegistersTmp[160] * (10m / 100m)))) : 0));

                Console.Write($"({i++}/{testDataReceiptList.Count}) {testData.ToString()}");
                if (testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[testData.receiptKind], false).Length != 0)
                {
                    err += testData.ToString();
                }
            }
            return err;
        }

        /// <summary>
        /// Тестирование чека с товаром с признаком оплаты  -  оплата кредита
        /// </summary>
        /// <param name="abort"></param>
        /// <returns></returns>
        private string TestReceiptPayCredit(bool abort = false)
        {
            string ret = "";
            int countReciepts = TestingInterfaceFW16.countReceiptKind * TestingInterfaceFW16.countAdjustment * TestingInterfaceFW16.countVatCode;
            int i = 1;
            int itemBy = 0;
            int itemPaymentKind = 7;   //перебор типов оплаты товара
            int item = 7;
            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)                              //перебор типов чеков
            {
                for (int adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)                                               //перебор типов добавления товара
                {
                    for (int vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)                                      //перебор типов налоговой ставки
                    {

                        testingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, TestingInterfaceFW16.receiptKind[receiptKind]);
                        testingInterfaceFW16.AddEntry(document,
                            TestingInterfaceFW16.receiptKind[receiptKind],
                            "Item " + vatCode + "" + adjustment + "" + itemPaymentKind + "" + item,
                            counts[receiptKind - 1, item / TestingInterfaceFW16.countcosts % TestingInterfaceFW16.countCounts],
                            TestingInterfaceFW16.vatCode[vatCode],
                            (TestingInterfaceFW16.ItemBy)itemBy,
                            costs[receiptKind - 1, item % TestingInterfaceFW16.countcosts],
                            TestingInterfaceFW16.itemPaymentKind[itemPaymentKind]);  //создание товара

                        decimal sumCorr = adjustment == 0 ? -1 * ((testingInterfaceFW16.RegistersTmp[160] * 100) % 100) / 100m : 0.99m - ((testingInterfaceFW16.RegistersTmp[160] * 100) % 100) / 100m;
                        testingInterfaceFW16.SetAdjustment(document, TestingInterfaceFW16.receiptKind[receiptKind], sumCorr);
                        decimal sum = 0m;
                        decimal totalaPaid = 0;
                        for (int tenderCode = 1; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)                           //перебор видов платежей
                        {
                            totalaPaid += sum = Math.Round(testingInterfaceFW16.RegistersTmp[160] / 9 - tenderCode, 2);
                            testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], (Native.CmdExecutor.TenderCode)tenderCode, sum);
                        }

                        sum = testingInterfaceFW16.RegistersTmp[160] - totalaPaid;
                        testingInterfaceFW16.AddPayment(document, TestingInterfaceFW16.receiptKind[receiptKind], Native.CmdExecutor.TenderCode.Cash, sum + (random.Next(0, (int)(sum * (10m / 100m)))));       //оплата наличными
                        Console.Write($"({i++}/{countReciepts}) ");
                        ret += testingInterfaceFW16.DocumentComplete(document, TestingInterfaceFW16.receiptKind[receiptKind], abort);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Формирует набор данных тестирования нефискального документа исходя из полученных номеров регистров
        /// </summary>
        /// <param name="registers">Строка, с номерами регистров, разделённых , </param>
        public void CreateNFDocDataCollection(string registers)
        {
            List<string> regList = new List<string>(registers.Split(','));                      //Разделяет строку на подстроки
            List<TestDataNFDoc> listNFDocTmp = new List<TestDataNFDoc>();//создаются временные списки
            regList.Remove("");                                                                 //удаляется пустая строка, если она есть
            for (int i = 0; i < regList.Count; i++)
            {
                if (regList.IndexOf(regList[i]) != i) { regList.RemoveAt(i); i--; }             //удаляются повторения
            }
            foreach (var item in regList)                                                       //перебор номеров регистров
            {
                int nfDocType;                              //Тип нефискального документа
                int tenderCode;                             //Номер платежа
                int numberRegister = Int32.Parse(item);

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

                if (110 < numberRegister && numberRegister < 119)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 111-118 регистрах
                {
                    tenderCode = numberRegister % 10 - 1;
                    for (nfDocType = 1; nfDocType < TestingInterfaceFW16.countNFDocType; nfDocType++)
                    {
                        listNFDocTmp.Add(new TestDataNFDoc(nfDocType, tenderCode));                                                                                                                                     //Добавление тестовых данных для нефискального документа
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
                                listNFDocTmp.Add(new TestDataNFDoc(nfDocType, tenderCode));                                                                                                                             //добавление тестовых данных для нефискального документа
                            }
                        }
                    }
                    continue;
                }
            }
            foreach (var testData in listNFDocTmp)
            {
                if (!testDataNFDocList.Contains(testData)) { testDataNFDocList.Add(testData); }
            }

        }

        /// <summary>
        /// Формирует набор данных тестирования чека коррекции исходя из полученных номеров регистров
        /// </summary>
        /// <param name="registers">Строка, с номерами регистров, разделённых , </param>
        public void CreateCorrectionDataCollection(string registers)
        {
            List<string> regList = new List<string>(registers.Split(','));                      //Разделяет строку на подстроки    
            List<TestDataCorrection> listCorrectionTmp = new List<TestDataCorrection>();        //создаются временные списки
            regList.Remove("");                                                                 //удаляется пустая строка, если она есть
            for (int i = 0; i < regList.Count; i++)
            {
                if (regList.IndexOf(regList[i]) != i) { regList.RemoveAt(i); i--; }             //удаляются повторения
            }
            foreach (var item in regList)                                                       //перебор номеров регистров
            {
                int receiptKind;                            //Тип чека
                int vatCode;                                //Ставка НДС
                int tenderCode;                             //Номер платежа
                int numberRegister = Int32.Parse(item);
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

                if (110 < numberRegister && numberRegister < 119)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 111-118 регистрах
                {
                    tenderCode = numberRegister % 10 - 1;
                    for (receiptKind = 1; receiptKind < TestingInterfaceFW16.countReceiptKind; receiptKind += 2)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));                                                                                                            //Добавление тестовых данных для чека коррекции
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
                            for (receiptKind = 1; receiptKind < TestingInterfaceFW16.countReceiptKind; receiptKind += 2)
                            {
                                for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                                {
                                    listCorrectionTmp.Add(new TestDataCorrection(receiptKind, vatCode, tenderCode));                                                                                                    //добавление тестовых данных для чека коррекции
                                }
                            }
                        }
                    }
                    continue;
                }
            }
            foreach (var testData in listCorrectionTmp)
            {
                if (!testDataCorrectionList.Contains(testData)) { testDataCorrectionList.Add(testData); }
            }

        }

        /// <summary>
        /// Формирует набор данных тестирования чека исходя из полученных номеров регистров
        /// </summary>
        /// <param name="registers">Строка, с номерами регистров, разделённых , </param>
        public void CreateReceiptDataCollection(string registers)
        {
            List<string> regList = new List<string>(registers.Split(','));                      //Разделяет строку на подстроки
            List<TestDataReceipt> listReceiptTmp = new List<TestDataReceipt>();                 //создаются временные списки
            regList.Remove("");                                                                 //удаляется пустая строка, если она есть
            for (int i = 0; i < regList.Count; i++)
            {
                if (regList.IndexOf(regList[i]) != i) { regList.RemoveAt(i); i--; }             //удаляются повторения
            }
            foreach (var item in regList)                                                       //перебор номеров регистров
            {
                int receiptKind;                            //Тип чека
                int vatCode;                                //Ставка НДС
                int itemPaymentKind;                        //Тип оплаты товара
                int adjustment;                             //Добавление товара по
                int tenderCode;                             //Номер платежа
                int numberRegister = Int32.Parse(item);     //Номер регистра
                if (numberRegister > 0 && numberRegister < 5)                                                                                                                                                           //Создаёт тестовые данные для проврки ошибки в 1-4 регистрах
                {
                    receiptKind = numberRegister;
                    for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                    {
                        for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                        {
                            for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                            {
                                for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                            for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                                    for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                                    {
                                        listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                        for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                        {
                            for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                        for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                        {
                            for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                                for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                                {
                                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                    {
                                        listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                            for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                            {
                                for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                            for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                            {
                                for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                                for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
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
                                for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                                {
                                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                                    {
                                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.Cash) && numberRegister == 180) listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
                                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash) && numberRegister == 181) listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }
                if (181 < numberRegister && numberRegister < 190)                                                                                                                                                                              //Создаёт тестовые данные для проврки ошибки в 119 регистрах
                {
                    adjustment = numberRegister % 2;
                    receiptKind = (numberRegister - 180) / 2;
                    for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                    {
                        if (TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash))
                        {
                            for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                            {
                                for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));                                                                         //добавление тестовых данных для чека
                                }
                            }
                        }
                    }
                    continue;
                }
                /*191-194 не участвуют в минимальных тестах*/
                if (199 < numberRegister && numberRegister < 207 || 209 < numberRegister && numberRegister < 217 || 219 < numberRegister && numberRegister < 227 || 229 < numberRegister && numberRegister < 237)       //Создаёт тестовые данные для проврки ошибки в 200-206, 210-216, 220-226, 230-236 регистрах
                {
                    receiptKind = (numberRegister - 190) / 10;
                    itemPaymentKind = numberRegister % 10;
                    for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                    {
                        for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                        {
                            for (tenderCode = 0; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));
                            }
                        }
                    }
                    continue;
                }
                if (110 < numberRegister && numberRegister < 119)                                                                                                                                                       //Создаёт тестовые данные для проврки ошибки в 111-118 регистрах
                {
                    tenderCode = numberRegister % 10 - 1;
                    for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                    {
                        for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                        {
                            for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                            {
                                for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                                {
                                    listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));                                                                                 //добавление тестовых данных для чека
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
                            for (receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
                            {
                                for (vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                                {
                                    for (itemPaymentKind = 1; itemPaymentKind <= TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                                    {
                                        for (adjustment = 0; adjustment < TestingInterfaceFW16.countAdjustment; adjustment++)
                                        {
                                            listReceiptTmp.Add(new TestDataReceipt(receiptKind, vatCode, itemPaymentKind, adjustment, tenderCode));                                                                         //добавление тестовых данных для чека
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
        }
    }

    /// <summary>
    /// Класс, хранящий набор тестовых данных для чека
    /// </summary>
    class TestDataReceipt
    {
        /// <summary>
        /// Номер типа чека коррекции
        /// </summary>
        public int receiptKind;
        /// <summary>
        /// Номер процентной ставки
        /// </summary>
        public int vatCode;
        /// <summary>
        /// Номер типа оплаты товара
        /// </summary>
        public int itemPaymentKind;
        /// <summary>
        /// Номер типа добавления товара
        /// </summary>
        public int itemBy;
        /// <summary>
        /// Номер платежа
        /// </summary>
        public int tenderCode;
        /// <summary>
        /// Номер типа коррекции суммы
        /// </summary>
        public int adjustment;

        public TestDataReceipt(int receiptKind, int vatCode, int itemPaymentKind, int adjustment, int tenderCode, int itemBy = 0)
        {
            this.receiptKind = receiptKind;
            this.vatCode = vatCode;
            this.itemPaymentKind = itemPaymentKind;
            this.itemBy = itemBy;
            this.tenderCode = tenderCode;
            this.adjustment = adjustment;
        }

        public override bool Equals(object obj)
        {
            var receipt = obj as TestDataReceipt;
            return receipt != null &&
                   receiptKind == receipt.receiptKind &&
                   vatCode == receipt.vatCode &&
                   itemPaymentKind == receipt.itemPaymentKind &&
                   itemBy == receipt.itemBy &&
                   tenderCode == receipt.tenderCode &&
                   adjustment == receipt.adjustment;
        }

        public override int GetHashCode()
        {
            var hashCode = 1741174571;
            hashCode = hashCode * -1521134295 + receiptKind.GetHashCode();
            hashCode = hashCode * -1521134295 + vatCode.GetHashCode();
            hashCode = hashCode * -1521134295 + itemPaymentKind.GetHashCode();
            hashCode = hashCode * -1521134295 + itemBy.GetHashCode();
            hashCode = hashCode * -1521134295 + tenderCode.GetHashCode();
            hashCode = hashCode * -1521134295 + adjustment.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"|{TestingInterfaceFW16.receiptKind[receiptKind],12}|{TestingInterfaceFW16.vatCode[vatCode],17}|{TestingInterfaceFW16.itemPaymentKind[itemPaymentKind],17}|{(TestingInterfaceFW16.ItemBy)itemBy,8}|{(TestingInterfaceFW16.AdjustmentType)adjustment,5}|{tenderCode,15}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode)tenderCode]],15}|\n";
        }
    }

    /// <summary>
    /// Класс, хранящий набор тестовых данных для чека коррекции
    /// </summary>
    class TestDataCorrection
    {
        /// <summary>
        /// Номер типа чека коррекции
        /// </summary>
        public int receiptKind;
        /// <summary>
        /// Номер процентной ставки
        /// </summary>
        public int vatCode;
        /// <summary>
        /// Номер платежа
        /// </summary>
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

    /// <summary>
    /// Класс, хранящий набор тестовых данных для нефискального документа
    /// </summary>
    class TestDataNFDoc
    {
        /// <summary>
        /// номер типа нефискального документа
        /// </summary>
        public int nfDocType;
        /// <summary>
        /// номер платежа
        /// </summary>
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
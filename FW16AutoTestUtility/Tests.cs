using System;
using System.Collections.Generic;
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
        decimal[] coasts = new decimal[] { 217m, 193.7m };          //варианты цен
        decimal[] counts = new decimal[] { 1m, 5m, 0.17m, 1.73m };  //варианты колличества


        public Tests()
        {
            TestingInterfaceFW16 = new TestingInterfaceFW16(out ecrCtrl);
            BeginTest();
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
            ecrCtrl.Shift.Open(nameOperator);                //открытие смены для этого теста
            TestingInterfaceFW16.GetRegisters();
            TestingInterfaceFW16.GetCounters();
            TestReceipt();                                  //вызов функции тестирования чека
            TestCorrection();                               //вызов функции тестирования чека коррекции
            TestNonFiscal();                                //вызов функции нефискального документа
            TestReceipt(true);                              //вызов функции тестирования чека c отменой.
            TestNonFiscal(true);                            //вызов функции нефискального документа с отменой
            ecrCtrl.Shift.Close(nameOperator);               //закрытие смены этого теста

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
        private void TestNonFiscal(bool abort = false)
        {
            for (int nfDocType = 1; nfDocType < 4; nfDocType++)                                           //Перебор типов нефиксальных документов
            {
                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.NonFiscalBase document, (Native.CmdExecutor.NFDocType)nfDocType);
                for (int i = 0; i < TestingInterfaceFW16.countCoasts * TestingInterfaceFW16.countTenderCode && nfDocType != 3; i++)                                         //
                {
                    var tender = new Tender
                    {
                        Amount = coasts[i / TestingInterfaceFW16.countTenderCode],
                        Code = (Native.CmdExecutor.TenderCode)(i % TestingInterfaceFW16.countTenderCode)
                    };

                    TestingInterfaceFW16.AddTender(document, (Native.CmdExecutor.NFDocType)nfDocType, (Native.CmdExecutor.TenderCode)(i / TestingInterfaceFW16.countCoasts % TestingInterfaceFW16.countTenderCode), coasts[i % TestingInterfaceFW16.countCoasts]);
                }
                document.PrintText("Тестовый текст теста текстовго нефиксального документа");

                TestingInterfaceFW16.DocumentComplete(document, (Native.CmdExecutor.NFDocType)nfDocType, abort);
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
                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Correction document, nameOperator, (ReceiptKind)receiptKind);
                decimal sum = 0;

                for (int i = 0; i < TestingInterfaceFW16.countCoasts * TestingInterfaceFW16.countTenderCode; i++)         //перебор возврата средств всеми способами, целове и дробная суммы
                {
                    TestingInterfaceFW16.AddTender(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)(i / TestingInterfaceFW16.countCoasts % TestingInterfaceFW16.countTenderCode), coasts[i % TestingInterfaceFW16.countCoasts]);
                    sum += coasts[i % TestingInterfaceFW16.countCoasts];
                }
                decimal sumPaid = 0m;
                for (ushort i = 1; i <= TestingInterfaceFW16.countVatCode; i++)                      //перебор налоговых ставок
                {
                    sumPaid = Math.Round(sum / ((TestingInterfaceFW16.countVatCode + 1) - i), 2);
                    TestingInterfaceFW16.AddAmount(document, (ReceiptKind)receiptKind, (VatCode)i, sumPaid);
                    sum = sum - sumPaid;
                }


                TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort);
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
                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, (ReceiptKind)receiptKind);

                bool coast = true;
                for (int i = 0; i < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countVatCode * TestingInterfaceFW16.countCoasts * TestingInterfaceFW16.countPaymentKind); i++)
                {

                    TestingInterfaceFW16.AddEntry(document,
                        (ReceiptKind)receiptKind,
                        "Tovar" + i.ToString(),
                        counts[i / TestingInterfaceFW16.countVatCode / TestingInterfaceFW16.countCoasts / TestingInterfaceFW16.countPaymentKind % TestingInterfaceFW16.countCounts],
                        (Native.CmdExecutor.VatCodeType)((i / TestingInterfaceFW16.countCoasts / TestingInterfaceFW16.countPaymentKind % TestingInterfaceFW16.countVatCode) + 1),
                        TestingInterfaceFW16.ItemBy.coast,
                        coasts[i / TestingInterfaceFW16.countPaymentKind % TestingInterfaceFW16.countCoasts],
                        (ItemPaymentKind)((i % TestingInterfaceFW16.countPaymentKind) + 1));  //создание товара
                }

                decimal sum = 0m;
                for (int tenderCode = 1; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                {
                    sum = Math.Round(document.Total / 9 - tenderCode, 2);
                    sum += (decimal)(new Random().Next(-1 * (int)sum * (5 / 100), (int)sum * (5 / 100)));
                    TestingInterfaceFW16.AddPayment(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)tenderCode, sum);
                    sum = document.Total - document.TotalaPaid;
                }

                TestingInterfaceFW16.AddPayment(document, (ReceiptKind)receiptKind, Native.CmdExecutor.TenderCode.Cash, sum);

                TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort);
            }
        }
    }
}
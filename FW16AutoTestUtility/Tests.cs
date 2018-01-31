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
            ecrCtrl.Shift.Open(nameOperator);                //открытие смены для этого теста
            TestingInterfaceFW16.GetRegisters();
            TestingInterfaceFW16.GetCounters();
            if (TestReceiptMax() != 0)//вызов функции тестирования чека
            {
            //Console.WriteLine($"+------------+-----------------+-----------------+--------+---------------+---------------+\n" +
            //    $"|{"Тип чека",12}|{"Процентная ставка",17}|{"Тип оплаты",17}|{"Товар по",8}|{"Номер оплаты",15}|{"Тип оплаты",15}|\n" +
            //    $"+------------+-----------------+-----------------+--------+---------------+---------------+\n" + TestReceiptMin());
            }
            TestCorrection();                               //вызов функции тестирования чека коррекции
            TestNonFiscal();                                //вызов функции нефискального документа
            TestReceiptMax(true);                              //вызов функции тестирования чека c отменой.
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
        /// Тестирование чека с перебором большого количества товаров
        /// </summary>
        /// <param name="abort">Отменить создание чека</param>
        private int TestReceiptMax(bool abort = false)
        {
            int ret = 0;

            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
            {
                for (int itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                {
                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, (ReceiptKind)receiptKind);
                    for (int vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                    {
                        for (int itemPaymentKind = 1; itemPaymentKind < TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                        {
                            for (int i = 0; i < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countCoasts); i++)
                            {
                                TestingInterfaceFW16.AddEntry(document,
                                    (ReceiptKind)receiptKind,
                                    "Item " + ((Native.CmdExecutor.VatCodeType)vatCode) + " " + (TestingInterfaceFW16.ItemBy)itemBy + " " + (ItemPaymentKind)itemPaymentKind + " " + i,
                                    counts[i / TestingInterfaceFW16.countCoasts % TestingInterfaceFW16.countCounts],
                                    (Native.CmdExecutor.VatCodeType)vatCode,
                                    (TestingInterfaceFW16.ItemBy)itemBy,
                                    coasts[i % TestingInterfaceFW16.countCoasts],
                                    (ItemPaymentKind)itemPaymentKind);  //создание товара
                            }


                        }
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

            for (int receiptKind = 1; receiptKind <= TestingInterfaceFW16.countReceiptKind; receiptKind++)
            {
                for (int vatCode = 1; vatCode <= TestingInterfaceFW16.countVatCode; vatCode++)
                {
                    for (int itemPaymentKind = 1; itemPaymentKind < TestingInterfaceFW16.countItemPaymentKind; itemPaymentKind++)
                    {
                        for (int itemBy = 0; itemBy < TestingInterfaceFW16.countItemBy; itemBy++)
                        {
                            for (int tenderCode = 1; tenderCode < TestingInterfaceFW16.countTenderCode; tenderCode++)
                            {
                                TestingInterfaceFW16.StartDocument(out Fw16.Ecr.Receipt document, nameOperator, (ReceiptKind)receiptKind);
                                for (int i = 0; i < (TestingInterfaceFW16.countCounts * TestingInterfaceFW16.countCoasts); i++)
                                {
                                    TestingInterfaceFW16.AddEntry(document,
                                        (ReceiptKind)receiptKind,
                                        "Item " + ((Native.CmdExecutor.VatCodeType)vatCode) + " " + (TestingInterfaceFW16.ItemBy)itemBy + " " + (ItemPaymentKind)itemPaymentKind + " " + i,
                                        counts[i / TestingInterfaceFW16.countCoasts % TestingInterfaceFW16.countCounts],
                                        (Native.CmdExecutor.VatCodeType)vatCode,
                                        (TestingInterfaceFW16.ItemBy)itemBy,
                                        coasts[i % TestingInterfaceFW16.countCoasts],
                                        (ItemPaymentKind)itemPaymentKind);  //создание товара
                                }
                                TestingInterfaceFW16.AddPayment(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode) tenderCode, document.Total);
                                if (TestingInterfaceFW16.DocumentComplete(document, (ReceiptKind)receiptKind, abort) != 0)
                                {
                                    err += $"|{(ReceiptKind)receiptKind,12}|{(Native.CmdExecutor.VatCodeType)vatCode,17}|{(ItemPaymentKind)itemPaymentKind,17}|{(TestingInterfaceFW16.ItemBy)itemBy,8}|{(Native.CmdExecutor.TenderCode)tenderCode,15}|{(Native.CmdExecutor.TenderType)TestingInterfaceFW16.tenderCodeType[(Native.CmdExecutor.TenderCode) tenderCode],15}|\n";
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
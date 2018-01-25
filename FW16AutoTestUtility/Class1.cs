﻿using System;
using System.Collections.Generic;
using System.Text;
using Fw16;
using Fw16.Model;

namespace FWAutoTestUtility
{
    class Class1
    {
        public EcrCtrl ecrCtrl;                                     //подключение к ККТ
        public int[] counters = new int[23];                        //массив счётчиков
        public decimal[] registers = new decimal[236];              //массив регистров
        decimal[] registersTmp = new decimal[236];           //массив временных регистров
        string nameOerator = "test program";                        //имя касира 
        decimal[] coasts = new decimal[] { 217m, 193.7m };          //варианты цен
        decimal[] counts = new decimal[] { 1m, 5m, 0.17m, 1.73m };  //варианты колличества
        Dictionary<Native.CmdExecutor.VatCodeType, int> vatCode;
        Dictionary<Fw16.Model.VatCode, int> vatCode2;
        private Dictionary<ItemPaymentKind, int> paymentKind;
        private Dictionary<ReceiptKind, int> receiptKind;

        enum MyEnum
        {
            
        }

        public Class1()
        {
            ecrCtrl = new EcrCtrl();
            vatCode = new Dictionary<Native.CmdExecutor.VatCodeType, int>() {
                { Native.CmdExecutor.VatCodeType.Vat18,1 },
                { Native.CmdExecutor.VatCodeType.Vat10,2 },
                { Native.CmdExecutor.VatCodeType.Vat0,3 },
                { Native.CmdExecutor.VatCodeType.NoVat,4 },
                { Native.CmdExecutor.VatCodeType.Vat18Included,5 },
                { Native.CmdExecutor.VatCodeType.Vat10Included,6 },
            };
            vatCode2 = new Dictionary<Fw16.Model.VatCode, int>() {
                { Fw16.Model.VatCode.Vat18,1 },
                { Fw16.Model.VatCode.Vat10,2 },
                { Fw16.Model.VatCode.Vat0,3 },
                { Fw16.Model.VatCode.NoVat,4 },
                { Fw16.Model.VatCode.Vat18Included,5 },
                { Fw16.Model.VatCode.Vat10Included,6 },
            };
            paymentKind = new Dictionary<Fw16.Model.ItemPaymentKind, int>
            {
                {ItemPaymentKind.Prepay,0 },
{ItemPaymentKind.PartlyPrepay,1 },
{ItemPaymentKind.Advance,2 },
{ItemPaymentKind.Payoff,3 },
{ItemPaymentKind.PartlyLoanCredit,4 },
{ItemPaymentKind.LoanCredit,5 },
{ItemPaymentKind.PayCredit,6 }
            };
            receiptKind = new Dictionary<ReceiptKind, int>
            {
                {ReceiptKind.Income,1 },
                {ReceiptKind.IncomeBack,2 },
                {ReceiptKind.Outcome,3 },
                { ReceiptKind.OutcomeBack,4}
            };

            ConnectToFW();
            BeginTest();
        }

        //функция подключения/переподключения к ККТ
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

        private void BeginTest()
        {
            ConnectToFW();
            Preparation();
            SimpleTest();
        }

        public void Preparation()                                                                        //Функция подготовки к тестам
        {
            ecrCtrl.Service.SetParameter(Native.CmdExecutor.ParameterCode.AbortDocFontSize, "51515");    //отключение печати чека
            if ((ecrCtrl.Info.Status & Fw16.Ecr.GeneralStatus.DocOpened) > 0)
            {
                ecrCtrl.Service.AbortDoc();                                                             //закрыть документ если открыт
            }
            if ((ecrCtrl.Info.Status & Fw16.Ecr.GeneralStatus.ShiftOpened) > 0)
            {
                ecrCtrl.Shift.Close(nameOerator);                                                       //закрыть смену если открыта
            }
        }

        public void SimpleTest()                            //функция прогона по всем видам чеков и чеков коррекции
        {
            ecrCtrl.Shift.Open(nameOerator);                //открытие смены для этого теста
            GetRegisters();
            GetCounters();
            TestReceipt();                                  //вызов функции тестирования чека
            //TestCorrection();                               //вызов функции тестирования чека коррекции
            //TestNonFiscal();                                //вызов функции нефискального документа
            TestReceipt(true);                              //вызов функции тестирования чека c отменой
            //TestCorrection(true);                         //вызов функции тестирования чека коррекции с отменой
            //отключено в связи с тем что чек коррекции не возможно отменить, потому что он отправляется одним пакетом.
            //TestNonFiscal(true);                            //вызов функции нефискального документа с отменой
            ecrCtrl.Shift.Close(nameOerator);               //закрытие смены этого теста

            RequestRegisters();
            RequestCounters();

            Console.WriteLine("Завершено тестирование SimpleTest ");     //логирование
        }

        private void TestNonFiscal(bool abort = false)                                              //тест нефискального документа
        {
            for (int nfdType = 1; nfdType < 4; nfdType++)                                           //Перебор типов нефиксальных документов
            {
                var document = ecrCtrl.Shift.BeginNonFiscal((Native.CmdExecutor.NFDocType)nfdType); //открытие нефиксального документа
                for (int i = 0; i < 14 && nfdType < 3; i++)                                         //
                {
                    var tender = new Fw16.Model.Tender
                    {
                        Amount = coasts[i / 7],
                        Code = (Native.CmdExecutor.TenderCode)(i % 7)
                    };
                    document.AddTender(tender);

                }
                document.PrintText("Тестовый текст теста текстовго нефиксального документа");
                if (abort)
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);
                    Console.WriteLine("Оформлен нефиксальный документ типа " + (Native.CmdExecutor.NFDocType)nfdType + "");
                    counters[nfdType + 8 + 11]++;
                }
                else
                {
                    document.Complete(Native.CmdExecutor.DocEndMode.Default);                                           //закрытие нефиксального документа
                    Console.WriteLine("Оформлен нефиксальный документ типа " + (Native.CmdExecutor.NFDocType)nfdType + "");
                    counters[nfdType + 8]++;
                }
            }
        }

        private void TestCorrection(bool abort = false)
        {
            for (int ReceptKind = 1; ReceptKind < 4; ReceptKind += 2)
            {
                var document = ecrCtrl.Shift.BeginCorrection(nameOerator, (Fw16.Model.ReceiptKind)ReceptKind);
                decimal sum = 0;

                decimal[] registersTmp = new decimal[236];           //массив временных регистров

                for (int i = 0; i < 7; i++)                                                                             //перебор возврата средств всеми способами, целове и дробная суммы
                {
                    document.AddTender((Native.CmdExecutor.TenderCode)(i / 2), coasts[i % 2]);
                    sum += coasts[i % 2];
                }
                for (int i = 1; i < 6; i++)                                                                             //перебор налоговых ставок
                {
                    document.AddAmount((Fw16.Model.VatCode)(i), Math.Round(sum / 6m, 2));

                    registersTmp[(ReceptKind==3?10:0) + i + 60] += Math.Round(sum / 6m, 2);     //сумма по ставкам НДС
                    if (i != 3 && i != 4)
                        registersTmp[(ReceptKind - 1) * 10 + (i > 4 ? i - 2 : i) + 120 + 5] += Math.Round(sum / 6m, 2);    //сумма НДС 

                }
                document.AddAmount(Fw16.Model.VatCode.NoVat, sum - Math.Round(sum / 6, 2) * 5);
                if (abort)
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);
                    Console.WriteLine("Отменён чек коррекции типа " + (Fw16.Model.ReceiptKind)ReceptKind + "");         //логирование
                    counters[ReceptKind + 4 + 11]++;
                }
                else
                {
                    registers[ReceptKind + 4] += sum;
                    document.Complete();                                                                                //закрытие чека коррекции
                    Console.WriteLine("Оформлен чек коррекции типа " + (Fw16.Model.ReceiptKind)ReceptKind + "");        //логирование
                    counters[ReceptKind + 4]++;
                }
            }
        }

        private void TestReceipt(bool abort = false)
        {
            for (int ReceptKind = 1; ReceptKind < 5; ReceptKind++)
            {
                var document = ecrCtrl.Shift.BeginReceipt(nameOerator, (Fw16.Model.ReceiptKind)ReceptKind, new
                {
                    Taxation = Fs.Native.TaxationType.Agro,         //налогообложение по умолчанию
                    CustomerAddress = "qwe@ewq.xxx",                //адрес получателя
                    SenderAddress = "ewq@qwe.yyy"                   //адрес отправтеля
                });
                SetValue(registers, 0, 160, 182);
                Fw16.Ecr.ReceiptEntry receiptEntry;
                for (int i = 0; i < (4*6*2*7); i++)
                {
                    /*4 - количество, 6-ставок ндс, 2- цены, 7 - типов оплаты(1-7) */
                    //создание товара
                    receiptEntry = document.NewItemCosted(i.ToString(), "tovar " + i, counts[i / 6/2/7], (Native.CmdExecutor.VatCodeType)((i / 2/7 % 6) + 1), coasts[i/7 % 2]);
                    receiptEntry.PaymentKind = (ItemPaymentKind)((i%7)+1);
                    document.AddEntry(receiptEntry);                                                        //добавления товара в чек
                    registersTmp[(ReceptKind - 1) * 10 + vatCode[receiptEntry.VatCode] - 1 + 120] += receiptEntry.Cost;     //сумма по ставкам НДС
                    if (vatCode[receiptEntry.VatCode] != 3 && vatCode[receiptEntry.VatCode] != 4)
                        registersTmp[(ReceptKind - 1) * 10 + (vatCode[receiptEntry.VatCode] > 4 ? vatCode[receiptEntry.VatCode] - 2 : vatCode[receiptEntry.VatCode]) + 120 + 5] += receiptEntry.VatAmount;    //сумма НДС 
                    registersTmp[ReceptKind * 10 + paymentKind[receiptEntry.PaymentKind] + 190] += receiptEntry.Cost; //сумма по способу рассчёта 

                    registersTmp[160] += receiptEntry.Cost;                                                 //Сумма открытого документа; рассчитывается по стоимости тваров
                    registersTmp[vatCode[receiptEntry.VatCode] + 160] += receiptEntry.Cost;                     //Сумма открытого документа по ставкам НДС
                    if (vatCode[receiptEntry.VatCode] != 3 && vatCode[receiptEntry.VatCode] != 4)
                        registersTmp[(vatCode[receiptEntry.VatCode] > 4 ? vatCode[receiptEntry.VatCode] - 2 : vatCode[receiptEntry.VatCode]) + 160 + 6] += receiptEntry.VatAmount; //сумма НДС открытого документа
                    registersTmp[171]++;                                                                    //Количество товарных позиций


                }
                decimal balance = Math.Round(document.Total / 8m, 2);                                       //Сумма разделённая на количество типов оплаты.
                for (int tenderCode = 1; tenderCode < 7; tenderCode++)
                {
                    document.AddPayment((Native.CmdExecutor.TenderCode)tenderCode, balance);                //оплата всеми способами кроме нала

                    registersTmp[ReceptKind] += balance;                                                    //сумма по типу операции
                    registersTmp[ReceptKind * 10 + 1 + tenderCode] += balance;                              //сумма по номеру платежа
                    registersTmp[ReceptKind * 10 + 1 + 8] += balance;                                       //сумма электрооного типа платежа

                    registersTmp[tenderCode + 172] += balance;                                              //сумма открытого документа по номеру платежа
                    registersTmp[181] += balance;                                                           //сумма открытого документа электронного типа платежа

                    registersTmp[ReceptKind + 190] += balance;                                              //накопительный регистр по типу операции
                }
                balance = document.Total - document.TotalaPaid;                                             //вычисление остатка суммы для оплаты 
                document.AddPayment((Native.CmdExecutor.TenderCode)0, balance);                             //оплата наличнми

                registersTmp[ReceptKind] += balance;                                                        //сумма прихода
                registersTmp[ReceptKind * 10 + 1 + 0] += balance;                                           //сумма прихода по номеру платежа

                registersTmp[0 + 172] += balance;                                                           //сумма открытого документа по номеру платежа 0
                registersTmp[180] += balance;                                                               //сумма открытого документа наличного типа платежа

                registersTmp[ReceptKind + 190] += balance;                                                  //накопительный регистр по типу операции
                
                if (abort)
                {
                    document.Abort();                                                                       //отмена документа
                    Console.WriteLine("Отменён чек типа " + (Fw16.Model.ReceiptKind)ReceptKind + "");       //логирование
                    counters[ReceptKind + 11]++;                                                            //увеличение счётчика отмены соотвествующего типа чека

                }
                else
                {
                    document.Complete();                                                                    //закрытие чека
                    Console.WriteLine("Оформлен чек типа " + (Fw16.Model.ReceiptKind)ReceptKind + "");      //логирование
                    counters[ReceptKind]++;                                                                 //учеличение счётчика оформленного соответсвющего типа чека
                    AddRegistersTmp(registersTmp);
                }
                RequestRegisters(160, 182);                                                                 //запрос регистров по открытому документу
            }
        }

        public void RequestRegisters(ushort startIndex = 1, ushort endIndex = 0)        //запрос значений всех регистров / начиная с индекса / в диапозоне [startIndex,endIndex) 
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)236;                                                           //проверка конечного значения если 0, то до конца
            string err = "";                                                                                            //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i < endIndex; i++)
            {
                try
                {
                    decimal tmp = ecrCtrl.Info.GetRegister(i);
                    if (tmp != registers[i]) { err += $"Счётчик {i} имеет расхождеие с ККТ {registers[i]} != {tmp}\n"; }//заполнение ошибки несоотвествия регистров
                }
                catch (Exception)
                {
                    Console.WriteLine("Не удолось получить доступ к регистру №" + i + "");
                }
            }
            Console.WriteLine("Запрошены данные с регистров с " + startIndex + " по " + endIndex + "\n" + err);           //логирование
        }

        public void RequestCounters(ushort startIndex = 1, ushort endIndex = 0)         //запрос значений всех счётчиков / начиная с индекса / в диапозоне [startIndex,endIndex)
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

        public void GetRegisters()                                                      //считывание значений всех регистров в переменные
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
                }
            }
            Console.WriteLine("Запрошены данные с регистров получены");     //логирование
        }

        public void GetCounters()                                                       //считывание значений всех счётчиков в переменные
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

        public void AddRegistersTmp(decimal[] registersTmp)                             //функция применения временных регистров к конечным
        {
            ushort endIndex = 236;
            ushort startIndex = 1;
            SetValue(registers, 0, 160, 182);
            for (int i = startIndex; i < endIndex; i++)
            {
                registers[i] += registersTmp[i];                                        //применение временного массива к конечному
            }
        }

        void SetValue(decimal[] arr,decimal value, ushort startIndex=0, ushort endIndex=0)      //устаовка значений для переданного массива
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)arr.Length;
            for (int i = startIndex; i < endIndex; i++)
            {
                arr[i] = value;
            }
        }


        /// <summary>
        /// Создаёт и добавляет товар в чек. Записывает суммы во временный регистр.
        /// </summary>
        /// <param name="document">Чек в который необходимо добавить товар</param>
        /// <param name="receiptKind">Тип чека (Приход, Отмена прихода..)</param>
        /// <param name="name">Название товара</param>
        /// <param name="count">Количество товара</param>
        /// <param name="vatCode">Тип налоговой ставки</param>
        /// <param name="coast">true - параметр money - стоимость, false - цена </param>
        /// <param name="money">Сумма</param>
        /// <param name="paymentKind">Способ рассчёта (Предоплата, полная оплата, кредит..)</param>
        /// <param name="kind">Тип добавляемого товара (товар,услуга..)</param>
        void AddEntry(Fw16.Ecr.Receipt document,ReceiptKind receiptKind, string name, decimal count, Native.CmdExecutor.VatCodeType vatCode, bool coast, decimal money, ItemPaymentKind paymentKind = ItemPaymentKind.Payoff, ItemFlags kind = ItemFlags.Regular)
        {
            Fw16.Ecr.ReceiptEntry receiptEntry;                                                                                 //товар
            if (coast) receiptEntry = document.NewItemCosted(new Random().Next().ToString(), name, count, vatCode, money);      //создание по стоимости
            else receiptEntry = document.NewItemPriced(new Random().Next().ToString(), name,vatCode, money,  count);            //создание по цене
            receiptEntry.PaymentKind = paymentKind;                                                                             //спооб рассчёта
            receiptEntry.Kind = kind;                                                                                           //тип добавляемого товара
            document.AddEntry(receiptEntry);                                                                                    //добавления товара в чек

            registersTmp[(this.receiptKind[receiptKind] - 1) * 10 + this.vatCode[vatCode] - 1 + 120] += receiptEntry.Cost;      //добаление в регистр (120-125,130-135,140-145,150-155) суммы по ставке НДС
            if (this.vatCode[vatCode] != 3 && this.vatCode[vatCode] != 4)                                                       //проверка на нулевые ставки НДС
                registersTmp[(this.receiptKind[receiptKind] - 1) * 10 + (this.vatCode[vatCode] > 4 ? this.vatCode[vatCode] - 2 : this.vatCode[vatCode]) + 120 + 5] += receiptEntry.VatAmount;    //добавление в регистр (126-129,136-139,146-149,156-159) суммы НДС 
            registersTmp[this.receiptKind[receiptKind] * 10 + this.paymentKind[paymentKind] + 190] += receiptEntry.Cost;        //добавление в регистр (20-206, 210-216, 220-226, 230-236) суммы по способу рассчёта 

            registersTmp[160] += receiptEntry.Cost;                                                                             //добавление в регистр (160) суммы открытого документа
            registersTmp[this.vatCode[vatCode] + 160] += receiptEntry.Cost;                                                     //добавление в регситр (161-166) сумма открытого документа по ставкам НДС
            if (this.vatCode[vatCode] != 3 && this.vatCode[vatCode] != 4)
                registersTmp[(this.vatCode[vatCode] > 4 ? this.vatCode[vatCode] - 2 : this.vatCode[vatCode]) + 160 + 6] += receiptEntry.VatAmount;                                               //добавление в регситр (167-170) суммы НДС открытого документа 
            registersTmp[171]++;                                                                                                //Добавление в регситр (171)  количество товарных позиций
        }
    }
}

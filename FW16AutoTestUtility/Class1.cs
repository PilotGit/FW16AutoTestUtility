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
        const int countCounts = 6;
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
        const int countTenderCode = 7;
        /// <summary>
        /// Количество ставок НДС
        /// </summary>
        const int countVatCode = 6;


        public EcrCtrl ecrCtrl;                                     //подключение к ККТ
        public int[] counters = new int[23];                        //массив счётчиков
        public decimal[] registers = new decimal[236];              //массив регистров
        decimal[] registersTmp = new decimal[236];           //массив временных регистров
        string nameOerator = "test program";                        //имя касира 
        decimal[] coasts = new decimal[] { 217m, 193.7m };          //варианты цен
        decimal[] counts = new decimal[] { 1m, 5m, 0.17m, 1.73m };  //варианты колличества

        private Dictionary<Native.CmdExecutor.VatCodeType, int> vatCode = new Dictionary<Native.CmdExecutor.VatCodeType, int>() {
                { Native.CmdExecutor.VatCodeType.Vat18,1 },
                { Native.CmdExecutor.VatCodeType.Vat10,2 },
                { Native.CmdExecutor.VatCodeType.Vat0,3 },
                { Native.CmdExecutor.VatCodeType.NoVat,4 },
                { Native.CmdExecutor.VatCodeType.Vat18Included,5 },
                { Native.CmdExecutor.VatCodeType.Vat10Included,6 },
            };
        private Dictionary<VatCode, int> vatCode2 = new Dictionary<VatCode, int>() {
                { VatCode.Vat18,1 },
                { VatCode.Vat10,2 },
                { VatCode.Vat0,3 },
                { VatCode.NoVat,4 },
                { VatCode.Vat18Included,5 },
                { VatCode.Vat10Included,6 },
            };
        private Dictionary<ItemPaymentKind, int> paymentKind = new Dictionary<Fw16.Model.ItemPaymentKind, int>
            {
                {ItemPaymentKind.Prepay,0 },
                {ItemPaymentKind.PartlyPrepay,1 },
                {ItemPaymentKind.Advance,2 },
                {ItemPaymentKind.Payoff,3 },
                {ItemPaymentKind.PartlyLoanCredit,4 },
                {ItemPaymentKind.LoanCredit,5 },
                {ItemPaymentKind.PayCredit,6 }
            };
        private Dictionary<ReceiptKind, int> receiptKind = new Dictionary<ReceiptKind, int>
            {
                {ReceiptKind.Income,1 },
                {ReceiptKind.IncomeBack,2 },
                {ReceiptKind.Outcome,3 },
                {ReceiptKind.OutcomeBack,4}
            };
        private Dictionary<Native.CmdExecutor.TenderType, int> tenderType = new Dictionary<Native.CmdExecutor.TenderType, int>
            {
                {Native.CmdExecutor.TenderType.Cash,0 },
                {Native.CmdExecutor.TenderType.NonCash,1 },
                {Native.CmdExecutor.TenderType.Advance,2 },
                {Native.CmdExecutor.TenderType.Credit,3 },
                {Native.CmdExecutor.TenderType.Barter,4 }
            };
        private Dictionary<Native.CmdExecutor.TenderCode, int> tenderCodeType;

        public Class1()
        {
            ecrCtrl = new EcrCtrl();
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

        public void Preparation()                                                                                   //Функция подготовки к тестам
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
            tenderCodeType = new Dictionary<Native.CmdExecutor.TenderCode, int>();
            var tenderList = ecrCtrl.Info.GetTendersList().GetEnumerator();                                         //получение коллекции соответствий кода платежа типу платежа
            for (int i = 0; i < countTenderCode; i++)
            {
                tenderList.MoveNext();                                                                              //перебор коллекции
                tenderCodeType.Add((Native.CmdExecutor.TenderCode)i, tenderType[tenderList.Current.Mode]);          //создание соответствия кода платежа типу 
            }
        }

        public void SimpleTest()                            //функция прогона по всем видам чеков и чеков коррекции
        {
            ecrCtrl.Shift.Open(nameOerator);                //открытие смены для этого теста
            GetRegisters();
            GetCounters();
            //TestReceipt();                                  //вызов функции тестирования чека
            TestCorrection();                               //вызов функции тестирования чека коррекции
            //TestNonFiscal();                                //вызов функции нефискального документа
            //TestReceipt(true);                              //вызов функции тестирования чека c отменой
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
            for (int receiptKind = 1; receiptKind < 4; receiptKind += 2)
            {
                var document = ecrCtrl.Shift.BeginCorrection(nameOerator, (ReceiptKind)receiptKind);
                SetValue(registersTmp, 0);
                decimal sum = 0;

                for (int i = 0; i < countCoasts * countTenderCode; i++)                                                                             //перебор возврата средств всеми способами, целове и дробная суммы
                {
                    AddTender(document, (ReceiptKind)receiptKind, (Native.CmdExecutor.TenderCode)(i / countCoasts % countTenderCode), coasts[i % countCoasts]);
                    sum += coasts[i % countCoasts];
                }
                decimal sumPaid = 0m;
                for (ushort i = 1; i < countVatCode; i++)                                                                             //перебор налоговых ставок
                {
                    sumPaid = Math.Round(sum / (countVatCode - i), 2);
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
                    AddEntry(document, (ReceiptKind)receiptKind, "Tovar" + i, counts[i / countVatCode / countCoasts / countPaymentKind % countCounts], (Native.CmdExecutor.VatCodeType)((i / countCoasts / countPaymentKind % countVatCode) + 1), coast, coasts[i / countPaymentKind % countCoasts], (ItemPaymentKind)((i % countPaymentKind) + 1));  //создание товара
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

        public void AddRegistersTmp()                                                   //функция применения временных регистров к конечным
        {
            ushort endIndex = 236;
            ushort startIndex = 1;
            for (int i = startIndex; i < endIndex; i++)
            {
                registers[i] += registersTmp[i];                                        //применение временного массива к конечному
            }
        }

        void SetValue(decimal[] arr, decimal value, ushort startIndex = 0, ushort endIndex = 0)      //установка значений для переданного массива
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
        void AddEntry(Fw16.Ecr.Receipt document, ReceiptKind receiptKind, string name, decimal count, Native.CmdExecutor.VatCodeType vatCode, bool coast, decimal money, ItemPaymentKind paymentKind = ItemPaymentKind.Payoff, ItemFlags kind = ItemFlags.Regular)
        {
            Fw16.Ecr.ReceiptEntry receiptEntry;                                                                                 //товар
            if (coast) receiptEntry = document.NewItemCosted(name, name, count, vatCode, money);      //создание по стоимости
            else receiptEntry = document.NewItemPriced(new Random().Next().ToString(), name, vatCode, money, count);            //создание по цене
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

        /// <summary>
        /// Добавляет в чек оплату.
        /// </summary>
        /// <param name="document">Чек в который необходимо добавить товар</param>
        /// <param name="receiptKind">Тип чека (Приход, Отмена прихода..)</param>
        /// <param name="tenderCode">Тип оплаты</param>
        /// <param name="sum">Сумма оплаты</param>
        void AddPayment(Fw16.Ecr.Receipt document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            document.AddPayment(tenderCode, sum);                                                   //добавление оплаты 

            registersTmp[this.receiptKind[receiptKind]] += sum;                                     //добавление в регистры (1-4) суммы по типу операции
            registersTmp[this.receiptKind[receiptKind] * 10 + 1 + (int)tenderCode] += sum;          //добавление в регистры (11-18, 21-28, 31-38, 41-48) суммы по номеру платежа
            registersTmp[this.receiptKind[receiptKind] * 10 + 1 + 8] += sum;                        //добавление в регистры (19, 29, 39, 49) суммы электрооного типа платежа

            registersTmp[(int)tenderCode + 172] += sum;                                             //добавление в регистры (172-179) суммы открытого документа по номеру платежа
            if (tenderCode != Native.CmdExecutor.TenderCode.Cash) registersTmp[181] += sum;           //добавление в регистр (181) суммы открытого документа электронного типа платежа
            else registersTmp[180] += sum;                                                          //добавление в регистр (180) суммы открытого документа наличного типа платежа

            registersTmp[this.receiptKind[receiptKind] + 190] += sum;                               //добавление в регистры (191-194) накопительный регистр по типу операции
        }

        void AddTender(Fw16.Ecr.Correction document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            document.AddTender(tenderCode, sum);
            registersTmp[tenderCodeType[tenderCode] + this.receiptKind[receiptKind] * 10 + 41] += sum;                                                                                    //добавление в регистры (51-55,71-75) суммы по типу платежа
            registersTmp[this.receiptKind[receiptKind] + 4] += sum;
        }

        void AddAmount(Fw16.Ecr.Correction document, ReceiptKind receiptKind, VatCode vatCode, decimal sum)
        {
            document.AddAmount(vatCode, sum);

            registersTmp[(this.receiptKind[receiptKind] == 3 ? 10 : 0) + this.vatCode2[vatCode] + 60] += sum;                                                                       //добавление в регистры (60-65,80-85) суммы по ставкам НДС
            switch (this.vatCode2[vatCode])
            {
                case 1: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum * 18m / 118m); break;                                     //добавление в регистры (66,86) суммы НДС
                case 5: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum / 11m); break;                                            //добавление в регистры (68,88) суммы НДС
                case 2: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 18m / 118m); break;                                 //добавление в регистры (67,87) суммы НДС
                case 6: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum / 11m); break;                                        //добавление в регистры (69,89) суммы НДС
                default:
                    break;
            }
        }
    }
}

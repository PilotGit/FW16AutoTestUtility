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

        /// <summary>
        /// Соответствие типа НДС его номеру
        /// </summary>
        private Dictionary<Native.CmdExecutor.VatCodeType, int> vatCode = new Dictionary<Native.CmdExecutor.VatCodeType, int>() {
                { Native.CmdExecutor.VatCodeType.Vat18,1 },
                { Native.CmdExecutor.VatCodeType.Vat10,2 },
                { Native.CmdExecutor.VatCodeType.Vat0,3 },
                { Native.CmdExecutor.VatCodeType.NoVat,4 },
                { Native.CmdExecutor.VatCodeType.Vat18Included,5 },
                { Native.CmdExecutor.VatCodeType.Vat10Included,6 },
            };

        /// <summary>
        /// Соответствие типа НДС его номеру
        /// </summary>
        private Dictionary<VatCode, int> vatCode2 = new Dictionary<VatCode, int>() {
                { VatCode.Vat18,1 },
                { VatCode.Vat10,2 },
                { VatCode.Vat0,3 },
                { VatCode.NoVat,4 },
                { VatCode.Vat18Included,5 },
                { VatCode.Vat10Included,6 },
            };

        /// <summary>
        /// Соответствие типа оплаты товара его номеру
        /// </summary>
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

        /// <summary>
        /// Соответствие типа чека его номеру
        /// </summary>
        private Dictionary<ReceiptKind, int> receiptKind = new Dictionary<ReceiptKind, int>
            {
                {ReceiptKind.Income,1 },
                {ReceiptKind.IncomeBack,2 },
                {ReceiptKind.Outcome,3 },
                {ReceiptKind.OutcomeBack,4}
            };

        /// <summary>
        /// Соответствие типа по номеру платежа его номеру
        /// </summary>
        private Dictionary<Native.CmdExecutor.TenderType, int> tenderType = new Dictionary<Native.CmdExecutor.TenderType, int>
            {
                {Native.CmdExecutor.TenderType.Cash,0 },
                {Native.CmdExecutor.TenderType.NonCash,1 },
                {Native.CmdExecutor.TenderType.Advance,2 },
                {Native.CmdExecutor.TenderType.Credit,3 },
                {Native.CmdExecutor.TenderType.Barter,4 }
            };

        /// <summary>
        /// Соответствие типа по номеру платежа его типу(электронные, аванс)
        /// </summary>
        private Dictionary<Native.CmdExecutor.TenderCode, int> tenderCodeType;

        /// <summary>
        /// Соответствие типа нефискльного документа его номеру в ККТ
        /// </summary>
        private Dictionary<Native.CmdExecutor.NFDocType, int> nfDocType = new Dictionary<Native.CmdExecutor.NFDocType, int>
        {
            {Native.CmdExecutor.NFDocType.Income,1 },
            {Native.CmdExecutor.NFDocType.Outcome,2 },
            {Native.CmdExecutor.NFDocType.Report,3 }
        };

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
            if (coast) receiptEntry = document.NewItemCosted(name, name, count, vatCode, money);                                //создание по стоимости
            else receiptEntry = document.NewItemPriced(new Random().Next().ToString(), name, vatCode, money, count);            //создание по цене
            receiptEntry.PaymentKind = paymentKind;                                                                             //спооб рассчёта
            receiptEntry.Kind = kind;                                                                                           //тип добавляемого товара
            document.AddEntry(receiptEntry);                                                                                    //добавления товара в чек

            registersTmp[(this.receiptKind[receiptKind] - 1) * 10 + this.vatCode[vatCode] - 1 + 120] += receiptEntry.Cost;      //добаление в регистр (120-125,130-135,140-145,150-155) суммы по ставке НДС
            if (this.vatCode[vatCode] != 3 && this.vatCode[vatCode] != 4)                                                       //проверка на нулевые ставки НДС
                registersTmp[(this.receiptKind[receiptKind] - 1) * 10 + (this.vatCode[vatCode] > 4 ? this.vatCode[vatCode] - 2 : this.vatCode[vatCode]) + 120 + 5] += receiptEntry.VatAmount;   //добавление в регистр (126-129,136-139,146-149,156-159) суммы НДС 
            registersTmp[this.receiptKind[receiptKind] * 10 + this.paymentKind[paymentKind] + 190] += receiptEntry.Cost;        //добавление в регистр (20-206, 210-216, 220-226, 230-236) суммы по способу рассчёта 

            registersTmp[160] += receiptEntry.Cost;                                                                             //добавление в регистр (160) суммы открытого документа
            registersTmp[this.vatCode[vatCode] + 160] += receiptEntry.Cost;                                                     //добавление в регситр (161-166) сумма открытого документа по ставкам НДС
            if (this.vatCode[vatCode] != 3 && this.vatCode[vatCode] != 4)
                registersTmp[(this.vatCode[vatCode] > 4 ? this.vatCode[vatCode] - 2 : this.vatCode[vatCode]) + 160 + 6] += receiptEntry.VatAmount;                                              //добавление в регситр (167-170) суммы НДС открытого документа 
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
            document.AddPayment(tenderCode, sum);                                                                                                                               //добавление оплаты 

            registersTmp[this.receiptKind[receiptKind]] += sum;                                                                                                                 //добавление в регистры (1-4) суммы по типу операции
            registersTmp[this.receiptKind[receiptKind] * 10 + 1 + (int)tenderCode] += sum;                                                                                      //добавление в регистры (11-18, 21-28, 31-38, 41-48) суммы по номеру платежа
            if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[this.receiptKind[receiptKind] * 10 + 1 + 8] += sum;     //добавление в регистры (19, 29, 39, 49) суммы электрооного типа платежа

            registersTmp[(int)tenderCode + 172] += sum;                                                                                                                         //добавление в регистры (172-179) суммы открытого документа по номеру платежа
            switch (this.tenderCodeType[tenderCode])
            {
                case 1: registersTmp[181] += sum; break;                                                                                                                        //добавление в регистр (181) суммы открытого документа электронного типа платежа
                case 0: registersTmp[180] += sum; break;                                                                                                                        //добавление в регистр (180) суммы открытого документа наличного типа платежа

                default:
                    break;
            }
            /*
            if (this.tenderCodeType[tenderCode] == tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[181] += sum;             //добавление в регистр (181) суммы открытого документа электронного типа платежа
            else registersTmp[180] += sum;                                                                                                  //добавление в регистр (180) суммы открытого документа наличного типа платежа
            */
            registersTmp[this.receiptKind[receiptKind] + 190] += sum;                                                                                                                   //добавление в регистры (191-194) накопительный регистр по типу операции
        }

        /// <summary>
        /// Добавление суммы по типу оплаты.
        /// </summary>
        /// <param name="document">Чек коррекции</param>
        /// <param name="receiptKind">Тип чека (Приход, изъятие)</param>
        /// <param name="tenderCode">Тип оплаты</param>
        /// <param name="sum">Сумма</param>
        void AddTender(Fw16.Ecr.Correction document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            document.AddTender(tenderCode, sum);
            registersTmp[tenderCodeType[tenderCode] + this.receiptKind[receiptKind] * 10 + 41] += sum;                                                                                  //добавление в регистры (51-55,71-75) суммы по типу платежа
            registersTmp[this.receiptKind[receiptKind] + 4] += sum;                                                                                                                     //добавление в регистры (5,7) суммы по типу чека коррекции
        }

        /// <summary>
        /// Добавление суммы по типу оплаты.
        /// </summary>
        /// <param name="document">Нефискальный документ</param>
        /// <param name="nfDocType">Тип нефискального документа</param>
        /// <param name="tenderCode">Тип оплаты</param>
        /// <param name="sum">Сумма</param>
        void AddTender(Fw16.Ecr.NonFiscalBase document, Native.CmdExecutor.NFDocType nfDocType, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            var tender = new Tender
            {
                Amount = sum,
                Code = tenderCode
            };
            document.AddTender(tender);

            registersTmp[this.nfDocType[nfDocType] + 8] += sum;                                                                                                                             //добавление в регистры (9,10) суммы по типу нефискального документа
            registersTmp[(int)tenderCode +this.nfDocType[nfDocType]*10+ 81] += sum;                                                                                                         //добавление в регистры (91-98,101-108) суммы по номеру платежа
            if(this.tenderCodeType[tenderCode]==this.tenderType[Native.CmdExecutor.TenderType.NonCash])registersTmp[this.nfDocType[nfDocType] * 10 + 89] += sum;                            //добавление в регистры (99,109) суммы электронных типов платежей

            registersTmp[(int)tenderCode + 111] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum;                                                                           //добавление в регистры (111,118) суммы по номеру платежа
            if (this.tenderCodeType[tenderCode]==this.tenderType[Native.CmdExecutor.TenderType.NonCash])registersTmp[119] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum; //добавление в регистры (119) суммы электронных типов платежей
        }

        /// <summary>
        /// Добавление суммы по ставке НДС.
        /// </summary>
        /// <param name="document">Чек коррекции</param>
        /// <param name="receiptKind">Тип чека (Приход, изъятие)</param>
        /// <param name="vatCode">Ставка НДС</param>
        /// <param name="sum">Сумма</param>
        void AddAmount(Fw16.Ecr.Correction document, ReceiptKind receiptKind, VatCode vatCode, decimal sum)
        {
            document.AddAmount(vatCode, sum);

            registersTmp[this.receiptKind[receiptKind]*10 + this.vatCode2[vatCode] + 50] += sum;                                                                    //добавление в регистры (60-65,80-85) суммы по ставкам НДС
            switch (this.vatCode2[vatCode])
            {
                case 1: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum * 18m / 118m,2); break;            //добавление в регистры (66,86) суммы НДС
                case 5: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum *10m/ 110m,2); break;              //добавление в регистры (68,88) суммы НДС
                case 2: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 18m / 118m,2); break;        //добавление в регистры (67,87) суммы НДС
                case 6: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 10m/ 110m,2); break;         //добавление в регистры (69,89) суммы НДС
                default:
                    break;
            }
        }

    }
}
/*
регистры 111-119 нигде не учитываются
     */
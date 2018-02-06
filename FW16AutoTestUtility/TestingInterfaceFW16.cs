﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fw16;
using Fw16.Ecr;
using Fw16.Model;


namespace FW16AutoTestUtility
{
    class TestingInterfaceFW16
    {
        /// <summary>
        /// Количество цен
        /// </summary>
        public const int countcosts = 2;
        /// <summary>
        /// Количество вариантов количеств
        /// </summary>
        public const int countCounts = 4;
        /// <summary>
        /// Количество типов оплаты
        /// </summary>
        public const int countItemPaymentKind = 6;
        /// <summary>
        /// Количество типов чеков
        /// </summary>
        public const int countReceiptKind = 4;
        /// <summary>
        /// Количество типов нефискальных документов
        /// </summary>
        public const int countNFDocType = 3;
        /// <summary>
        /// Количество типов оплаты
        /// </summary>
        public const int countTenderCode = 8;
        /// <summary>
        /// Количество ставок НДС
        /// </summary>
        public const int countVatCode = 6;
        /// <summary>
        /// Количество тпов добавления товара
        /// </summary>
        public const int countItemBy = 2;
        /// <summary>
        /// Длинна описания регистра или счётчика
        /// </summary>
        public const int lenStringDiscription = 50;
        /// <summary>
        /// Количество регистров
        /// </summary>
        public const ushort countRegisters = 236;
        /// <summary>
        /// Количество счётчиков
        /// </summary>
        public const ushort countCounters = 22;

        private string fileName;

        private int[] registersReciept = { 1, 2, 3, 4, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23, 24, 25, 26, 27, 28, 29, 31, 32, 33, 34, 35, 36, 37, 38, 39, 41, 42, 43, 44, 45, 46, 47, 48, 49, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 200, 201, 202, 203, 204, 205, 206, 210, 211, 212, 213, 214, 215, 216, 220, 221, 223, 224, 225, 226, 230, 231, 232, 233, 234, 235, 236 };
        private int[] registersCorrection = { 5, 7, 51, 52, 53, 54, 55, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 72, 73, 74, 75, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, };
        private int[] registersNFDoc = { 9, 10, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 102, 103, 104, 105, 106, 107, 108, 109 };
        private int[] registersСumulative = { 191, 192, 193, 194 };

        public int[] RegistersReciept { get => registersReciept; }
        public int[] RegistersCorrection { get => registersCorrection; }
        public int[] RegistersNFDoc { get => registersNFDoc; }
        public int[] RegistersСumulative { get => registersСumulative; }

        public EcrCtrl ecrCtrl;
        public uint versionFFD = 0;                                                 //Версия ФФД
        decimal[] registersTmp = new decimal[countRegisters + 1];                   //массив временных регистров
        public decimal[] registers = new decimal[countRegisters + 1];               //массив регистров
        public int[] counters = new int[countCounters + 1];                           //массив счётчиков
        public List<int> inaccessibleRegisters = new List<int>();                   //недоступные регистры
        private Random random = new Random();

        public enum ItemBy
        {
            price = 0,
            cost = 1
        }

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
        public Dictionary<Native.CmdExecutor.TenderCode, int> tenderCodeType;

        /// <summary>
        /// Соответствие типа нефискльного документа его номеру в ККТ
        /// </summary>
        private Dictionary<Native.CmdExecutor.NFDocType, int> nfDocType = new Dictionary<Native.CmdExecutor.NFDocType, int>
        {
            {Native.CmdExecutor.NFDocType.Income,1 },
            {Native.CmdExecutor.NFDocType.Outcome,2 },
            {Native.CmdExecutor.NFDocType.Report,3 }
        };

        public TestingInterfaceFW16(out EcrCtrl ecrCtrl)
        {
            fileName = $"log\\log{DateTime.Now.ToString("ddMMyy")}.log";
            if (!Directory.Exists("log")) { Directory.CreateDirectory("log"); }
            if (!File.Exists(fileName)) File.Create(fileName).Close();

            this.ecrCtrl = ecrCtrl = new EcrCtrl();
            if (ConnectToFW() == 0)
            {
                tenderCodeType = new Dictionary<Native.CmdExecutor.TenderCode, int>();
                var tenderList = ecrCtrl.Info.GetTendersList().GetEnumerator();                                         //получение коллекции соответствий кода платежа типу платежа

                Log("Получено соответствие номеров платежа и типов");

                for (int i = 0; i < countTenderCode; i++)
                {
                    tenderList.MoveNext();                                                                              //перебор коллекции
                    tenderCodeType.Add((Native.CmdExecutor.TenderCode)i, tenderType[tenderList.Current.Mode]);          //создание соответствия кода платежа типу 

                    Log($"|{i,3}|{tenderList.Current.Mode,10}|");
                }
            }
        }

        /// <summary>
        /// Подключение к ККТ
        /// </summary>
        /// <param name="serialPort">Порт по покотору производится поключение к ККТ</param>
        /// <param name="baudRate">Частота подключения</param>
        int ConnectToFW(int serialPort, int baudRate = 57600)
        {
            try
            {
                ecrCtrl.Init(serialPort, baudRate);             //Подключчение по порту и частоте
                Console.WriteLine($"Произведено подключение к ККТ. Port={serialPort} Rate={baudRate}");
                Log($"Произведено подключение к ККТ.\n" +
                    $" Port={serialPort}\n" +
                    $" Rate={baudRate}");
                ShowInformation();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.Message);                 //вывод ошибки доступа порта
                Log($"Error! Не удалось подключиться к ККТ.\n" +
                    $" Message={excep.Message}");
                return 1;
            }
            return 0;
        }

        int ConnectToFW()
        {
            try
            {
                ecrCtrl.Init();             //Подключчение по порту и частоте
                Console.WriteLine($"Произведено подключение к ККТ. Port=default Rate=default");
                Log($"Произведено подключение к ККТ.\n" +
                    $" Port=default\n" +
                    $" Rate=default");
                ShowInformation();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.Message);                 //вывод ошибки доступа порта
                Log($"Error! Не удалось подключиться к ККТ.\n" +
                    $" Message={excep.Message}");
                return 1;
            }
            return 0;
        }

        void ShowInformation()
        {
            versionFFD = ecrCtrl.Info.FfdVersion;
            Console.WriteLine("Версия прошивки: " + ecrCtrl.Info.FactoryInfo.FwBuild +
                "\nКод firmware: " + ecrCtrl.Info.FactoryInfo.FwType +
                "\nСерийный номер ККТ: " + ecrCtrl.Info.EcrInfo.Id +
                "\nМодель: " + ecrCtrl.Info.EcrInfo.Model +
                "\nФФД: v" + versionFFD);
            Log("Версия прошивки: " + ecrCtrl.Info.FactoryInfo.FwBuild +
                "\nКод firmware: " + ecrCtrl.Info.FactoryInfo.FwType +
                "\nСерийный номер ККТ: " + ecrCtrl.Info.EcrInfo.Id +
                "\nМодель: " + ecrCtrl.Info.EcrInfo.Model +
                "\nФФД: v" + versionFFD);
        }


        public void OpenShift(string nameOperator)
        {
            try
            {
                ecrCtrl.Shift.Open(nameOperator);
                Log($"\tСмена открыта");
            }
            catch (Exception ex)
            {
                Log($"\tError! Не удалось открыть смену.\n" +
                    $"\t Exception={ex.Message}");
            }
        }

        internal void CloseShift(string nameOperator)
        {
            try
            {
                ecrCtrl.Shift.Close(nameOperator);
                Log($"\tСмена закрыта");
            }
            catch (Exception ex)
            {
                Log($"\tError! Не удалось закрыт смену.\n" +
                    $"\t Exception={ex.Message}");
            }

        }

        /// <summary>
        /// Открывает нефискальный документ
        /// </summary>
        /// <param name="document">Нефискальный документ, который следует открыть</param>
        /// <param name="nfDocType">Тип нефискального документа</param>
        public void StartDocument(out NonFiscalBase document, Native.CmdExecutor.NFDocType nfDocType)
        {
            try
            {
                document = ecrCtrl.Shift.BeginNonFiscal(nfDocType); //открытие нефиксального документа
                Log($"\t\tНефискальный документ открыт.\n" +
                    $"\t\t Type={nfDocType}");
            }
            catch (Exception ex)
            {
                Log($"\t\tError! Не удалось открыть нефискальный документ.\n" +
                    $"\t\t Type={nfDocType}\n" +
                    $"\t\t Exception={ex.Message}");
                document = null;
            }
            SetValue(registersTmp, 0);
        }

        /// <summary>
        /// Открывает чек коррекции
        /// </summary>
        /// <param name="document">Чек коррекции который следует открыть</param>
        /// <param name="nameOerator">Имя оператора</param>
        /// <param name="receiptKind">Тип чека коррекции</param>
        public void StartDocument(out Correction document, string nameOerator, ReceiptKind receiptKind)
        {
            try
            {
                document = ecrCtrl.Shift.BeginCorrection(nameOerator, receiptKind);
                Log($"\t\tЧек коррекции открыт.\n" +
                    $"\t\t Type={receiptKind}");
            }
            catch (Exception ex)
            {
                Log($"\t\tError! Не удалось открыть чек коррекции.\n" +
                    $"\t\t Kind={receiptKind}\n" +
                    $"\t\t Exception={ex.Message}");
                document = null;
            }
            SetValue(registersTmp, 0);
        }

        /// <summary>
        /// Открывает чек
        /// </summary>
        /// <param name="document">Чек который следует открыть</param>
        /// <param name="nameOerator">Имя оператора</param>
        /// <param name="receiptKind">Тип чека</param>
        public void StartDocument(out Receipt document, string nameOerator, ReceiptKind receiptKind)
        {
            try
            {
                document = ecrCtrl.Shift.BeginReceipt(nameOerator, receiptKind, new
                {
                    Taxation = Fs.Native.TaxationType.Agro,         //налогообложение по умолчанию
                    CustomerAddress = "qwe@ewq.xxx",                //адрес получателя
                    SenderAddress = "ewq@qwe.yyy"                   //адрес отправтеля
                });
                Log($"\t\tЧек открыт.\n" +
                    $"\t\t Operator={nameOerator}\n" +
                    $"\t\t Type={receiptKind}");
            }
            catch (Exception ex)
            {
                Log($"\t\tError! Не удалось открыть чек.\n" +
                    $"\t\t Kind={receiptKind}\n" +
                    $"\t\t Operator={nameOerator}\n" +
                    $"\t\t Exception={ex.Message}");
                document = null;
            }
            SetValue(registers, 0, 160, 182);
            SetValue(registersTmp, 0);
        }

        /// <summary>
        /// Закрывает нефискальный документ
        /// </summary>
        /// <param name="document">Нефискальный документ</param>
        /// <param name="nfDocType">Тип нефискального документа</param>
        /// <param name="abort">Отмена чека</param>
        public int DocumentComplete(NonFiscalBase document, Native.CmdExecutor.NFDocType nfDocType, bool abort)
        {
            if (abort)
            {
                try
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);                //отмена нефискального документа
                    Console.WriteLine("Отменён нефиксальный документ типа " + nfDocType + "");      //логирование
                    Log($"\t\tНефискальный документ отменён.\n" +
                        $"---------------------------------------------------");
                    counters[this.nfDocType[nfDocType] + 8 + 11]++;
                }
                catch (Exception ex)
                {
                    Log($"\t\tError! Не удалось отменить нефискальный документ.\n" +
                        $"\t\t Exception={ex.Message}" +
                        $"---------------------------------------------------");
                    document = null;
                }
            }
            else
            {
                try
                {
                    document.Complete(Native.CmdExecutor.DocEndMode.Default);                       //закрытие нефиксального документа
                    Console.WriteLine("Оформлен нефиксальный документ типа " + nfDocType + "");     //логирование
                    Log($"\t\tНефискальный докумен оформлент.\n" +
                        $"---------------------------------------------------");
                    counters[this.nfDocType[nfDocType] + 8]++;
                    AddRegistersTmp();
                }
                catch (Exception ex)
                {
                    Log($"\t\tError! Не удалось оформить нефискальный документ.\n" +
                        $"\t\t Exception={ex.Message}" +
                        $"---------------------------------------------------");
                }
            }
            //return RequestRegisters(111, 120);
            return RequestRegisters(RegistersNFDoc);

        }

        /// <summary>
        /// Заверает чек
        /// </summary>
        /// <param name="document">чек который следует завершить</param>
        /// <param name="receiptKind">Тип чека коррекции</param>
        /// <param name="abort">Отменить документ</param>
        public int DocumentComplete(Receipt document, ReceiptKind receiptKind, bool abort)
        {
            if (abort)
            {
                try
                {
                    document.Abort();                                                               //отмена документа
                    Console.WriteLine("Отменён чек типа " + receiptKind + "");                      //логирование
                    Log($"\t\tЧек отменён.\n" +
                        $"---------------------------------------------------");
                    counters[this.receiptKind[receiptKind] + 11]++;                                 //увеличение счётчика (12-15) отмены по типу чека
                }
                catch (Exception ex)
                {
                    Log($"\t\tError! Не удалось отменить чек.\n" +
                        $"\t\t Exception={ex.Message}" +
                        $"---------------------------------------------------");
                    document = null;
                }
            }
            else
            {
                try
                {
                    document.Complete();                                                            //закрытие чека
                    Console.WriteLine("Оформлен чек типа " + receiptKind + "");                     //логирование
                    Log($"\t\tЧек оформлен.\n" +
                        $"---------------------------------------------------");
                    counters[this.receiptKind[receiptKind]]++;                                      //учеличение счётчика (1-4) оформления по типу чека
                    SetValue(registers, 0, 160, 182);
                    AddRegistersTmp();
                }
                catch (Exception ex)
                {
                    Log($"\t\tError! Не удалось оформить чек.\n" +
                        $"\t\t Exception={ex.Message}" +
                        $"---------------------------------------------------");
                    document = null;
                }
            }
            //return RequestRegisters(160, 182); //+ RequestRegisters(111, 120);
            return RequestRegisters(RegistersReciept);
        }

        /// <summary>
        /// Заверает чек коррекции
        /// </summary>
        /// <param name="document">Документ который следует завершить</param>
        /// <param name="receiptKind">Тип чека коррекции</param>
        /// <param name="abort">Отменить документ</param>
        public int DocumentComplete(Correction document, ReceiptKind receiptKind, bool abort)
        {
            if (abort)
            {
                try
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);
                    Console.WriteLine("Отменён чек коррекции типа " + receiptKind + "");            //логирование
                    Log($"\t\tЧек коррекции отменён.\n" +
                        $"---------------------------------------------------");
                    counters[this.receiptKind[receiptKind] + 4 + 11]++;
                }
                catch (Exception ex)
                {
                    Log($"\t\tError! Не удалось отменить чек коррекции.\n" +
                        $"\t\t Exception={ex.Message}" +
                        $"---------------------------------------------------");
                    document = null;
                }
            }
            else
            {
                try
                {
                    document.Complete();                                                            //закрытие чека коррекции
                    Console.WriteLine("Оформлен чек коррекции типа " + receiptKind + "");           //логирование
                    Log($"\t\tЧек коррекции оформлен.\n" +
                        $"---------------------------------------------------");
                    counters[this.receiptKind[receiptKind] + 4]++;
                    AddRegistersTmp();
                }
                catch (Exception ex)
                {
                    Log($"\t\tError! Не удалось оформить чек коррекции.\n" +
                        $"\t\t Exception={ex.Message}" +
                        $"---------------------------------------------------");
                    document = null;
                }
            }
            //return RequestRegisters(111, 120);
            return RequestRegisters(RegistersCorrection);
        }

        /// <summary>
        /// Создаёт и добавляет товар в чек. Записывает суммы во временный регистр.
        /// </summary>
        /// <param name="document">Чек в который необходимо добавить товар</param>
        /// <param name="receiptKind">Тип чека (Приход, Отмена прихода..)</param>
        /// <param name="name">Название товара</param>
        /// <param name="count">Количество товара</param>
        /// <param name="vatCode">Тип налоговой ставки</param>
        /// <param name="itemBy">true - параметр money - стоимость, false - цена </param>
        /// <param name="money">Сумма</param>
        /// <param name="paymentKind">Способ рассчёта (Предоплата, полная оплата, кредит..)</param>
        /// <param name="kind">Тип добавляемого товара (товар,услуга..)</param>
        public void AddEntry(Receipt document, ReceiptKind receiptKind, string name, decimal count, Native.CmdExecutor.VatCodeType vatCode, ItemBy itemBy, decimal money, ItemPaymentKind paymentKind = ItemPaymentKind.Payoff, ItemFlags kind = ItemFlags.Regular)
        {
            string code = random.Next().ToString();
            try
            {
                ReceiptEntry receiptEntry;                                                                                                  //товар
                if (itemBy == ItemBy.cost) receiptEntry = document.NewItemCosted(code, name, count, vatCode, money);                        //создание по стоимости
                else receiptEntry = document.NewItemPriced(code, name, vatCode, money, count);                                              //создание по цене
                if (ecrCtrl.Info.FfdVersion >= 2)
                {
                    receiptEntry.PaymentKind = paymentKind;                                                                                     //спооб рассчёта
                    receiptEntry.Kind = kind;                                                                                                   //тип добавляемого товара
                }
                document.AddEntry(receiptEntry);                                                                                            //добавления товара в чек

                Log($"\t\t\tТовар добавлен\n" +
                    $"\t\t\t {code,15}|{name,12}|{itemBy,6}|{count,7}|{money,8}|{vatCode,15}");

                registersTmp[(this.receiptKind[receiptKind] - 1) * 10 + this.vatCode[vatCode] - 1 + 120] += receiptEntry.Cost;              //добаление в регистр (120-125,130-135,140-145,150-155) суммы по ставке НДС
                if (this.vatCode[vatCode] != 3 && this.vatCode[vatCode] != 4)                                                               //проверка на нулевые ставки НДС
                    registersTmp[(this.receiptKind[receiptKind] - 1) * 10 + (this.vatCode[vatCode] > 4 ? this.vatCode[vatCode] - 2 : this.vatCode[vatCode]) + 120 + 5] += receiptEntry.VatAmount;   //добавление в регистр (126-129,136-139,146-149,156-159) суммы НДС 
                registersTmp[this.receiptKind[receiptKind] * 10 + this.paymentKind[paymentKind] + 190] += receiptEntry.Cost;                //добавление в регистр (20-206, 210-216, 220-226, 230-236) суммы по способу рассчёта 

                registersTmp[160] += receiptEntry.Cost;                                                                                     //добавление в регистр (160) суммы открытого документа
                registersTmp[this.vatCode[vatCode] + 160] += receiptEntry.Cost;                                                             //добавление в регситр (161-166) сумма открытого документа по ставкам НДС
                if (this.vatCode[vatCode] != 3 && this.vatCode[vatCode] != 4)
                    registersTmp[(this.vatCode[vatCode] > 4 ? this.vatCode[vatCode] - 2 : this.vatCode[vatCode]) + 160 + 6] += receiptEntry.VatAmount;                                              //добавление в регситр (167-170) суммы НДС открытого документа 
                registersTmp[171]++;                                                                                                        //Добавление в регситр (171)  количество товарных позиций
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить товар\n" +
                    $"\t\t\t {code,15}|{name,12}|{itemBy,5}|{count,7}|{money,8}|{vatCode,13}\n" +
                    $"\t\t\t Exception={ex.Message}");

            }
        }

        /// <summary>
        /// Добавляет в чек оплату.
        /// </summary>
        /// <param name="document">Чек в который необходимо добавить товар</param>
        /// <param name="receiptKind">Тип чека (Приход, Отмена прихода..)</param>
        /// <param name="tenderCode">Тип оплаты</param>
        /// <param name="sum">Сумма оплаты</param>
        public void AddPayment(Receipt document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            try
            {
                decimal balance = (document.Total - document.TotalaPaid);
                document.AddPayment(tenderCode, sum);                                                                                                                                       //добавление оплаты 

                Log($"\t\t\tОплата добавлена\n" +
                    $"\t\t\t {(int)tenderCode,3}|{(Native.CmdExecutor.TenderType)this.tenderCodeType[tenderCode],7}|{sum,8}");

                if (tenderCode == Native.CmdExecutor.TenderCode.Cash && balance < sum)                                                                                                      //учитывание сдачи при расплате наличными
                {
                    Log($"\t\t\t {(int)tenderCode,3}|{(Native.CmdExecutor.TenderType)this.tenderCodeType[tenderCode],7}|{balance - sum,8}");                                                //Логирование сдачи
                    sum = balance;
                }

                registersTmp[this.receiptKind[receiptKind]] += sum;                                                                                                                         //добавление в регистры (1-4) суммы по типу операции
                registersTmp[this.receiptKind[receiptKind] * 10 + 1 + (int)tenderCode] += sum;                                                                                              //добавление в регистры (11-18, 21-28, 31-38, 41-48) суммы по номеру платежа
                if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[this.receiptKind[receiptKind] * 10 + 1 + 8] += sum;             //добавление в регистры (19, 29, 39, 49) суммы электрооного типа платежа

                registersTmp[(int)tenderCode + 172] += sum;                                                                                                                                 //добавление в регистры (172-179) суммы открытого документа по номеру платежа
                switch (this.tenderCodeType[tenderCode])
                {
                    case 1: registersTmp[181] += sum; break;                                                                                                                                //добавление в регистр (181) суммы открытого документа электронного типа платежа
                    case 0: registersTmp[180] += sum; break;                                                                                                                                //добавление в регистр (180) суммы открытого документа наличного типа платежа
                    default:
                        break;
                }
                registersTmp[(int)tenderCode + 111] += this.receiptKind[receiptKind] % 3 == 1 ? sum : -sum;                                                                                  //добавление в регистры (111-118) суммы по номеру платежа
                if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[119] += this.receiptKind[receiptKind] % 3 == 1 ? sum : -sum;     //добавление в регистр (119) суммы электрооного типа платежа

                registersTmp[this.receiptKind[receiptKind] + 190] += sum;                                                                                                                   //добавление в регистры (191-194) накопительный регистр по типу операции
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить оплату\n" +
                    $"\t\t\t {(int)tenderCode,3}|{(Native.CmdExecutor.TenderType)this.tenderCodeType[tenderCode],7}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Добавление суммы по типу оплаты.
        /// </summary>
        /// <param name="document">Чек коррекции</param>
        /// <param name="receiptKind">Тип чека (Приход, изъятие)</param>
        /// <param name="tenderCode">Тип оплаты</param>
        /// <param name="sum">Сумма</param>
        public void AddTender(Correction document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            try
            {
                document.AddTender(tenderCode, sum);

                Log($"\t\t\tСумма коррекции добавлена\n" +
                        $"\t\t\t {(int)tenderCode,3}|{(Native.CmdExecutor.TenderType)this.tenderCodeType[tenderCode],7}|{sum,8}");

                registersTmp[tenderCodeType[tenderCode] + this.receiptKind[receiptKind] * 10 + 41] += sum;                                                                                  //добавление в регистры (51-55,71-75) суммы по типу платежа
                registersTmp[this.receiptKind[receiptKind] + 4] += sum;                                                                                                                     //добавление в регистры (5,7) суммы по типу чека коррекции

                registersTmp[(int)tenderCode + 111] += this.receiptKind[receiptKind] % 3 == 1 ? sum : -sum;                                                                                  //добавление в регистры (111-118) суммы по номеру платежа
                if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[119] += this.receiptKind[receiptKind] % 3 == 1 ? sum : -sum;     //добавление в регистры (119) суммы электрооного типа платежа

                //registersTmp[this.receiptKind[receiptKind] + 190] += sum;                                                                                                                   //добавление в регистры (191-194) накопительный регистр по типу операции
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить сумму коррекции\n" +
                    $"\t\t\t {(int)tenderCode,3}|{(Native.CmdExecutor.TenderType)this.tenderCodeType[tenderCode],7}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Добавление суммы по типу оплаты.
        /// </summary>
        /// <param name="document">Нефискальный документ</param>
        /// <param name="nfDocType">Тип нефискального документа</param>
        /// <param name="tenderCode">Тип оплаты</param>
        /// <param name="sum">Сумма</param>
        public void AddTender(NonFiscalBase document, Native.CmdExecutor.NFDocType nfDocType, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            try
            {
                var tender = new Tender
                {
                    Amount = sum,
                    Code = tenderCode
                };
                document.AddTender(tender);

                Log($"\t\t\tСумма добавлена\n" +
                    $"\t\t\t {(int)tenderCode,3}|{(Native.CmdExecutor.TenderType)this.tenderCodeType[tenderCode],7}|{sum,8}");

                registersTmp[this.nfDocType[nfDocType] + 8] += sum;                                                                                                                                 //добавление в регистры (9,10) суммы по типу нефискального документа
                registersTmp[(int)tenderCode + this.nfDocType[nfDocType] * 10 + 81] += sum;                                                                                                         //добавление в регистры (91-98,101-108) суммы по номеру платежа
                if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[this.nfDocType[nfDocType] * 10 + 89] += sum;                            //добавление в регистры (99,109) суммы электронных типов платежей

                registersTmp[(int)tenderCode + 111] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum;                                                                               //добавление в регистры (111,118) суммы по номеру платежа
                if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[119] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum;  //добавление в регистры (119) суммы электронных типов платежей
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить сумму\n" +
                    $"\t\t\t {(int)tenderCode,3}|{this.tenderCodeType[tenderCode],7}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Добавление суммы по ставке НДС.
        /// </summary>
        /// <param name="document">Чек коррекции</param>
        /// <param name="receiptKind">Тип чека (Приход, изъятие)</param>
        /// <param name="vatCode">Ставка НДС</param>
        /// <param name="sum">Сумма</param>
        public void AddAmount(Correction document, ReceiptKind receiptKind, VatCode vatCode, decimal sum)
        {
            try
            {
                document.AddAmount(vatCode, sum);

                Log($"\t\t\tСумма коррекции добавлена\n" +
                    $"\t\t\t {vatCode,13}|{sum,8}");

                registersTmp[this.receiptKind[receiptKind] * 10 + this.vatCode2[vatCode] + 49] += sum;                                                                      //добавление в регистры (60-65,80-85) суммы по ставкам НДС
                switch (this.vatCode2[vatCode])
                {
                    case 1: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum * 18m / 118m, 2); break;               //добавление в регистры (66,86) суммы НДС
                    case 5: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum * 10m / 110m, 2); break;               //добавление в регистры (68,88) суммы НДС
                    case 2: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 18m / 118m, 2); break;           //добавление в регистры (67,87) суммы НДС
                    case 6: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 10m / 110m, 2); break;           //добавление в регистры (69,89) суммы НДС
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить сумму коррекции\n" +
                    $"\t\t\t {vatCode,13}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Сверяет регистры с массивом регистров в указанном диапозоне
        /// </summary>
        /// <param name="startIndex">Начальный индекс</param>
        /// <param name="endIndex">Конечный индекс, не включительно</param>
        public int RequestRegisters(ushort startIndex = 1, ushort endIndex = 0)
        {
            //endIndex = endIndex > 0 ? endIndex : (ushort)(countRegisters + 1);                                                           //проверка конечного значения если 0, то до конца
            endIndex = countRegisters + 1;
            string err = $"Error!\n" +
                $"+-------+------------------+-------------------+\n" +
                $"|   #   |       test       |        ККТ        |\n" +
                $"+-------+------------------+-------------------+\n";                                                                                            //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i < endIndex; i++)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1)
                {
                    try
                    {
                        decimal tmp = ecrCtrl.Info.GetRegister(i);
                        if (tmp != registers[i]) { err += $"|{i,7:D}|{registers[i],18:F}|{tmp,19:F}|\n"; }//заполнение ошибки несоотвествия регистров
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Warning! Не удалось получить доступ к регистру №" + i + "");
                        Log($"Warning! Не удалось получить доступ к регистру №{i}");
                    }
                }
            }
            Console.Write(((err.Length > 310) ? err : ""));           //логирование
            Log($"Запрошеные данные с регистров с {startIndex} по {endIndex} {((err.Length > 310) ? "\n" + err : "")}");           //логирование
            if (err.Length > 310) return 1;
            return 0;
        }

        /// <summary>
        /// Сравнение значени программных регистров с значениями регистров ККТ
        /// </summary>
        /// <param name="arr">Массив номеров сравниваемых регистров</param>
        /// <returns></returns>
        public int RequestRegisters(int[] arr)
        {
            string err = $"Error!\n" +
                         $"+-------+--------------------------------------------------+------------------+-------------------+\n" +
                         $"|   #   |{"discription",lenStringDiscription}|       test       |        ККТ        |\n" +
                         $"+-------+--------------------------------------------------+------------------+-------------------+\n";                                                                                          //строка ошибки заполняемая при несоответсвии регистров
            foreach (ushort i in arr)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1)
                {
                    try
                    {
                        decimal tmp = ecrCtrl.Info.GetRegister(i);
                        if (tmp != registers[i])                                                                                                //Проверка расходения регистров
                        {
                            string discription = _GetDescription((Native.CmdExecutor.RegisterCode)i);                                           //Получение описания регистра
                            int startPosition = 0;                                                                                              //Стартовая позция вывода описания
                            err += $"|{i,7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{registers[i],18:F}|{tmp,19:F}|\n";   //Вывод первой строки описания
                            for (startPosition = lenStringDiscription; startPosition < discription.Length; startPosition += lenStringDiscription)
                            {
                                err += $"|{"",7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{"",18:F}|{"",19:F}|\n";         //Вывод последующих строк описания, если необходимо
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Warning! Не удалось получить доступ к регистру №" + i + "");
                        Log($"Warning! Не удалось получить доступ к регистру №{i}\n" +
                            $" Exception={ex.Message}");
                    }
                }
            }
            Console.Write(((err.Length > 310) ? err : ""));                                         //логирование
            Log($"Запрошеные данные с регистров{((err.Length > 310) ? "\n" + err : "")}");          //логирование
            if (err.Length > 310) return 1;
            return 0;
        }

        /// <summary>
        /// Сверяет счётчики с массивом счтчиков в указанном диапозоне
        /// </summary>
        /// <param name="startIndex">Начальный индекс</param>
        /// <param name="endIndex">Конечный индекс, не включительно</param>
        public void RequestCounters(ushort startIndex = 1, ushort endIndex = 0)
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)(countCounters + 1);                                                            //проверка конечного значения если 0, то до конца
            string err = $"Error!\n" +
                $"+-------+--------------------------------------------------+------------------+-------------------+\n" +
                $"|   #   |{"discription",lenStringDiscription}|       test       |        ККТ        |\n" +
                $"+-------+--------------------------------------------------+------------------+-------------------+\n";                                                                                              //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i < endIndex; i++)
            {
                try
                {
                    int tmp = ecrCtrl.Info.GetCounter(i);
                    if (tmp != counters[i])                                                                                                                 //Проверка расхождения счётчиков
                    {
                        string discription = _GetDescription((Native.CmdExecutor.CounterCode)i);                                                            //Получение описания счётчика
                        int startPosition = 0;                                                                                                              //Стартовая позиция с которой выводится строка описания
                        err += $"|{i,7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{counters[i],18:F}|{tmp,19:F}|\n";    //Вывод первой строки описания счётчика
                        for (startPosition = lenStringDiscription; startPosition < discription.Length; startPosition += lenStringDiscription)
                        {
                            err += $"|{"",7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{"",18:F}|{"",19:F}|\n";         //Вывод последующих строк описания счётчика, если необходимо
                        }

                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("Warning! Не удалось получить доступ к счётчику №" + i + "");                              //ошибка доступа к регистру
                    Log($"Warning! Не удалось получить доступ к счётчику №{i}");
                }
            }
            Console.Write(((err.Length > 310) ? err : ""));           //логирование
            Log($"Запрошеные данные с счётчиков с {startIndex} по {endIndex} {((err.Length > 310) ? "\n" + err : "")}");           //логирование
        }


        /// <summary>
        /// Обновление программных регистров данными из ККТ
        /// </summary>
        /// <param name="arr">Массив пропускаемых регистров</param>
        public void GetRegisters(int[] arr = null)
        {
            ushort endIndex = countRegisters + 1;
            ushort startIndex = 1;
            string err = "";
            if (arr == null) arr = new int[] { -1 };
            for (ushort i = startIndex; i < endIndex; i++)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1 && Array.IndexOf(arr, i) == -1)
                {
                    try
                    {
                        registers[i] = ecrCtrl.Info.GetRegister(i);             //запрос значений регистров из ККТ
                        Log($"Программный регистр №{i,4} получил значение {registers[i]}");
                    }
                    catch (Exception)
                    {
                        err += $"Warning! Не удалось получить получить значение регистра №{i}\n";
                        inaccessibleRegisters.Add(i);
                    }
                }
            }
            Console.WriteLine($"{err}Значения программных регистров обновлены данными из ККТ");     //логирование
            Log($"{err}Значения программных регистров обновлены данными из ККТ");
            if (Array.IndexOf(arr, -1) == -1)
            {
                string s = "";
                foreach (int i in arr)
                    s += i + " ";
                Console.WriteLine($" Были пропущены {s}");     //логирование
                Log($" Были пропущены {s}");
            }
        }

        /// <summary>
        /// Считывает все счтчики в массив счётчиков
        /// </summary>
        public void GetCounters()
        {
            ushort endIndex = countCounters + 1;
            ushort startIndex = 1;
            for (ushort i = startIndex; i < endIndex; i++)
            {
                try
                {
                    counters[i] = ecrCtrl.Info.GetCounter(i);               //запрос значений регистров из ККТ
                    Log($"Программный счётчик №{i,3} получил значение {counters[i]}");
                }
                catch (Exception)
                {
                    Console.WriteLine($"Warning! Не удалось получить получить значение счётчика №{i}");
                    Log($"Warning! Не удалось получить получить значение счётчика №{i}");
                }
            }
            Console.WriteLine($"Значения программных счётчиков обновлены данными из ККТ");     //логирование
            Log($"Значения программных счётчиков обновлены данными из ККТ");
        }

        /// <summary>
        /// Применяет изменения врменного регистра в основной
        /// </summary>
        public void AddRegistersTmp()
        {
            ushort endIndex = countRegisters + 1;
            ushort startIndex = 1;
            for (int i = startIndex; i < endIndex; i++)
            {
                registers[i] += registersTmp[i];                                                        //применение временного массива к конечному
            }
            Log($"Значения из времеенного программного регистра успешно применены");
        }

        /// <summary>
        /// Утсановка значения  для каждого элемента или в заданном диапозоне
        /// </summary>
        /// <param name="arr">Массив</param>
        /// <param name="value">Значение</param>
        /// <param name="startIndex">Индекс с которого надо заполнять массив значениям</param>
        /// <param name="endIndex">Конечный индекс заполнения, не включается</param>
        public void SetValue(decimal[] arr, decimal value, ushort startIndex = 0, ushort endIndex = 0)
        {
            endIndex = endIndex > 0 ? endIndex : (ushort)arr.Length;
            for (int i = startIndex; i < endIndex; i++)
            {
                arr[i] = value;
            }
        }

        /// <summary>
        /// Пишет лог
        /// </summary>
        /// <param name="message">Строка, записываемая в лог</param>
        private void Log(string message)
        {
            string[] messages = message.Split('\n');
            foreach (string i in messages)
            {
                File.AppendAllText(fileName, $"{DateTime.Now.ToString("HH:mm:ss.ffff")}\t{i}\r\n");
            }

        }

        internal static string _GetDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            if (field == null)
                return String.Format("<{0}>", value);

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute?.Description;

        }
    }
}

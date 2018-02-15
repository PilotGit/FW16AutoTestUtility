using System;
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
        public const int countItemPaymentKind = 6;  //на самом деле 7, но 7й выдаёт ошибку.
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
        /// Количество типов коррекции суммы
        /// </summary>
        public const int countAdjustment = 2;
        /// <summary>
        /// Длинна описания регистра или счётчика
        /// </summary>
        public const int lenStringDiscription = 50;
        /// <summary>
        /// Количество регистров
        /// </summary>
        public const ushort countRegisters = 296;
        /// <summary>
        /// Количество счётчиков
        /// </summary>
        public const ushort countCounters = 22;

        private string fileName;

        private readonly int[] registersReciept = { 1, 2, 3, 4, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23, 24, 25, 26, 27, 28, 29, 31, 32, 33, 34, 35, 36, 37, 38, 39, 41, 42, 43, 44, 45, 46, 47, 48, 111, 112, 113, 114, 115, 116, 117, 118, 119, 49, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 200, 201, 202, 203, 204, 205, 206, 210, 211, 212, 213, 214, 215, 216, 220, 221, 223, 224, 225, 226, 230, 231, 232, 233, 234, 235, 236 };
        private readonly int[] registersCorrection = { 5, 7, 51, 52, 53, 54, 55, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 72, 73, 74, 75, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 111, 112, 113, 114, 115, 116, 117, 118, 119, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181 };
        private readonly int[] registersNFDoc = { 9, 10, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 102, 103, 104, 105, 106, 107, 108, 109, 111, 112, 113, 114, 115, 116, 117, 118, 119 };
        private readonly int[] registersСumulative = { 191, 192, 193, 194, 195, 196, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 270, 271, 272, 273, 274, 275, 276, 280, 281, 282, 283, 284, 285, 286, 290, 291, 292, 293, 294, 295, 296 };
        private readonly int[] registersOpenReciept = { 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181 };

        /// <summary>
        /// регистры связаные с кассовым чеком
        /// </summary>
        public int[] RegistersReciept { get => registersReciept; }
        /// <summary>
        /// регистры связанные с чеком коррекции
        /// </summary>
        public int[] RegistersCorrection { get => registersCorrection; }
        /// <summary>
        /// регистры связанные с нефискальным документом
        /// </summary>
        public int[] RegistersNFDoc { get => registersNFDoc; }
        /// <summary>
        /// накопительные регистры
        /// </summary>
        public int[] RegistersСumulative { get => registersСumulative; }
        /// <summary>
        /// регистры открытого документа
        /// </summary>
        public int[] RegistersOpenReciept { get => registersOpenReciept; }

        /// <summary>
        /// Массив, хранящий данные последнего документа
        /// </summary>
        public decimal[] RegistersTmp { get => registersTmp; }
        /// <summary>
        /// Массив, хранящий данные последней смены 
        /// </summary>
        public decimal[] ControlRegisters { get => controlRegisters; }
        /// <summary>
        /// Массив, хранящий данные последней смены 
        /// </summary>
        decimal[] controlRegisters = new decimal[countRegisters + 1];
        /// <summary>
        /// Массив, хранящий данные последнего документа
        /// </summary>
        decimal[] registersTmp = new decimal[countRegisters + 1];                   
        /// <summary>
        /// Массив, хранящий данные текущего состояния регситров.
        /// </summary>
        decimal[] registers = new decimal[countRegisters + 1];       
        /// <summary>
        /// Массив, хранящий текущее состояние счётчиков
        /// </summary>
        int[] counters = new int[countCounters + 1]; 
        /// <summary>
        /// Список регистров недоступных при первом считовании
        /// </summary>
        public List<int> inaccessibleRegisters = new List<int>();         

        readonly EcrCtrl ecrCtrl;
        /// <summary>
        /// версия ФФД
        /// </summary>
        readonly uint versionFFD = 0;                       
        /// <summary>
        /// Счётчик считывания регистров
        /// </summary>
        public int countGetRegister = 0;

        /// <summary>
        /// Тип добавления товара
        /// </summary>
        public enum ItemBy
        {
            price = 0,
            cost = 1
        }
        /// <summary>
        /// Тип коррекции суммы
        /// </summary>
        public enum AdjustmentType
        {
            [Description("Скидка")]
            sale = 0,
            [Description("Наценка")]
            markup = 1
        }      

        private Random random = new Random();

        /// <summary>
        /// Соответствие типа НДС его номеру
        /// </summary>
        public static readonly List<Native.CmdExecutor.VatCodeType> vatCode = new List<Native.CmdExecutor.VatCodeType>() { 0, Native.CmdExecutor.VatCodeType.Vat18, Native.CmdExecutor.VatCodeType.Vat10, Native.CmdExecutor.VatCodeType.Vat0, Native.CmdExecutor.VatCodeType.NoVat, Native.CmdExecutor.VatCodeType.Vat18Included, Native.CmdExecutor.VatCodeType.Vat10Included, };

        /// <summary>
        /// Соответствие типа НДС его номеру
        /// </summary>
        public static readonly List<VatCode> vatCodeCorr = new List<VatCode> { 0, VatCode.Vat18, VatCode.Vat10, VatCode.Vat0, VatCode.NoVat, VatCode.Vat18Included, VatCode.Vat10Included };

        /// <summary>
        /// Соответствие типа оплаты товара его номеру
        /// </summary>
        public static readonly List<ItemPaymentKind> itemPaymentKind = new List<ItemPaymentKind> { 0, ItemPaymentKind.Prepay, ItemPaymentKind.PartlyPrepay, ItemPaymentKind.Advance, ItemPaymentKind.Payoff, ItemPaymentKind.PartlyLoanCredit, ItemPaymentKind.LoanCredit, ItemPaymentKind.PayCredit };

        /// <summary>
        /// Соответствие типа чека его номеру
        /// </summary>
        public static readonly List<ReceiptKind> receiptKind = new List<ReceiptKind> { 0, ReceiptKind.Income, ReceiptKind.IncomeBack, ReceiptKind.Outcome, ReceiptKind.OutcomeBack };

        /// <summary>
        /// Соответствие типа по номеру платежа его номеру
        /// </summary>
        public static readonly List<Native.CmdExecutor.TenderType> tenderType = new List<Native.CmdExecutor.TenderType> { Native.CmdExecutor.TenderType.Cash, Native.CmdExecutor.TenderType.NonCash, Native.CmdExecutor.TenderType.Advance, Native.CmdExecutor.TenderType.Credit, Native.CmdExecutor.TenderType.Barter };

        /// <summary>
        /// Соответствие типа по номеру платежа его типу(электронные, аванс)
        /// </summary>
        public static Dictionary<Native.CmdExecutor.TenderCode, int> tenderCodeType;

        /// <summary>
        /// Соответствие типа нефискльного документа его номеру в ККТ
        /// </summary>
        public static readonly List<Native.CmdExecutor.NFDocType> nfDocType = new List<Native.CmdExecutor.NFDocType> { 0, Native.CmdExecutor.NFDocType.Income, Native.CmdExecutor.NFDocType.Outcome, Native.CmdExecutor.NFDocType.Report };

        public TestingInterfaceFW16(out EcrCtrl ecrCtrl)
        {
            StartLog();
            this.ecrCtrl = ecrCtrl = new EcrCtrl();
            if (ConnectToFW() == 0)
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

                CreateTenderCodeTypeDictionary();
            }
        }

        public TestingInterfaceFW16(out EcrCtrl ecrCtrl, int serialPort, int baudRate)
        {
            StartLog();

            this.ecrCtrl = ecrCtrl = new EcrCtrl();
            if (ConnectToFW(serialPort, baudRate) == 0)
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

                CreateTenderCodeTypeDictionary();
            }
        }

        /// <summary>
        /// Обновление списка соответствий номеров платежей типу платежа.
        /// </summary>
        public void CreateTenderCodeTypeDictionary()
        {
            tenderCodeType = new Dictionary<Native.CmdExecutor.TenderCode, int>();
            var tenderList = ecrCtrl.Info.GetTendersList().GetEnumerator();                                         //получение коллекции соответствий кода платежа типу платежа

            Log("Получено соответствие номеров платежа и типов");

            for (int i = 0; i < countTenderCode; i++)
            {
                tenderList.MoveNext();                                                                              //перебор коллекции
                tenderCodeType.Add((Native.CmdExecutor.TenderCode)i, tenderType.IndexOf(tenderList.Current.Mode));          //создание соответствия кода платежа типу 

                Log($"|{i,3}|{tenderList.Current.Mode,10}|");
            }
        }

        /// <summary>
        /// Создание каталога и файла лога, если это необходимо
        /// </summary>
        public void StartLog()
        {
            fileName = $"log\\log{DateTime.Now.ToString("ddMMyy")}.log";
            if (!Directory.Exists("log")) { Directory.CreateDirectory("log"); }
            if (!File.Exists(fileName)) File.Create(fileName).Close();
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

        /// <summary>
        /// Подключение к ККТ
        /// </summary>
        /// <returns></returns>
        int ConnectToFW()
        {
            try
            {
                ecrCtrl.Init();             //Подключчение по порту и частоте
                Console.WriteLine($"Произведено подключение к ККТ. Port=default Rate=default");
                Log($"Произведено подключение к ККТ.\n" +
                    $" Port=default\n" +
                    $" Rate=default");
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

        /// <summary>
        /// Открыть смену
        /// </summary>
        /// <param name="nameOperator">Имя оператора</param>
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

        /// <summary>
        /// Закрыть смену
        /// </summary>
        /// <param name="nameOperator">Имя оператора</param>
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
            SetValue(registers, 0, 160, 182);
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
            SetValue(registers, 0, 160, 182);
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
                    $"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]],7}|{sum,8}");
                /*регистры*/
                registersTmp[TestingInterfaceFW16.nfDocType.IndexOf(nfDocType) + 8] += sum;                                                                                                                                                     //добавление в регистры (9,10) суммы по типу нефискального документа
                registersTmp[(int)tenderCode + TestingInterfaceFW16.nfDocType.IndexOf(nfDocType) * 10 + 81] += sum;                                                                                                                             //добавление в регистры (91-98,101-108) суммы по номеру платежа
                if (TestingInterfaceFW16.tenderCodeType[tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash)) registersTmp[TestingInterfaceFW16.nfDocType.IndexOf(nfDocType) * 10 + 89] += sum;        //добавление в регистры (99,109) суммы электронных типов платежей
                /*регистры денежного ящика*/
                registersTmp[(int)tenderCode + 111] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum;                                                                                                                       //добавление в регистры (111,118) суммы по номеру платежа
                if (TestingInterfaceFW16.tenderCodeType[tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash)) registersTmp[119] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum;  //добавление в регистры (119) суммы электронных типов платежей
                /*регистры открытого документа*/
                registersTmp[160] += sum;                                                                                                                                                   //добавление в регистр (160) суммы открытого документа
                registersTmp[(int)tenderCode + 172] += sum;                                                                                                                                 //добавление в регистрф (172-179) суммы откртого документа по номеру платежа
                switch (TestingInterfaceFW16.tenderCodeType[tenderCode])
                {
                    case 1: registersTmp[181] += sum; break;                                                                                                                                //добавление в регистр (181) суммы открытого документа электронного типа платежа
                    case 0: registersTmp[180] += sum; break;                                                                                                                                //добавление в регистр (180) суммы открытого документа наличного типа платежа
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить сумму\n" +
                    $"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderCodeType[tenderCode],7}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Закрывает нефискальный документ
        /// </summary>
        /// <param name="document">Нефискальный документ</param>
        /// <param name="nfDocType">Тип нефискального документа</param>
        /// <param name="abort">Отмена чека</param>
        public string DocumentComplete(NonFiscalBase document, Native.CmdExecutor.NFDocType nfDocType, bool abort)
        {
            if (abort)
            {
                try
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);                //отмена нефискального документа
                    Console.WriteLine("Отменён нефиксальный документ типа " + nfDocType + "");      //логирование
                    Log($"\t\tНефискальный документ отменён.\n" +
                        $"---------------------------------------------------");
                    counters[TestingInterfaceFW16.nfDocType.IndexOf(nfDocType) + 8 + 11]++;
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
                    counters[TestingInterfaceFW16.nfDocType.IndexOf(nfDocType) + 8]++;
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
            return RequestRegisters(this.registers, RegistersСumulative);

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
            SetValue(registers, 0, 160, 182);
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
                        $"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]],7}|{sum,8}");
                /*регистры*/
                registersTmp[tenderCodeType[tenderCode] + TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) * 10 + 41] += sum;                                                                                  //добавление в регистры (51-55,71-75) суммы по типу платежа
                registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) + 4] += sum;                                                                                                                     //добавление в регистры (5,7) суммы по типу чека коррекции
                /*регистры денежного ящика*/
                registersTmp[(int)tenderCode + 111] += TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) % 3 == 1 ? sum : -sum;                                                                                  //добавление в регистры (111-118) суммы по номеру платежа
                if (TestingInterfaceFW16.tenderCodeType[tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash)) registersTmp[119] += TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) % 3 == 1 ? sum : -sum;     //добавление в регистры (119) суммы электрооного типа платежа
                /*регистры открытого документа*/
                registersTmp[(int)tenderCode + 172] += sum;                                                                                                                                 //добавление в регистры (172-179) суммы открытого документа по номеру платежа
                switch (TestingInterfaceFW16.tenderCodeType[tenderCode])
                {
                    case 1: registersTmp[181] += sum; break;                                                                                                                                //добавление в регистр (181) суммы открытого документа электронного типа платежа
                    case 0: registersTmp[180] += sum; break;                                                                                                                                //добавление в регистр (180) суммы открытого документа наличного типа платежа
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить сумму коррекции\n" +
                    $"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]],7}|{sum,8}\n" +
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
                /*регистры*/
                registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) * 10 + TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode) + 49] += sum;                                                                      //добавление в регистры (60-65,80-85) суммы по ставкам НДС
                switch (TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode))
                {
                    case 1: registersTmp[(TestingInterfaceFW16.receiptKind.IndexOf(receiptKind)) * 10 + (TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode)) + 50 + 5] += Math.Round(sum * 18m / 118m, 2); break;               //добавление в регистры (66,86) суммы НДС
                    case 2: registersTmp[(TestingInterfaceFW16.receiptKind.IndexOf(receiptKind)) * 10 + (TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode)) + 50 + 5] += Math.Round(sum * 10m / 110m, 2); break;                //добавление в регистры (67,87) суммы НДС
                    case 5: registersTmp[(TestingInterfaceFW16.receiptKind.IndexOf(receiptKind)) * 10 + (TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode) - 2) + 50 + 5] += Math.Round(sum * 18m / 118m, 2); break;           //добавление в регистры (68,88) суммы НДС
                    case 6: registersTmp[(TestingInterfaceFW16.receiptKind.IndexOf(receiptKind)) * 10 + (TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode) - 2) + 50 + 5] += Math.Round(sum * 10m / 110m, 2); break;           //добавление в регистры (69,89) суммы НДС
                    default:
                        break;
                }

                /*регистры открытого документа*/
                registersTmp[160] += sum;                                                                                                                       //добавление в регистр (160) суммы открытого документа
                registersTmp[TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode) + 160] += sum;                                                                   //добавление в регситр (161-166) сумма открытого документа по ставкам НДС
                switch (TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode))
                {
                    case 1: registersTmp[(TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode)) + 166] += Math.Round(sum * 18m / 118m, 2); break;                  //добавление в регистры (167) суммы НДС по 18% открытого документа
                    case 2: registersTmp[(TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode)) + 166] += Math.Round(sum * 10m / 110m, 2); break;                  //добавление в регистры (168) суммы НДС по 10% открытого документа
                    case 5: registersTmp[(TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode) - 2) + 166] += Math.Round(sum * 18m / 118m, 2); break;              //добавление в регистры (169) суммы НДС по 18% включительно открытого документа
                    case 6: registersTmp[(TestingInterfaceFW16.vatCodeCorr.IndexOf(vatCode) - 2) + 166] += Math.Round(sum * 10m / 110m, 2); break;              //добавление в регистры (170) суммы НДС по 10% включительно открытого документа
                    default:
                        break;
                }
                /*накопительные регистры*/
                switch (receiptKind)
                {
                    case ReceiptKind.Income: registersTmp[195] += sum; break;
                    case ReceiptKind.Outcome: registersTmp[196] += sum; break;
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
        /// Заверает чек коррекции
        /// </summary>
        /// <param name="document">Документ который следует завершить</param>
        /// <param name="receiptKind">Тип чека коррекции</param>
        /// <param name="abort">Отменить документ</param>
        public string DocumentComplete(Correction document, ReceiptKind receiptKind, bool abort)
        {
            if (abort)
            {
                try
                {
                    ecrCtrl.Service.AbortDoc(Native.CmdExecutor.DocEndMode.Default);
                    Console.WriteLine("Отменён чек коррекции типа " + receiptKind + "");            //логирование
                    Log($"\t\tЧек коррекции отменён.\n" +
                        $"---------------------------------------------------");
                    counters[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) + 4 + 11]++;
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
                    counters[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) + 4]++;
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
            return RequestRegisters(this.registers, RegistersСumulative);
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
            SetValue(registersTmp, 0);
            SetValue(registers, 0, 160, 182);
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
        /// <param name="sum">Сумма</param>
        /// <param name="itemPaymentKind">Способ рассчёта (Предоплата, полная оплата, кредит..)</param>
        /// <param name="kind">Тип добавляемого товара (товар,услуга..)</param>
        public void AddEntry(Receipt document, ReceiptKind receiptKind, string name, decimal count, Native.CmdExecutor.VatCodeType vatCode, ItemBy itemBy, decimal sum, ItemPaymentKind itemPaymentKind = ItemPaymentKind.Payoff, ItemFlags kind = ItemFlags.Regular)
        {
            string code = random.Next().ToString();
            try
            {
                ReceiptEntry receiptEntry;                                                                                                  //товар
                switch (itemBy)
                {
                    case ItemBy.price:receiptEntry = document.NewItemPriced(code, name, vatCode, sum, count);                               //создание по цене
                        break;
                    case ItemBy.cost:receiptEntry = document.NewItemCosted(code, name, count, vatCode, sum);                                //создание по стоимости
                        break;
                    default:
                        receiptEntry = null;
                        break;
                }
                if (ecrCtrl.Info.FfdVersion >= 2)
                {
                    receiptEntry.PaymentKind = itemPaymentKind;                                                                                 //спооб рассчёта
                    receiptEntry.Kind = kind;                                                                                               //тип добавляемого товара
                }
                document.AddEntry(receiptEntry);                                                                                            //добавления товара в чек

                Log($"\t\t\tТовар добавлен\n" +
                    $"\t\t\t {code,15}|{name,12}|{itemBy,6}|{itemPaymentKind,17}|{count,7}|{sum,8}|{vatCode,15}");
                /*регистры*/
                registersTmp[(TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10 + TestingInterfaceFW16.vatCode.IndexOf(vatCode) - 1 + 120] += receiptEntry.Cost;                  //добаление в регистр (120-125,130-135,140-145,150-155) суммы по ставке НДС
                if (TestingInterfaceFW16.vatCode.IndexOf(vatCode) != 3 && TestingInterfaceFW16.vatCode.IndexOf(vatCode) != 4)                                                                   //проверка на нулевые ставки НДС
                    registersTmp[(TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10 + (TestingInterfaceFW16.vatCode.IndexOf(vatCode) > 4 ? TestingInterfaceFW16.vatCode.IndexOf(vatCode) - 2 : TestingInterfaceFW16.vatCode.IndexOf(vatCode)) + 120 + 5] += receiptEntry.VatAmount;   //добавление в регистр (126-129,136-139,146-149,156-159) суммы НДС 
                registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) * 10 + TestingInterfaceFW16.itemPaymentKind.IndexOf(itemPaymentKind) + 189] += receiptEntry.Cost;                //добавление в регистр (200-206, 210-216, 220-226, 230-236) суммы по способу рассчёта 
                /*регистры открытого документа*/
                registersTmp[160] += receiptEntry.Cost;                                                                                                                                                   //добавление в регистр (160) суммы открытого документа
                registersTmp[TestingInterfaceFW16.vatCode.IndexOf(vatCode) + 160] += receiptEntry.Cost;                                                                                         //добавление в регситр (161-166) сумма открытого документа по ставкам НДС
                if (TestingInterfaceFW16.vatCode.IndexOf(vatCode) != 3 && TestingInterfaceFW16.vatCode.IndexOf(vatCode) != 4)
                    registersTmp[(TestingInterfaceFW16.vatCode.IndexOf(vatCode) > 4 ? TestingInterfaceFW16.vatCode.IndexOf(vatCode) - 2 : TestingInterfaceFW16.vatCode.IndexOf(vatCode)) + 160 + 6] += receiptEntry.VatAmount;      //добавление в регситр (167-170) суммы НДС открытого документа 
                registersTmp[171]++;                                                                                                                                                            //Добавление в регситр (171)  количество товарных позиций
                /*накопительные регистры*/
                switch (itemPaymentKind)
                {
                    case ItemPaymentKind.Prepay:
                        registersTmp[260 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (260,270,280,290) накопительный регистр по типу операции
                        break;
                    case ItemPaymentKind.PartlyPrepay:
                        registersTmp[261 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (261,271,281,291) накопительный регистр по типу операции
                        break;
                    case ItemPaymentKind.Advance:
                        registersTmp[262 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (262,272,282,292) накопительный регистр по типу операции
                        break;
                    case ItemPaymentKind.Payoff:
                        registersTmp[263 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (263,273,283,293) накопительный регистр по типу операции
                        break;
                    case ItemPaymentKind.PartlyLoanCredit:
                        registersTmp[264 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (264,274,284,294) накопительный регистр по типу операции
                        break;
                    case ItemPaymentKind.LoanCredit:
                        registersTmp[265 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (265,275,285,295) накопительный регистр по типу операции
                        break;
                    case ItemPaymentKind.PayCredit:
                        registersTmp[266 + ((TestingInterfaceFW16.itemPaymentKind.IndexOf(itemPaymentKind) - 1) * 10)] += sum;                                                                                               //добавление в регистры (266,276,286,296) накопительный регистр по типу операции
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить товар\n" +
                    $"\t\t\t {code,15}|{name,12}|{itemBy,6}|{itemPaymentKind,17}|{count,7}|{sum,8}|{vatCode,13}\n" +
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
            decimal total = registersTmp[160];
            decimal totalPaid = 0m;
            for (int i = 172; i < 180; i++) totalPaid += registersTmp[i];
            decimal balance = (total - totalPaid);
            try
            {
                document.AddPayment(tenderCode, sum);                                                                                                                                       //добавление оплаты 

                Log($"\t\t\tОплата добавлена\n" +
                    $"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]],7}|{sum,8}");

                if (tenderCode == Native.CmdExecutor.TenderCode.Cash && balance < sum)                                                                                                      //учитывание сдачи при расплате наличными
                {
                    Log($"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]],7}|{balance - sum,8}");                                                //Логирование сдачи
                    sum = balance;
                }

                /*регистры*/
                registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind)] += sum;                                                                                                                         //добавление в регистры (1-4) суммы по типу операции
                registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) * 10 + 1 + (int)tenderCode] += sum;                                                                                              //добавление в регистры (11-18, 21-28, 31-38, 41-48) суммы по номеру платежа
                if (TestingInterfaceFW16.tenderCodeType[tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash)) registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) * 10 + 1 + 8] += sum;             //добавление в регистры (19, 29, 39, 49) суммы электрооного типа платежа
                /*регистры открытого документа*/
                registersTmp[(int)tenderCode + 172] += sum;                                                                                                                                 //добавление в регистры (172-179) суммы открытого документа по номеру платежа
                switch (TestingInterfaceFW16.tenderCodeType[tenderCode])
                {
                    case 1: registersTmp[181] += sum; break;                                                                                                                                //добавление в регистр (181) суммы открытого документа электронного типа платежа
                    case 0: registersTmp[180] += sum; break;                                                                                                                                //добавление в регистр (180) суммы открытого документа наличного типа платежа
                    default:
                        break;
                }
                /*регистры денежного ящика*/
                registersTmp[(int)tenderCode + 111] += TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) % 3 == 1 ? sum : -sum;                                                                                  //добавление в регистры (111-118) суммы денежного ящика по номеру платежа
                if (TestingInterfaceFW16.tenderCodeType[tenderCode] == TestingInterfaceFW16.tenderType.IndexOf(Native.CmdExecutor.TenderType.NonCash)) registersTmp[119] += TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) % 3 == 1 ? sum : -sum;     //добавление в регистр (119) суммы денежного электрооного типа платежа
                /*накопительные регистры*/
                registersTmp[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) + 190] += sum;                                                                                                                   //добавление в регистры (191-194) накопительный регистр по типу операции
                switch (TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]])
                {
                    case Native.CmdExecutor.TenderType.Cash:
                        registersTmp[240 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 5)] += sum;                                                                                               //добавление в регистры (240,245,250,255) накопительный регистр по типу операции
                        break;
                    case Native.CmdExecutor.TenderType.NonCash:
                        registersTmp[241 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 5)] += sum;                                                                                               //добавление в регистры (241,246,251,256) накопительный регистр по типу операции
                        break;
                    case Native.CmdExecutor.TenderType.Advance:
                        registersTmp[242 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 5)] += sum;                                                                                               //добавление в регистры (242,247,252,257) накопительный регистр по типу операции
                        break;
                    case Native.CmdExecutor.TenderType.Credit:
                        registersTmp[243 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 5)] += sum;                                                                                               //добавление в регистры (243,248,253,258) накопительный регистр по типу операции
                        break;
                    case Native.CmdExecutor.TenderType.Barter:
                        registersTmp[244 + ((TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) - 1) * 5)] += sum;                                                                                               //добавление в регистры (244,249,254,259) накопительный регистр по типу операции
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось добавить оплату\n" +
                    $"\t\t\t {(int)tenderCode,3}|{TestingInterfaceFW16.tenderType[TestingInterfaceFW16.tenderCodeType[tenderCode]],7}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Добавление скидки/наценки 
        /// </summary>
        /// <param name="document">Чек в который необходимо добавить товар</param>
        /// <param name="receiptKind">Тип чека (Приход, Отмена прихода..)</param>
        /// <param name="sum">Сумма коррекции</param>
        public void SetAdjustment(Receipt document, ReceiptKind receiptKind, decimal sum)
        {
            AdjustmentType adjustmentType = (sum > 0m ? AdjustmentType.markup : AdjustmentType.sale);
            string description = _GetDescription(adjustmentType);
            try
            {
                document.SetAdjustment(sum, description);
                Log($"\t\t\tДобавлена коррекция суммы.\n" +
                    $"\t\t\t {description,8}|{sum,8}\n");
                /*регистры*/
                registersTmp[180 + TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) * 2 + (int)adjustmentType] += Math.Abs(sum);   //Добавление в регистры (182-189) суммы коррекции суммы
                /*регистры открытого документа*/
                registersTmp[160] += sum;                                           //Добавление в регистр (160) коррекции суммы
            }
            catch (Exception ex)
            {
                Log($"\t\t\tError! Не удалось коррекцию суммы.\n" +
                    $"\t\t\t {description,8}|{sum,8}\n" +
                    $"\t\t\t Exception={ex.Message}");
            }
        }

        /// <summary>
        /// Заверает чек
        /// </summary>
        /// <param name="document">чек который следует завершить</param>
        /// <param name="receiptKind">Тип чека коррекции</param>
        /// <param name="abort">Отменить документ</param>
        public string DocumentComplete(Receipt document, ReceiptKind receiptKind, bool abort)
        {
            if (abort)
            {
                try
                {
                    document.Abort();                                                               //отмена документа
                    Console.WriteLine("Отменён чек типа " + receiptKind + "");                      //логирование
                    Log($"\t\tЧек отменён.\n" +
                        $"---------------------------------------------------");
                    counters[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind) + 11]++;                                 //увеличение счётчика (12-15) отмены по типу чека
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
                    counters[TestingInterfaceFW16.receiptKind.IndexOf(receiptKind)]++;                                      //учеличение счётчика (1-4) оформления по типу чека
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
            return RequestRegisters(this.registers, RegistersСumulative);
        }

        /// <summary>
        /// Сверяет регистры с массивом регистров в указанном диапозоне
        /// </summary>
        /// <param name="startIndex">Начальный индекс</param>
        /// <param name="endIndex">Конечный индекс, не включительно</param>
        public int RequestRegisters(decimal[] testRegisters)
        {
            ushort startIndex = 1;
            ushort endIndex = countRegisters;
            string err = $"Error!\n" +
                         $"+-------+--------------------------------------------------+------------------+-------------------+\n" +
                         $"|   #   |{"discription",lenStringDiscription}|       test       |        ККТ        |\n" +
                         $"+-------+--------------------------------------------------+------------------+-------------------+\n";                                       //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i <= endIndex; i++)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1)
                {
                    try
                    {
                        decimal tmp = ecrCtrl.Info.GetRegister(i);
                        if (tmp != testRegisters[i])                                                                                                //Проверка расходения регистров
                        {
                            string discription = _GetDescription((Native.CmdExecutor.RegisterCode)i);                                           //Получение описания регистра
                            int startPosition = 0;                                                                                              //Стартовая позция вывода описания
                            err += $"|{i,7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{testRegisters[i],18:F}|{tmp,19:F}|\n";   //Вывод первой строки описания
                            for (startPosition = lenStringDiscription; startPosition < discription.Length; startPosition += lenStringDiscription)
                            {
                                err += $"|{"",7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{"",18:F}|{"",19:F}|\n";         //Вывод последующих строк описания, если необходимо
                            }
                        }
                        testRegisters[i] = tmp;
                        Log($"Программный регистр №{i,4} получил значение {registers[i]}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Warning! Не удалось получить доступ к регистру №" + i + "");
                        Log($"Warning! Не удалось получить доступ к регистру №{i}\n" +
                            $" Exception={ex.Message}");
                    }
                }
            }
            Console.Write(((err.Length > 310) ? err : ""));           //логирование
            Log($"Запрошеные данные с регистров с {startIndex} по {endIndex} {((err.Length > 310) ? "\n" + err : "")}");           //логирование
            if (err.Length > 310)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Сравнение значени программных регистров с значениями регистров ККТ
        /// </summary>
        /// <param name="arr">Массив номеров исключаемых регистров</param>
        /// <returns></returns>
        public string RequestRegisters(decimal[] testRegisters, int[] arr)
        {
            ushort endIndex = countRegisters;
            ushort startIndex = 1;
            string errRegisters = "";
            string err = $"Error!\n" +
                         $"+-------+--------------------------------------------------+------------------+-------------------+\n" +
                         $"|   #   |{"discription",lenStringDiscription}|       test       |        ККТ        |\n" +
                         $"+-------+--------------------------------------------------+------------------+-------------------+\n";                                                                                          //строка ошибки заполняемая при несоответсвии регистров
            for (ushort i = startIndex; i <= endIndex; i++)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1 && Array.IndexOf(arr, i) == -1)
                {
                    try
                    {
                        decimal tmp = ecrCtrl.Info.GetRegister(i);
                        if (tmp != registers[i])                                                                                                //Проверка расходения регистров
                        {
                            errRegisters += i + ",";
                            string discription = _GetDescription((Native.CmdExecutor.RegisterCode)i);                                           //Получение описания регистра
                            int startPosition = 0;                                                                                              //Стартовая позция вывода описания
                            err += $"|{i,7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{registers[i],18:F}|{tmp,19:F}|\n";   //Вывод первой строки описания
                            for (startPosition = lenStringDiscription; startPosition < discription.Length; startPosition += lenStringDiscription)
                            {
                                err += $"|{"",7:D}|{discription.Substring(startPosition, Math.Min(discription.Length - startPosition, lenStringDiscription)),lenStringDiscription}|{"",18:F}|{"",19:F}|\n";         //Вывод последующих строк описания, если необходимо
                            }
                            testRegisters[i] = tmp;
                            Log($"Программный регистр №{i,4} получил значение {registers[i]}");
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
            return errRegisters;
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
        public void GetRegisters(int[] arr)
        {
            ushort endIndex = countRegisters;
            ushort startIndex = 1;
            string err = "";
            if (arr == null) arr = new int[] { -1 };
            for (ushort i = startIndex; i <= endIndex; i++)
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
        /// Обновление программных регистров данными из ККТ
        /// </summary>
        public void GetRegisters()
        {
            ushort endIndex = countRegisters;
            ushort startIndex = 1;
            string err = "";
            for (ushort i = startIndex; i <= endIndex; i++)
            {
                if (inaccessibleRegisters.IndexOf(i) == -1)
                {
                    try
                    {
                        registers[i] = ecrCtrl.Info.GetRegister(i);                                 //запрос значений регистров из ККТ
                        if (countGetRegister == 0) controlRegisters[i] = registers[i];              //присвоение значений накпоительным программным регистрам
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
            countGetRegister++;
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
            ushort endIndex = countRegisters;
            ushort startIndex = 1;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (Array.IndexOf(RegistersOpenReciept, i) != -1)
                {
                    registers[i] = registersTmp[i];                                                        //применение временного массива к конечному
                    controlRegisters[i] = registersTmp[i];
                }
                else
                {
                    registers[i] += registersTmp[i];                                                        //применение временного массива к конечному
                    controlRegisters[i] += registersTmp[i];
                }
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
        public void Log(string message)
        {
            string[] messages = message.Split('\n');
            foreach (string i in messages)
            {
                File.AppendAllText(fileName, $"{DateTime.Now.ToString("HH:mm:ss.ffff")}\t{i}\r\n");
            }

        }

        /// <summary>
        /// Получить описание enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

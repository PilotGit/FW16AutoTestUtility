using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fw16;
using Fw16.Model;


namespace FW16AutoTestUtility
{
    class Class2
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

        public EcrCtrl ecrCtrl;
        decimal[] registersTmp;                  //массив временных регистров

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

        public Class2(out EcrCtrl  ecrCtrl, ref decimal[] registersTmp)
        {
            this.ecrCtrl = ecrCtrl = new EcrCtrl();
            this.registersTmp = registersTmp;
            ConnectToFW();
            tenderCodeType = new Dictionary<Native.CmdExecutor.TenderCode, int>();
            var tenderList = ecrCtrl.Info.GetTendersList().GetEnumerator();                                         //получение коллекции соответствий кода платежа типу платежа
            for (int i = 0; i < countTenderCode; i++)
            {
                tenderList.MoveNext();                                                                              //перебор коллекции
                tenderCodeType.Add((Native.CmdExecutor.TenderCode)i, tenderType[tenderList.Current.Mode]);          //создание соответствия кода платежа типу 
            }
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
        public void AddEntry(Fw16.Ecr.Receipt document, ReceiptKind receiptKind, string name, decimal count, Native.CmdExecutor.VatCodeType vatCode, bool coast, decimal money, ItemPaymentKind paymentKind = ItemPaymentKind.Payoff, ItemFlags kind = ItemFlags.Regular)
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
        public void AddPayment(Fw16.Ecr.Receipt document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
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
        public void AddTender(Fw16.Ecr.Correction document, ReceiptKind receiptKind, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
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
        public void AddTender(Fw16.Ecr.NonFiscalBase document, Native.CmdExecutor.NFDocType nfDocType, Native.CmdExecutor.TenderCode tenderCode, decimal sum)
        {
            var tender = new Tender
            {
                Amount = sum,
                Code = tenderCode
            };
            document.AddTender(tender);

            registersTmp[this.nfDocType[nfDocType] + 8] += sum;                                                                                                                             //добавление в регистры (9,10) суммы по типу нефискального документа
            registersTmp[(int)tenderCode + this.nfDocType[nfDocType] * 10 + 81] += sum;                                                                                                         //добавление в регистры (91-98,101-108) суммы по номеру платежа
            if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[this.nfDocType[nfDocType] * 10 + 89] += sum;                            //добавление в регистры (99,109) суммы электронных типов платежей

            registersTmp[(int)tenderCode + 111] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum;                                                                           //добавление в регистры (111,118) суммы по номеру платежа
            if (this.tenderCodeType[tenderCode] == this.tenderType[Native.CmdExecutor.TenderType.NonCash]) registersTmp[119] += nfDocType == Native.CmdExecutor.NFDocType.Income ? sum : -sum; //добавление в регистры (119) суммы электронных типов платежей
        }

        /// <summary>
        /// Добавление суммы по ставке НДС.
        /// </summary>
        /// <param name="document">Чек коррекции</param>
        /// <param name="receiptKind">Тип чека (Приход, изъятие)</param>
        /// <param name="vatCode">Ставка НДС</param>
        /// <param name="sum">Сумма</param>
        public void AddAmount(Fw16.Ecr.Correction document, ReceiptKind receiptKind, VatCode vatCode, decimal sum)
        {
            document.AddAmount(vatCode, sum);

            registersTmp[this.receiptKind[receiptKind] * 10 + this.vatCode2[vatCode] + 50] += sum;                                                                    //добавление в регистры (60-65,80-85) суммы по ставкам НДС
            switch (this.vatCode2[vatCode])
            {
                case 1: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum * 18m / 118m, 2); break;            //добавление в регистры (66,86) суммы НДС
                case 5: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode]) + 50 + 5] += Math.Round(sum * 10m / 110m, 2); break;              //добавление в регистры (68,88) суммы НДС
                case 2: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 18m / 118m, 2); break;        //добавление в регистры (67,87) суммы НДС
                case 6: registersTmp[(this.receiptKind[receiptKind]) * 10 + (this.vatCode2[vatCode] - 2) + 50 + 5] += Math.Round(sum * 10m / 110m, 2); break;         //добавление в регистры (69,89) суммы НДС
                default:
                    break;
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SpendingAnalyzer.Common
{
    internal class TransactionTypeMapping
    {
        public ConcurrentDictionary<TransactionType, string> Mapping =>
            new ConcurrentDictionary<TransactionType, string>
            {
                [TransactionType.StandingOrder] = "zlecenie stałe",
                [TransactionType.WebPaymentMobileCode] = "płatność web - kod mobilny",
                [TransactionType.AccountTransfer] = "przelew na rachunek",
                [TransactionType.CardPayment] = "płatność kartą",
                [TransactionType.IncomingPhoneTransferExternal] = "przelew na telefon przychodz. zew.",
                [TransactionType.OutgoingPhoneTransferExternal] = "przelew na telefon wychodzący zew.",
                [TransactionType.TerminalPurchaseMobileCode] = "zakup w terminalu - kod mobilny",
                [TransactionType.ATMWithdrawal] = "wypłata z bankomatu",
                [TransactionType.AccountDeposit] = "przelew z rachunku"
            };


    }

    internal enum TransactionType
    {
        StandingOrder,
        WebPaymentMobileCode,
        AccountTransfer,
        CardPayment,
        IncomingPhoneTransferExternal,
        OutgoingPhoneTransferExternal,
        TerminalPurchaseMobileCode,
        ATMWithdrawal,
        AccountDeposit
    }
}

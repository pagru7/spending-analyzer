using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SpendingAnalyzer.Common
{
    internal static class TransactionTypeMapping
    {
        public static ConcurrentDictionary<string, TransactionType> Mapping =>
            new ConcurrentDictionary<string, TransactionType>
            {
                ["zlecenie stałe"] = TransactionType.StandingOrder,
                ["płatność web - kod mobilny"] = TransactionType.WebPaymentMobileCode,
                ["przelew na rachunek"] = TransactionType.AccountTransfer,
                ["płatność kartą"] = TransactionType.CardPayment,
                ["przelew na telefon przychodz. zew."] = TransactionType.IncomingPhoneTransferExternal,
                ["przelew na telefon wychodzący zew."] = TransactionType.OutgoingPhoneTransferExternal,
                ["zakup w terminalu - kod mobilny"] = TransactionType.TerminalPurchaseMobileCode,
                ["wypłata z bankomatu"] = TransactionType.ATMWithdrawal,
                ["przelew z rachunku"] = TransactionType.AccountDeposit
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

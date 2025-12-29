using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SpendingAnalyzer.Common
{
    internal static class ImportTransactionTypeMapping
    {
        public static ConcurrentDictionary<string, ImportTransactionType> Mapping =>
            new ConcurrentDictionary<string, ImportTransactionType>
            {
                ["zlecenie stałe"] = ImportTransactionType.StandingOrder,
                ["płatność web - kod mobilny"] = ImportTransactionType.WebPaymentMobileCode,
                ["przelew na rachunek"] = ImportTransactionType.AccountTransfer,
                ["płatność kartą"] = ImportTransactionType.CardPayment,
                ["przelew na telefon przychodz. zew."] = ImportTransactionType.IncomingPhoneTransferExternal,
                ["przelew na telefon wychodzący zew."] = ImportTransactionType.OutgoingPhoneTransferExternal,
                ["zakup w terminalu - kod mobilny"] = ImportTransactionType.TerminalPurchaseMobileCode,
                ["wypłata z bankomatu"] = ImportTransactionType.ATMWithdrawal,
                ["przelew z rachunku"] = ImportTransactionType.AccountDeposit
            };
    }
}

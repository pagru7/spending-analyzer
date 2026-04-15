using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SpendingAnalyzer.Common
{
    internal static class ImportTransactionTypeMapping
    {
        //public static ConcurrentDictionary<string, ImportTransactionType> Mapping =>
        //    new ConcurrentDictionary<string, ImportTransactionType>
        //    {
        //        // przelew
        //        ["zlecenie stałe"] = ImportTransactionType.StandingOrder,
        //        // Blik
        //        ["płatność web - kod mobilny"] = ImportTransactionType.WebPaymentMobileCode,
        //        // przelew
        //        ["przelew na rachunek"] = ImportTransactionType.AccountTransfer,
        //        // karta
        //        ["płatność kartą"] = ImportTransactionType.CardPayment,
        //        // przelew
        //        ["przelew na telefon przychodz. zew."] = ImportTransactionType.IncomingPhoneTransferExternal,
        //        // przelew
        //        ["przelew na telefon wychodzący zew."] = ImportTransactionType.OutgoingPhoneTransferExternal,
        //        ["zakup w terminalu - kod mobilny"] = ImportTransactionType.TerminalPurchaseMobileCode,
        //        ["wypłata z bankomatu"] = ImportTransactionType.ATMWithdrawal,
        //        ["przelew z rachunku"] = ImportTransactionType.AccountDeposit,
        //        ["MOBILE-PAYMENT-C2C"] = ImportTransactionType.OutgoingPhoneTransferExternal
        //    };

        public static ConcurrentDictionary<string, TransactionType> Mapping =>
            new ConcurrentDictionary<string, TransactionType>
            {
                // przelew
                ["zlecenie stałe"] = TransactionType.Transfer,
                // Blik
                ["płatność web - kod mobilny"] = TransactionType.Code,
                // przelew
                ["przelew na rachunek"] = TransactionType.Transfer,
                // karta
                ["płatność kartą"] = TransactionType.Card,
                // przelew
                ["przelew na telefon przychodz. zew."] = TransactionType.Transfer,
                // przelew
                ["przelew na telefon wychodzący zew."] = TransactionType.Transfer,
                ["zakup w terminalu - kod mobilny"] = TransactionType.Code,
                ["wypłata z bankomatu"] = TransactionType.Withdrawal,
                ["przelew z rachunku"] = TransactionType.Transfer,
                ["MOBILE-PAYMENT-C2C"] = TransactionType.Transfer
            };
    }
}

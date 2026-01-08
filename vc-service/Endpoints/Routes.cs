using System;
using System.Collections.Generic;
using System.Text;

namespace SpendingAnalyzer.Endpoints
{
    internal class ApiRoutes
    {
        public const string Accounts = "/api/accounts";
        public const string Banks = "/api/banks";
        public const string BankById = "/api/banks/{id}";
        public const string BankAccounts = "/api/banks/{bankId}/accounts";
        public const string BankAccountById = "/api/banks/{bankId}/accounts/{accountId}";
        public const string BankAccountByIdTransactionImport = "/api/banks/{bankId}/accounts/{accountId}/transaction/import";
        public const string BankAccountByIdActive = "/api/banks/{bankId}/accounts/{accountId}/active";
        public const string BankAccountByIdBalance = "/api/banks/{bankId}/accounts/{accountId}/balance";
        public const string Transactions = "/api/transactions";
        public const string TransactionById = "/api/transactions/{id}";
    }
}

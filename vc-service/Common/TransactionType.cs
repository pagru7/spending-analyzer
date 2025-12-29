namespace SpendingAnalyzer.Common
{
    public enum ImportTransactionType
    {
        StandingOrder = 1,
        WebPaymentMobileCode = 2,
        AccountTransfer = 3,
        CardPayment = 4,
        IncomingPhoneTransferExternal = 5,
        OutgoingPhoneTransferExternal = 6,
        TerminalPurchaseMobileCode = 7,
        ATMWithdrawal = 8,
        AccountDeposit = 9
    }
}

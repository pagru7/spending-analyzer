I need to create api in latest version of dotnet.
Use fastendpoint for api endpoints, and serilog for logging (log to file).
The application will be using postgress db.
The app will be named "SpendingAnalyzer"

Requirements:

1. Banks
   - user can create bank definition which consists of id, name and inactive. Create request may contain list of bank accounts (check bank account section)
   - user can edit bank name, but can't delete, there is an delete endpoints which simply marks bank as inactive
2. Bank Account
   - user can create bank account, but it has to provice id of the bank where account need to defined. Bank account consits of id, name, creation date, and balance.
   - user can edit bank account name
   - user can set bank account as inactive
3. Transaction
   - user can create transaction, it consits of id, description, accountId, recipient, amount
   - user can modify transaction (description, accountId, recipient, amount)
   - user can remove transaction
4. Transfer
   - user can create a transfer, is migration of money from one account to another, transfer consists of id, description, sourceAccountId, targetAccountId, value. After creating the transfer, accounts balance has to updated
   - user can update transfer by changing source account, target, description and value.
   - user can also remove transfer

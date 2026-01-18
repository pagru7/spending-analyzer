# Goal
I want to create very simple spending tracker mobile application that allows users to log their expenses and view summaries of their spending habits.
App will have two main features: logging expenses and send them to backend server (backend server is already done, do not create it).

# Features
1. Settings
    - user can define host url for backend server
    - user can devine port number for backend server
    - user can define account id and name
    - user can define api key for authentication
    - user can enter initial balance amount
2. Log Expense
    - user can input expense amount, recipient, description, date and time (by default current time and date)
    - of course user can also edit previously logged expenses
3. View transactions
    - user can view list of all logged expenses
    - user can filter expenses by date range, recipient, amount range
    - no all transactions are loaded at once (pagination or infinite scroll)
4. Export data
    - user can export (send) to backend server in json format, app will ask user if he wants to send all transaction or just synchronize new onces that were not synchronize yet
    - exported transaction should be marked as synchronized
    - synchronization means we are also updating local data with modified data from backend
   
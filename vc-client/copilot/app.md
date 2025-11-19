# General description

This is the app that should help me track and manage my finance spendings.

# Libraries

It uses shadcn UI components.
For api communication axios library should be used (use-axios).
Use react-hook-form

# Api

Check swagger.json file in this directory to check what api methods are available

# App description

main view should display current total balance (summary) with all available account balances.
There should left side menu (side panel or something similar) where Banks, transaction and transfer links should be available. There should be also 'Add Bank' and 'Add transaction' and 'Add transfer' options in the side panel bottom.

## Banks

Once user click on banks, list of available banks should be presented (as collapsible options displayed in main view). When user clicks on bank name, list of available accounts should be displayed below.
User should be able to update bank name (dialog window) or remove it ( mark as inactive).

## Transfers

List of transfers should be presented in main view

## Transactions

List of transactions should be displayed

## Side panel bottom

In the bottom of the side panel there should be option to add new bank (in dialog), user can add only bank or bank with accounts (one or more);

There should be aslo option to enter transaction, where user can select account (from which payment was done) category etc, all other fields defined in swagger.api (api)

There should be also option to enter transfers between accounts. User should be able to select source account and target account, description and value of money transfered.

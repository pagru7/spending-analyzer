import './App.css';

import { useState } from 'react';

import { useAppData } from '@/hooks/useAppData';
import {
  type BankAccountResponse,
  type BankResponse,
  type TransactionResponse,
} from '@/types/api';
import type { ViewKey } from '@/types/app';
import AddAccountDialog from './features/AddAccountDialog';
import AddBankDialog from './features/AddBankDialog';
import AddTransactionDialog from './features/AddTransactionDialog';
import AddTransferDialog from './features/AddTransferDialog';
import AppSidebar from './features/AppSidebar';
import BanksView from './features/BanksView';
import DashboardCards from './features/DashboardCards';
import ImportedTransactionsView from './features/ImportedTransactionsView';
import ImportTransactionsDialog from './features/ImportTransactionsDialog';
import TransactionsView from './features/TransactionsView';
import UpdateAccountBalanceDialog from './features/UpdateAccountBalanceDialog';
import UpdateBankDialog from './features/UpdateBankDialog';
import UpdateTransactionDialog from './features/UpdateTransactionDialog';

function App() {
  const [activeView, setActiveView] = useState<ViewKey>('banks');
  const [isAddBankOpen, setIsAddBankOpen] = useState(false);
  const [isAddTransactionOpen, setIsAddTransactionOpen] = useState(false);
  const [isAddTransferOpen, setIsAddTransferOpen] = useState(false);
  const [isImportTransactionsOpen, setIsImportTransactionsOpen] =
    useState(false);
  const [bankToEdit, setBankToEdit] = useState<BankResponse | null>(null);
  const [bankForNewAccount, setBankForNewAccount] =
    useState<BankResponse | null>(null);
  const [transactionToEdit, setTransactionToEdit] =
    useState<TransactionResponse | null>(null);
  const [accountToUpdateBalance, setAccountToUpdateBalance] =
    useState<BankAccountResponse | null>(null);
  const [bankIdForBalanceUpdate, setBankIdForBalanceUpdate] = useState<
    string | null
  >(null);

  const {
    banksData,
    banksLoading,
    banksError,
    refetchBanks,
    transactionsData,
    transactionsLoading,
    transactionsError,
    refetchTransactions,
    importedTransactionsData,
    importedTransactionsLoading,
    importedTransactionsError,
    accounts,
    totalBalance,
    refetchAll,
    handleBankUpdate,
    handleBankInactive,
    handleDeleteAccount,
    busyBankId,
    busyAccountId,
    bankActionError,
  } = useAppData();

  const renderMainContent = () => {
    if (activeView === 'banks') {
      return (
        <BanksView
          banks={banksData}
          loading={banksLoading}
          errorMessage={banksError ? 'Unable to load banks.' : null}
          onEdit={setBankToEdit}
          onMarkInactive={(bank) => handleBankInactive(bank.id)}
          onAddAccount={setBankForNewAccount}
          onDeleteAccount={handleDeleteAccount}
          onUpdateBalance={(account, bankId) => {
            setAccountToUpdateBalance(account);
            setBankIdForBalanceUpdate(bankId);
          }}
          onRefresh={refetchBanks}
          busyBankId={busyBankId}
          busyAccountId={busyAccountId}
          actionError={bankActionError}
        />
      );
    }
    if (activeView === 'transactions') {
      return (
        <TransactionsView
          transactions={transactionsData}
          loading={transactionsLoading}
          errorMessage={
            transactionsError ? 'Unable to load transactions.' : null
          }
          onEdit={setTransactionToEdit}
        />
      );
    }
    return (
      <ImportedTransactionsView
        transactions={importedTransactionsData}
        loading={importedTransactionsLoading}
        errorMessage={
          importedTransactionsError
            ? 'Unable to load imported transactions.'
            : null
        }
      />
    );
  };

  return (
    <div className="flex h-dvh overflow-hidden bg-muted/40">
      <AppSidebar
        activeView={activeView}
        onViewChange={setActiveView}
        onRefresh={refetchAll}
        counts={{
          banks: banksData?.length ?? 0,
          transactions: transactionsData?.length ?? 0,
          importedTransactions: importedTransactionsData?.length ?? 0,
        }}
        actions={{
          onAddBank: () => setIsAddBankOpen(true),
          onAddTransaction: () => setIsAddTransactionOpen(true),
          onAddTransfer: () => setIsAddTransferOpen(true),
          onImportTransactions: () => setIsImportTransactionsOpen(true),
        }}
      />

      <main className="min-h-0 flex-1 overflow-hidden">
        <div className="flex h-full min-h-0 w-full flex-col gap-8 p-6">
          <DashboardCards
            totalBalance={totalBalance}
            accountCount={accounts.length}
            activeBankCount={
              (banksData ?? []).filter((b: BankResponse) => !b.isInactive)
                .length
            }
            allBankCount={banksData?.length ?? 0}
            loading={banksLoading}
          />
          <section id="main-content" className="min-h-0 flex-1 overflow-auto">
            {renderMainContent()}
          </section>
        </div>
      </main>

      <AddBankDialog
        open={isAddBankOpen}
        onOpenChange={setIsAddBankOpen}
        onSuccess={async () => {
          await refetchBanks();
        }}
      />
      <AddTransactionDialog
        open={isAddTransactionOpen}
        onOpenChange={setIsAddTransactionOpen}
        accounts={accounts}
        onSuccess={async () => {
          await Promise.allSettled([refetchTransactions(), refetchBanks()]);
        }}
      />
      <AddTransferDialog
        open={isAddTransferOpen}
        onOpenChange={setIsAddTransferOpen}
        accounts={accounts}
        onSuccess={async () => {
          await refetchBanks();
        }}
      />
      <UpdateBankDialog
        bank={bankToEdit}
        busy={busyBankId === bankToEdit?.id}
        onClose={() => setBankToEdit(null)}
        onSubmit={async (payload) => {
          if (!bankToEdit) return;
          await handleBankUpdate(bankToEdit.id, payload);
          setBankToEdit(null);
        }}
      />
      <AddAccountDialog
        open={bankForNewAccount !== null}
        onOpenChange={(open) => {
          if (!open) setBankForNewAccount(null);
        }}
        bank={bankForNewAccount}
        onSuccess={async () => {
          await refetchBanks();
        }}
      />
      <UpdateTransactionDialog
        transaction={transactionToEdit}
        accounts={accounts}
        onClose={() => setTransactionToEdit(null)}
        onSuccess={async () => {
          await Promise.allSettled([refetchTransactions(), refetchBanks()]);
        }}
      />
      <ImportTransactionsDialog
        open={isImportTransactionsOpen}
        onOpenChange={setIsImportTransactionsOpen}
        accounts={accounts}
        onSuccess={async () => {
          await Promise.allSettled([refetchTransactions(), refetchBanks()]);
        }}
      />
      <UpdateAccountBalanceDialog
        account={accountToUpdateBalance}
        bankId={bankIdForBalanceUpdate}
        onClose={() => {
          setAccountToUpdateBalance(null);
          setBankIdForBalanceUpdate(null);
        }}
        onSuccess={async () => {
          await refetchBanks();
        }}
      />
    </div>
  );
}

export default App;

import './App.css';

import useAxios from 'axios-hooks';
import {
  BanknoteIcon,
  Building2Icon,
  CoinsIcon,
  RefreshCcwIcon,
  SendIcon,
} from 'lucide-react';
import { useMemo, useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import { Skeleton } from '@/components/ui/skeleton';
import { currencyFormatter, numberFormatter } from '@/lib/formatters';
import {
  type BankAccountResponse,
  type BankResponse,
  type TransactionResponse,
  type TransferResponse,
  type UpdateBankRequest,
} from '@/types/api';
import AddBankDialog from './features/AddBankDialog';
import AddTransactionDialog from './features/AddTransactionDialog';
import AddTransferDialog from './features/AddTransferDialog';
import BanksView from './features/BanksView';
import TransactionsView from './features/TransactionsView';
import TransfersView from './features/TransfersView';
import UpdateBankDialog from './features/UpdateBankDialog';

type ViewKey = 'banks' | 'transactions' | 'transfers';

function App() {
  const [activeView, setActiveView] = useState<ViewKey>('banks');
  const [isAddBankOpen, setIsAddBankOpen] = useState(false);
  const [isAddTransactionOpen, setIsAddTransactionOpen] = useState(false);
  const [isAddTransferOpen, setIsAddTransferOpen] = useState(false);
  const [bankToEdit, setBankToEdit] = useState<BankResponse | null>(null);
  const [bankActionError, setBankActionError] = useState<string | null>(null);
  const [busyBankId, setBusyBankId] = useState<string | null>(null);

  const [
    { data: banksData, loading: banksLoading, error: banksError },
    refetchBanks,
  ] = useAxios<BankResponse[]>({ url: '/api/banks' });

  const [
    {
      data: transactionsData,
      loading: transactionsLoading,
      error: transactionsError,
    },
    refetchTransactions,
  ] = useAxios<TransactionResponse[]>({ url: '/api/transactions' });

  const [
    { data: transfersData, loading: transfersLoading, error: transfersError },
    refetchTransfers,
  ] = useAxios<TransferResponse[]>({ url: '/api/transfers' });

  const [, executeUpdateBank] = useAxios<BankResponse>(
    { method: 'PUT' },
    { manual: true }
  );
  const [, executeDeleteBank] = useAxios<void>(
    { method: 'DELETE' },
    { manual: true }
  );

  const accounts: BankAccountResponse[] = useMemo(() => {
    return (banksData ?? [])
      .flatMap((bank: BankResponse) =>
        bank.bankAccounts.map((account: BankAccountResponse) => ({
          ...account,
          bankId: bank.id,
          bankName: bank.name,
        }))
      )
      .filter((account: BankAccountResponse) => !account.isInactive);
  }, [banksData]);

  const totalBalance = useMemo(
    () =>
      accounts.reduce(
        (sum, account) =>
          sum + (Number.isFinite(account.balance) ? account.balance : 0),
        0
      ),
    [accounts]
  );

  const refetchAll = async () => {
    await Promise.allSettled([
      refetchBanks(),
      refetchTransactions(),
      refetchTransfers(),
    ]);
  };

  const navItems: {
    key: ViewKey;
    label: string;
    icon: React.ReactNode;
    total?: number;
  }[] = [
    {
      key: 'banks',
      label: 'Banks',
      icon: <Building2Icon className="size-4" />,
      total: banksData?.length ?? 0,
    },
    {
      key: 'transactions',
      label: 'Transactions',
      icon: <CoinsIcon className="size-4" />,
      total: transactionsData?.length ?? 0,
    },
    {
      key: 'transfers',
      label: 'Transfers',
      icon: <SendIcon className="size-4" />,
      total: transfersData?.length ?? 0,
    },
  ];

  const handleBankUpdate = async (
    bankId: string,
    payload: UpdateBankRequest
  ) => {
    setBankActionError(null);
    setBusyBankId(bankId);
    try {
      await executeUpdateBank({
        url: `/api/banks/${bankId}`,
        data: payload,
      });
      await refetchBanks();
    } catch {
      setBankActionError('Failed to update bank. Please try again.');
    } finally {
      setBusyBankId(null);
    }
  };

  const handleBankInactive = async (bankId: string) => {
    setBankActionError(null);
    setBusyBankId(bankId);
    try {
      await executeDeleteBank({
        url: `/api/banks/${bankId}`,
      });
      await refetchBanks();
    } catch {
      setBankActionError('Failed to update bank status. Please try again.');
    } finally {
      setBusyBankId(null);
    }
  };

  const renderMainContent = () => {
    if (activeView === 'banks') {
      return (
        <BanksView
          banks={banksData}
          loading={banksLoading}
          errorMessage={banksError ? 'Unable to load banks.' : null}
          onEdit={(bank) => {
            setBankToEdit(bank);
          }}
          onMarkInactive={(bank) => handleBankInactive(bank.id)}
          onRefresh={() => refetchBanks()}
          busyBankId={busyBankId}
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
        />
      );
    }

    return (
      <TransfersView
        transfers={transfersData}
        loading={transfersLoading}
        errorMessage={transfersError ? 'Unable to load transfers.' : null}
      />
    );
  };

  return (
    <div className="flex min-h-dvh bg-muted/40">
      <aside className="hidden w-72 shrink-0 border-r border-border/60 bg-sidebar/60 lg:flex lg:flex-col">
        <div className="flex h-16 items-center justify-between border-b border-border/60 px-6">
          <div>
            <p className="text-sm font-medium text-sidebar-foreground/70">
              Spending Analyzer
            </p>
            <p className="text-lg font-semibold text-sidebar-foreground">
              Control Center
            </p>
          </div>
          <Button
            size="icon"
            variant="ghost"
            onClick={refetchAll}
            aria-label="Refresh data"
          >
            <RefreshCcwIcon className="size-4" />
          </Button>
        </div>
        <ScrollArea className="flex-1 px-4">
          <nav className="flex flex-col gap-1 py-4">
            {navItems.map((item) => (
              <button
                key={item.key}
                type="button"
                onClick={() => setActiveView(item.key)}
                className={`flex items-center justify-between rounded-md px-3 py-2 text-sm transition-all hover:bg-sidebar-accent hover:text-sidebar-accent-foreground ${
                  activeView === item.key
                    ? 'bg-sidebar-primary text-sidebar-primary-foreground shadow-sm'
                    : 'text-sidebar-foreground'
                }`}
              >
                <span className="flex items-center gap-2">
                  {item.icon}
                  {item.label}
                </span>
                <span className="text-xs font-semibold opacity-70">
                  {numberFormatter.format(item.total ?? 0)}
                </span>
              </button>
            ))}
          </nav>
        </ScrollArea>
        <Separator className="opacity-60" />
        <div className="flex flex-col gap-2 px-4 py-4">
          <Button onClick={() => setIsAddBankOpen(true)}>Add bank</Button>
          <Button
            variant="outline"
            onClick={() => setIsAddTransactionOpen(true)}
          >
            Add transaction
          </Button>
          <Button variant="outline" onClick={() => setIsAddTransferOpen(true)}>
            Add transfer
          </Button>
        </div>
      </aside>

      <main className="flex-1 overflow-y-auto">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-8 p-6">
          <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle>Total balance</CardTitle>
                <BanknoteIcon className="size-5 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                {banksLoading ? (
                  <Skeleton className="h-8 w-32" />
                ) : (
                  <p className="text-2xl font-semibold">
                    {currencyFormatter.format(totalBalance)}
                  </p>
                )}
                <CardDescription className="mt-1">
                  {accounts.length > 0
                    ? `${accounts.length} active ${
                        accounts.length === 1 ? 'account' : 'accounts'
                      }`
                    : 'No active accounts yet'}
                </CardDescription>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle>Active banks</CardTitle>
                <Building2Icon className="size-5 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                {banksLoading ? (
                  <Skeleton className="h-8 w-24" />
                ) : (
                  <p className="text-2xl font-semibold">
                    {numberFormatter.format(
                      (banksData ?? []).filter(
                        (bank: BankResponse) => !bank.isInactive
                      ).length
                    )}
                  </p>
                )}
                <CardDescription className="mt-1">
                  {(banksData ?? []).length > 0
                    ? 'Including inactive banks hidden from totals.'
                    : 'Add your first bank to get started.'}
                </CardDescription>
              </CardContent>
            </Card>

            <Card className="md:col-span-2 xl:col-span-1">
              <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle>Last refresh</CardTitle>
                <RefreshCcwIcon className="size-5 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-semibold">
                  {new Date().toLocaleTimeString()}
                </p>
                <CardDescription className="mt-1">
                  Use the refresh button in the sidebar to sync with the API.
                </CardDescription>
              </CardContent>
            </Card>
          </section>

          <section>{renderMainContent()}</section>
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
          await Promise.allSettled([refetchTransfers(), refetchBanks()]);
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
    </div>
  );
}

export default App;

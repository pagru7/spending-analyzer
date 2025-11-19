import './App.css';

import useAxios from 'axios-hooks';
import {
  BanknoteIcon,
  Building2Icon,
  CoinsIcon,
  RefreshCcwIcon,
  SendIcon,
} from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import {
  Controller,
  useForm,
  type ControllerRenderProps,
} from 'react-hook-form';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { Skeleton } from '@/components/ui/skeleton';
import {
  type BankAccountResponse,
  type BankResponse,
  type CreateTransactionRequest,
  type CreateTransferRequest,
  type TransactionResponse,
  type TransferResponse,
  type UpdateBankRequest,
} from '@/types/api';
import AddBankDialog from './features/AddBankDialog';
import BanksView from './features/BanksView';

type ViewKey = 'banks' | 'transactions' | 'transfers';

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
});

const numberFormatter = new Intl.NumberFormat('en-US', {
  maximumFractionDigits: 0,
});

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

interface TransactionsViewProps {
  transactions?: TransactionResponse[];
  loading: boolean;
  errorMessage: string | null;
}

function TransactionsView({
  transactions,
  loading,
  errorMessage,
}: TransactionsViewProps) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Transactions</CardTitle>
          <CardDescription>
            Recent transactions fetched from the API service.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent>
        {loading ? (
          <div className="space-y-3">
            {[...Array(4).keys()].map((idx) => (
              <Skeleton key={idx} className="h-12 w-full" />
            ))}
          </div>
        ) : errorMessage ? (
          <p className="text-sm text-destructive">{errorMessage}</p>
        ) : !transactions || transactions.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No transactions recorded yet.
          </p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[640px] border-separate border-spacing-y-2 text-sm">
              <thead className="text-left text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-3 py-2 font-medium">Description</th>
                  <th className="px-3 py-2 font-medium">Account</th>
                  <th className="px-3 py-2 font-medium">Recipient</th>
                  <th className="px-3 py-2 font-medium text-right">Amount</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((transaction) => (
                  <tr key={transaction.id} className="rounded-md bg-muted/20">
                    <td className="px-3 py-2 text-sm font-medium">
                      {transaction.description}
                    </td>
                    <td className="px-3 py-2 text-sm">
                      {transaction.accountName}
                    </td>
                    <td className="px-3 py-2 text-sm">
                      {transaction.recipient}
                    </td>
                    <td className="px-3 py-2 text-sm text-right font-semibold">
                      {currencyFormatter.format(transaction.amount)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

interface TransfersViewProps {
  transfers?: TransferResponse[];
  loading: boolean;
  errorMessage: string | null;
}

function TransfersView({
  transfers,
  loading,
  errorMessage,
}: TransfersViewProps) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Transfers</CardTitle>
          <CardDescription>
            Monitor the money moved between your accounts.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent>
        {loading ? (
          <div className="space-y-3">
            {[...Array(3).keys()].map((idx) => (
              <Skeleton key={idx} className="h-12 w-full" />
            ))}
          </div>
        ) : errorMessage ? (
          <p className="text-sm text-destructive">{errorMessage}</p>
        ) : !transfers || transfers.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No transfers recorded yet.
          </p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[640px] border-separate border-spacing-y-2 text-sm">
              <thead className="text-left text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-3 py-2 font-medium">Description</th>
                  <th className="px-3 py-2 font-medium">Source</th>
                  <th className="px-3 py-2 font-medium">Target</th>
                  <th className="px-3 py-2 font-medium text-right">Amount</th>
                </tr>
              </thead>
              <tbody>
                {transfers.map((transfer) => (
                  <tr key={transfer.id} className="rounded-md bg-muted/20">
                    <td className="px-3 py-2 text-sm font-medium">
                      {transfer.description}
                    </td>
                    <td className="px-3 py-2 text-sm">
                      {transfer.sourceAccountName}
                    </td>
                    <td className="px-3 py-2 text-sm">
                      {transfer.targetAccountName}
                    </td>
                    <td className="px-3 py-2 text-sm text-right font-semibold">
                      {currencyFormatter.format(transfer.value)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

interface AddTransactionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accounts: BankAccountResponse[];
  onSuccess: () => Promise<void> | void;
}

type TransactionFormValues = {
  description: string;
  accountId: string;
  recipient: string;
  amount: string;
  isSpending: boolean;
};

function AddTransactionDialog({
  open,
  onOpenChange,
  accounts,
  onSuccess,
}: AddTransactionDialogProps) {
  const form = useForm<TransactionFormValues>({
    defaultValues: {
      description: '',
      accountId: '',
      recipient: '',
      amount: '',
      isSpending: true,
    },
  });

  const [{ loading, error }, executeCreateTransaction] =
    useAxios<TransactionResponse>(
      { url: '/api/transactions', method: 'POST' },
      { manual: true }
    );

  const handleSubmit = form.handleSubmit(
    async (values: TransactionFormValues) => {
      const parsedAmount = Number.parseFloat(values.amount || '0');
      const normalizedAmount = Number.isNaN(parsedAmount)
        ? 0
        : values.isSpending
        ? -Math.abs(parsedAmount)
        : Math.abs(parsedAmount);

      const payload: CreateTransactionRequest = {
        description: values.description.trim(),
        accountId: values.accountId,
        recipient: values.recipient.trim(),
        amount: normalizedAmount,
      };

      try {
        await executeCreateTransaction({ data: payload });
        await onSuccess();
        form.reset({
          description: '',
          accountId: '',
          recipient: '',
          amount: '',
          isSpending: true,
        });
        onOpenChange(false);
      } catch {
        // axios-hooks surfaces error state for UI feedback
      }
    }
  );

  return (
    <Dialog
      open={open}
      onOpenChange={(state: boolean) => {
        if (!loading) {
          onOpenChange(state);
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New transaction</DialogTitle>
          <DialogDescription>
            Record a payment made from one of your accounts.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="transaction-description">Description</Label>
            <Input
              id="transaction-description"
              placeholder="Grocery store"
              {...form.register('description', {
                required: 'Description is required',
              })}
            />
            {form.formState.errors.description ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.description.message}
              </p>
            ) : null}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-account">Account</Label>
            <Controller<TransactionFormValues, 'accountId'>
              control={form.control}
              name="accountId"
              rules={{ required: 'Select an account' }}
              render={({
                field,
              }: {
                field: ControllerRenderProps<
                  TransactionFormValues,
                  'accountId'
                >;
              }) => (
                <Select
                  value={field.value}
                  onValueChange={field.onChange}
                  disabled={accounts.length === 0 || loading}
                >
                  <SelectTrigger id="transaction-account">
                    <SelectValue
                      placeholder={
                        accounts.length === 0
                          ? 'No active accounts available'
                          : 'Choose account'
                      }
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {accounts.map((account) => (
                      <SelectItem key={account.id} value={account.id}>
                        {account.bankName
                          ? `${account.bankName} - ${account.name}`
                          : account.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {form.formState.errors.accountId ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.accountId.message}
              </p>
            ) : null}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-recipient">Recipient</Label>
            <Input
              id="transaction-recipient"
              placeholder="Store or person"
              {...form.register('recipient', {
                required: 'Recipient is required',
              })}
            />
            {form.formState.errors.recipient ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.recipient.message}
              </p>
            ) : null}
          </div>

          <Controller<TransactionFormValues, 'isSpending'>
            control={form.control}
            name="isSpending"
            render={({
              field,
            }: {
              field: ControllerRenderProps<TransactionFormValues, 'isSpending'>;
            }) => (
              <div className="flex items-center gap-2">
                <input
                  id="transaction-spending"
                  type="checkbox"
                  className="size-4 rounded border border-input/80"
                  checked={field.value}
                  onChange={(event) => {
                    const checked = event.target.checked;
                    const currentAmount = form.getValues('amount');
                    if (currentAmount.trim().length > 0) {
                      const numericValue = Number.parseFloat(currentAmount);
                      if (!Number.isNaN(numericValue)) {
                        const normalized = Math.abs(numericValue);
                        const nextAmount = checked ? -normalized : normalized;
                        form.setValue('amount', nextAmount.toString(), {
                          shouldDirty: true,
                          shouldValidate: true,
                        });
                      }
                    }
                    field.onChange(checked);
                  }}
                  onBlur={field.onBlur}
                  ref={field.ref}
                />
                <Label htmlFor="transaction-spending" className="m-0">
                  Spending
                </Label>
                <span className="text-xs text-muted-foreground">
                  (uncheck for income)
                </span>
              </div>
            )}
          />

          <div className="space-y-2">
            <Label htmlFor="transaction-amount">Amount</Label>
            <Input
              id="transaction-amount"
              type="number"
              step="0.01"
              placeholder="0.00"
              {...form.register('amount', {
                required: 'Amount is required',
                validate: (value: string) => {
                  const numericValue = Number.parseFloat(value);
                  if (Number.isNaN(numericValue)) {
                    return 'Enter a valid number';
                  }
                  return (
                    Math.abs(numericValue) > 0 ||
                    'Amount must be greater than zero'
                  );
                },
              })}
            />
            {form.formState.errors.amount ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.amount.message}
              </p>
            ) : null}
          </div>

          {error ? (
            <p className="text-sm text-destructive">
              Could not create the transaction.
            </p>
          ) : null}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading || accounts.length === 0}>
              {loading ? 'Saving...' : 'Save transaction'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

interface AddTransferDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accounts: BankAccountResponse[];
  onSuccess: () => Promise<void> | void;
}

type TransferFormValues = {
  description: string;
  sourceAccountId: string;
  targetAccountId: string;
  value: string;
};

function AddTransferDialog({
  open,
  onOpenChange,
  accounts,
  onSuccess,
}: AddTransferDialogProps) {
  const form = useForm<TransferFormValues>({
    defaultValues: {
      description: '',
      sourceAccountId: '',
      targetAccountId: '',
      value: '',
    },
  });

  const [{ loading, error }, executeCreateTransfer] =
    useAxios<TransferResponse>(
      { url: '/api/transfers', method: 'POST' },
      { manual: true }
    );

  const handleSubmit = form.handleSubmit(async (values: TransferFormValues) => {
    const payload: CreateTransferRequest = {
      description: values.description.trim(),
      sourceAccountId: values.sourceAccountId,
      targetAccountId: values.targetAccountId,
      value: Number.parseFloat(values.value || '0'),
    };

    try {
      await executeCreateTransfer({ data: payload });
      await onSuccess();
      form.reset({
        description: '',
        sourceAccountId: '',
        targetAccountId: '',
        value: '',
      });
      onOpenChange(false);
    } catch {
      // axios-hooks surfaces error state for UI feedback
    }
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(state: boolean) => {
        if (!loading) {
          onOpenChange(state);
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New transfer</DialogTitle>
          <DialogDescription>
            Move money between any two of your accounts.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="transfer-description">Description</Label>
            <Input
              id="transfer-description"
              placeholder="Monthly savings transfer"
              {...form.register('description', {
                required: 'Description is required',
              })}
            />
            {form.formState.errors.description ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.description.message}
              </p>
            ) : null}
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label>Source account</Label>
              <Controller<TransferFormValues, 'sourceAccountId'>
                control={form.control}
                name="sourceAccountId"
                rules={{ required: 'Select a source account' }}
                render={({
                  field,
                }: {
                  field: ControllerRenderProps<
                    TransferFormValues,
                    'sourceAccountId'
                  >;
                }) => (
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={accounts.length === 0 || loading}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose source" />
                    </SelectTrigger>
                    <SelectContent>
                      {accounts.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.bankName
                            ? `${account.bankName} - ${account.name}`
                            : account.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
              {form.formState.errors.sourceAccountId ? (
                <p className="text-xs text-destructive">
                  {form.formState.errors.sourceAccountId.message}
                </p>
              ) : null}
            </div>
            <div className="space-y-2">
              <Label>Target account</Label>
              <Controller<TransferFormValues, 'targetAccountId'>
                control={form.control}
                name="targetAccountId"
                rules={{
                  required: 'Select a target account',
                  validate: (value: string) =>
                    value !== form.getValues('sourceAccountId') ||
                    'Source and target must be different accounts',
                }}
                render={({
                  field,
                }: {
                  field: ControllerRenderProps<
                    TransferFormValues,
                    'targetAccountId'
                  >;
                }) => (
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={accounts.length === 0 || loading}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose target" />
                    </SelectTrigger>
                    <SelectContent>
                      {accounts.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.bankName
                            ? `${account.bankName} - ${account.name}`
                            : account.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
              {form.formState.errors.targetAccountId ? (
                <p className="text-xs text-destructive">
                  {form.formState.errors.targetAccountId.message}
                </p>
              ) : null}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="transfer-value">Amount</Label>
            <Input
              id="transfer-value"
              type="number"
              step="0.01"
              min="0"
              placeholder="0.00"
              {...form.register('value', {
                required: 'Amount is required',
                validate: (value: string) =>
                  Number.parseFloat(value) > 0 ||
                  'Amount must be greater than zero',
              })}
            />
            {form.formState.errors.value ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.value.message}
              </p>
            ) : null}
          </div>

          {error ? (
            <p className="text-sm text-destructive">
              Could not create the transfer.
            </p>
          ) : null}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading || accounts.length < 2}>
              {loading ? 'Saving...' : 'Save transfer'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

interface UpdateBankDialogProps {
  bank: BankResponse | null;
  onClose: () => void;
  onSubmit: (payload: UpdateBankRequest) => Promise<void>;
  busy: boolean;
}

function UpdateBankDialog({
  bank,
  onClose,
  onSubmit,
  busy,
}: UpdateBankDialogProps) {
  const form = useForm<UpdateBankRequest>({
    defaultValues: { name: bank?.name ?? '' },
  });

  const open = Boolean(bank);

  useEffect(() => {
    if (bank) {
      form.reset({ name: bank.name });
    } else {
      form.reset({ name: '' });
    }
  }, [bank, form]);

  const submitHandler = form.handleSubmit(async (values: UpdateBankRequest) => {
    if (!bank) return;
    await onSubmit({ name: values.name.trim() });
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(state: boolean) => {
        if (!state && !busy) {
          onClose();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit bank</DialogTitle>
          <DialogDescription>
            Update the name of the bank as it should appear in the app.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={submitHandler}>
          <div className="space-y-2">
            <Label htmlFor="update-bank-name">Bank name</Label>
            <Input
              id="update-bank-name"
              {...form.register('name', { required: 'Name is required' })}
            />
            {form.formState.errors.name ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.name.message}
              </p>
            ) : null}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={busy}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={busy}>
              {busy ? 'Saving...' : 'Save changes'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default App;

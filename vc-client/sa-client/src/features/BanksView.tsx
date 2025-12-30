import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { currencyFormatter } from '@/lib/formatters';
import type { BankAccountResponse, BankResponse } from '@/types/api';
import { Trash2Icon } from 'lucide-react';

interface BanksViewProps {
  banks?: BankResponse[];
  loading: boolean;
  errorMessage: string | null;
  onEdit: (bank: BankResponse) => void;
  onMarkInactive: (bank: BankResponse) => void;
  onAddAccount: (bank: BankResponse) => void;
  onDeleteAccount: (account: BankAccountResponse, bankId: number) => void;
  onRefresh: () => Promise<unknown>;
  busyBankId: string | null;
  busyAccountId: string | null;
  actionError: string | null;
}

const BanksView = ({
  banks,
  loading,
  errorMessage,
  onEdit,
  onMarkInactive,
  onAddAccount,
  onDeleteAccount,
  onRefresh,
  busyBankId,
  busyAccountId,
  actionError,
}: BanksViewProps) => {
  if (loading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Banks</CardTitle>
          <CardDescription>Loading data from the API...</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          {[...Array(3).keys()].map((idx) => (
            <div key={idx} className="space-y-2">
              <Skeleton className="h-5 w-48" />
              <Skeleton className="h-4 w-64" />
            </div>
          ))}
        </CardContent>
      </Card>
    );
  }

  if (errorMessage) {
    return (
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Banks</CardTitle>
            <CardDescription>{errorMessage}</CardDescription>
          </div>
          <Button onClick={() => onRefresh()} size="sm" variant="outline">
            Retry
          </Button>
        </CardHeader>
      </Card>
    );
  }

  if (!banks || banks.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Banks</CardTitle>
          <CardDescription>
            Add a bank to start tracking balances.
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-start justify-between gap-4">
        <div>
          <CardTitle>Banks</CardTitle>
          <CardDescription>
            Expand a bank to see its accounts and balances.
          </CardDescription>
        </div>
        <Button
          onClick={() => onRefresh()}
          size="sm"
          variant="outline"
          className="mt-1"
        >
          Refresh
        </Button>
      </CardHeader>
      <CardContent>
        {actionError ? (
          <p className="mb-4 text-sm text-destructive">{actionError}</p>
        ) : null}
        <Accordion type="multiple" className="divide-y divide-border/60">
          {banks.map((bank) => (
            <AccordionItem
              key={bank.id}
              value={bank.id}
              className="border-none"
            >
              <AccordionTrigger>
                <span className="flex flex-col items-start gap-1">
                  <span className="text-base font-semibold">
                    {bank.name}
                    {bank.isInactive ? (
                      <span className="ml-2 text-xs font-medium text-muted-foreground">
                        (inactive)
                      </span>
                    ) : null}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    {
                      (bank.bankAccounts || []).filter((acc) => !acc.isInactive)
                        .length
                    }{' '}
                    active accounts
                  </span>
                </span>
              </AccordionTrigger>
              <AccordionContent>
                <div className="space-y-4">
                  <div className="flex flex-wrap gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => onEdit(bank)}
                    >
                      Edit bank
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => onAddAccount(bank)}
                      disabled={bank.isInactive}
                    >
                      Add account
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      className="text-destructive hover:text-destructive"
                      onClick={() => onMarkInactive(bank)}
                      disabled={busyBankId === bank.id || bank.isInactive}
                    >
                      {busyBankId === bank.id
                        ? 'Updating...'
                        : bank.isInactive
                        ? 'Bank inactive'
                        : 'Mark inactive'}
                    </Button>
                  </div>

                  <div className="space-y-3">
                    {(bank.bankAccounts ?? []).length === 0 ? (
                      <p className="text-sm text-muted-foreground">
                        No accounts registered under this bank yet.
                      </p>
                    ) : (
                      (bank.bankAccounts ?? []).map((account) => (
                        <div
                          key={account.id}
                          className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border/60 bg-muted/10 px-3 py-2"
                        >
                          <div>
                            <p className="text-sm font-medium">
                              {account.name}
                              {account.isInactive ? (
                                <span className="ml-2 text-xs text-muted-foreground">
                                  (inactive)
                                </span>
                              ) : null}
                            </p>
                            <p className="text-xs text-muted-foreground">
                              Created{' '}
                              {new Date(
                                account.creationDate
                              ).toLocaleDateString()}
                            </p>
                          </div>
                          <div className="flex items-center gap-2">
                            <p className="text-sm font-semibold">
                              {currencyFormatter.format(account.balance)}
                            </p>
                            <Button
                              size="icon-sm"
                              variant="ghost"
                              className="text-destructive hover:text-destructive"
                              onClick={() => onDeleteAccount(account, bank.id)}
                              disabled={
                                busyAccountId === account.id ||
                                account.isInactive
                              }
                              title="Delete account"
                            >
                              {busyAccountId === account.id ? (
                                <span className="text-xs">...</span>
                              ) : (
                                <Trash2Icon className="size-4" />
                              )}
                            </Button>
                          </div>
                        </div>
                      ))
                    )}
                  </div>
                </div>
              </AccordionContent>
            </AccordionItem>
          ))}
        </Accordion>
      </CardContent>
    </Card>
  );
};

export default BanksView;

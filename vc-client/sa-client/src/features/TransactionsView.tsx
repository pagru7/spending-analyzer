import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { currencyFormatter } from '@/lib/formatters';
import type { TransactionResponse } from '@/types/api';

interface TransactionsViewProps {
  transactions?: TransactionResponse[];
  loading: boolean;
  errorMessage: string | null;
}

const TransactionsView = ({
  transactions,
  loading,
  errorMessage,
}: TransactionsViewProps) => {
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
};

export default TransactionsView;

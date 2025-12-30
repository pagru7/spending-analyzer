import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { currencyFormatter } from '@/lib/formatters';
import type { TransactionResponse } from '@/types/api';
import { PencilIcon } from 'lucide-react';

interface TransactionsViewProps {
  transactions?: TransactionResponse[];
  loading: boolean;
  errorMessage: string | null;
  onEdit: (transaction: TransactionResponse) => void;
}

const TransactionsView = ({
  transactions,
  loading,
  errorMessage,
  onEdit,
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
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>ID</TableHead>
                <TableHead>Date</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>Account</TableHead>
                <TableHead>Recipient</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {transactions.map((transaction) => (
                <TableRow key={transaction.id}>
                  <TableCell className="font-medium">
                    #{transaction.id}
                  </TableCell>
                  <TableCell>
                    {new Date(transaction.transactionDate).toLocaleDateString()}
                  </TableCell>
                  <TableCell>{transaction.description}</TableCell>
                  <TableCell>
                    {transaction.accountName}
                  </TableCell>
                  <TableCell>{transaction.recipient}</TableCell>
                  <TableCell className="text-right font-semibold">
                    {currencyFormatter.format(transaction.amount)}
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      size="icon-sm"
                      variant="ghost"
                      onClick={() => onEdit(transaction)}
                      title="Edit transaction"
                    >
                      <PencilIcon className="size-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
};

export default TransactionsView;

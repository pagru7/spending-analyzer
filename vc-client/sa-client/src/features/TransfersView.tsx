import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { currencyFormatter } from '@/lib/formatters';
import type { TransferResponse } from '@/types/api';

interface TransfersViewProps {
  transfers?: TransferResponse[];
  loading: boolean;
  errorMessage: string | null;
}

const TransfersView = ({
  transfers,
  loading,
  errorMessage,
}: TransfersViewProps) => {
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
};

export default TransfersView;

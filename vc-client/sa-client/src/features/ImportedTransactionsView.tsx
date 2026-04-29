import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import type { ImportedTransactionResponse } from '@/types/api';
import { useEffect, useMemo, useState } from 'react';

const PAGE_SIZE = 5;

interface ImportedTransactionsViewProps {
  transactions?: ImportedTransactionResponse[];
  loading: boolean;
  errorMessage: string | null;
}

const ImportedTransactionsView = ({
  transactions,
  loading,
  errorMessage,
}: ImportedTransactionsViewProps) => {
  const [currentPage, setCurrentPage] = useState(1);

  const totalPages = Math.max(
    1,
    Math.ceil((transactions?.length ?? 0) / PAGE_SIZE),
  );

  useEffect(() => {
    setCurrentPage((previousPage) => Math.min(previousPage, totalPages));
  }, [totalPages]);

  const currentPageTransactions = useMemo(() => {
    if (!transactions) {
      return [];
    }

    const startIndex = (currentPage - 1) * PAGE_SIZE;
    return transactions.slice(startIndex, startIndex + PAGE_SIZE);
  }, [currentPage, transactions]);

  const canGoToPreviousPage = currentPage > 1;
  const canGoToNextPage = currentPage < totalPages;

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Imported Transactions</CardTitle>
          <CardDescription>
            Raw transactions imported from external sources.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent>
        {loading ? (
          <div className="flex flex-col gap-3">
            {[...Array(3).keys()].map((idx) => (
              <Skeleton key={idx} className="h-12 w-full" />
            ))}
          </div>
        ) : errorMessage ? (
          <p className="text-sm text-destructive">{errorMessage}</p>
        ) : !transactions || transactions.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No imported transactions found.
          </p>
        ) : (
          <div className="flex flex-col gap-4">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[1200px] border-separate border-spacing-y-2 text-sm">
                <thead className="text-left text-xs uppercase text-muted-foreground">
                  <tr>
                    <th className="px-3 py-2 font-medium">ID</th>
                    <th className="px-3 py-2 font-medium">External ID</th>
                    <th className="px-3 py-2 font-medium">Ext. ID (parsed)</th>
                    <th className="px-3 py-2 font-medium">Date</th>
                    <th className="px-3 py-2 font-medium">Type</th>
                    <th className="px-3 py-2 font-medium">Description</th>
                    <th className="px-3 py-2 font-medium">Description 2</th>
                    <th className="px-3 py-2 font-medium">Issuer name</th>
                    <th className="px-3 py-2 font-medium">Issuer account</th>
                    <th className="px-3 py-2 font-medium">Account</th>
                    <th className="px-3 py-2 font-medium text-right">Amount</th>
                    <th className="px-3 py-2 font-medium text-right">
                      Currency
                    </th>
                    <th className="px-3 py-2 font-medium text-right">
                      Balance
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {currentPageTransactions.map((tx) => (
                    <tr key={tx.id} className="rounded-md bg-muted/20">
                      <td className="px-3 py-2">{tx.id}</td>
                      <td className="px-3 py-2 font-mono text-xs">
                        {tx.externalId}
                      </td>
                      <td className="px-3 py-2 text-center">
                        {tx.externalIdParsed ?? '—'}
                      </td>
                      <td className="px-3 py-2 whitespace-nowrap">
                        {tx.issueDate}
                      </td>
                      <td className="px-3 py-2">{tx.type}</td>
                      <td className="px-3 py-2 font-medium">
                        {tx.description}
                      </td>
                      <td className="px-3 py-2 text-xs">{tx.description2}</td>
                      <td className="px-3 py-2">{tx.issuerName}</td>
                      <td className="px-3 py-2 font-mono text-xs">
                        {tx.issuerBankAccountNumber}
                      </td>
                      <td className="px-3 py-2">{tx.accountName}</td>
                      <td className="px-3 py-2 text-right font-semibold whitespace-nowrap">
                        {tx.amount}
                      </td>
                      <td className="px-3 py-2 text-right">{tx.currency}</td>
                      <td className="px-3 py-2 text-right">{tx.balance}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {totalPages > 1 ? (
              <div className="flex items-center justify-between gap-4">
                <p className="text-sm text-muted-foreground">
                  Page {currentPage} of {totalPages}
                </p>
                <Pagination className="mx-0 w-auto">
                  <PaginationContent>
                    <PaginationItem>
                      <PaginationPrevious
                        href="#"
                        aria-disabled={!canGoToPreviousPage}
                        tabIndex={canGoToPreviousPage ? 0 : -1}
                        className={cn(
                          !canGoToPreviousPage &&
                            'pointer-events-none opacity-50',
                        )}
                        onClick={(event) => {
                          event.preventDefault();

                          if (canGoToPreviousPage) {
                            setCurrentPage((page) => page - 1);
                          }
                        }}
                      />
                    </PaginationItem>
                    <PaginationItem>
                      <PaginationNext
                        href="#"
                        aria-disabled={!canGoToNextPage}
                        tabIndex={canGoToNextPage ? 0 : -1}
                        className={cn(
                          !canGoToNextPage && 'pointer-events-none opacity-50',
                        )}
                        onClick={(event) => {
                          event.preventDefault();

                          if (canGoToNextPage) {
                            setCurrentPage((page) => page + 1);
                          }
                        }}
                      />
                    </PaginationItem>
                  </PaginationContent>
                </Pagination>
              </div>
            ) : null}
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default ImportedTransactionsView;

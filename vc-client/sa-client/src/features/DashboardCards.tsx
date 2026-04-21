import { BanknoteIcon, Building2Icon, RefreshCcwIcon } from 'lucide-react';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { currencyFormatter, numberFormatter } from '@/lib/formatters';

interface DashboardCardsProps {
  totalBalance: number;
  accountCount: number;
  activeBankCount: number;
  allBankCount: number;
  loading: boolean;
}

const DashboardCards = ({
  totalBalance,
  accountCount,
  activeBankCount,
  allBankCount,
  loading,
}: DashboardCardsProps) => (
  <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Total balance</CardTitle>
        <BanknoteIcon className="size-5 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        {loading ? (
          <Skeleton className="h-8 w-32" />
        ) : (
          <p className="text-2xl font-semibold">
            {currencyFormatter.format(totalBalance)}
          </p>
        )}
        <CardDescription className="mt-1">
          {accountCount > 0
            ? `${accountCount} active ${accountCount === 1 ? 'account' : 'accounts'}`
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
        {loading ? (
          <Skeleton className="h-8 w-24" />
        ) : (
          <p className="text-2xl font-semibold">
            {numberFormatter.format(activeBankCount)}
          </p>
        )}
        <CardDescription className="mt-1">
          {allBankCount > 0
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
);

export default DashboardCards;

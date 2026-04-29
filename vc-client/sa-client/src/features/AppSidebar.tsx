import {
  Building2Icon,
  CoinsIcon,
  RefreshCcwIcon,
  SendIcon,
} from 'lucide-react';
import { useMemo } from 'react';

import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import { numberFormatter } from '@/lib/formatters';
import type { ViewKey } from '@/types/app';

interface SidebarActions {
  onAddBank: () => void;
  onAddTransaction: () => void;
  onAddTransfer: () => void;
  onImportTransactions: () => void;
}

interface AppSidebarProps {
  activeView: ViewKey;
  onViewChange: (view: ViewKey) => void;
  onRefresh: () => void;
  counts: { banks: number; transactions: number; importedTransactions: number };
  actions: SidebarActions;
}

const NAV_ITEMS: { key: ViewKey; label: string; icon: React.ReactNode }[] = [
  { key: 'banks', label: 'Banks', icon: <Building2Icon className="size-4" /> },
  {
    key: 'transactions',
    label: 'Transactions',
    icon: <CoinsIcon className="size-4" />,
  },
  {
    key: 'importedTransactions',
    label: 'Imported transactions',
    icon: <SendIcon className="size-4" />,
  },
];

const AppSidebar = ({
  activeView,
  onViewChange,
  onRefresh,
  counts,
  actions,
}: AppSidebarProps) => {
  const navItems = useMemo(
    () =>
      NAV_ITEMS.map((item) => ({
        ...item,
        total: counts[item.key] ?? 0,
      })),
    [counts],
  );

  return (
    <aside className="hidden h-full w-72 shrink-0 overflow-hidden border-r border-border/60 bg-sidebar/60 lg:sticky lg:top-0 lg:flex lg:flex-col">
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
          onClick={onRefresh}
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
              onClick={() => onViewChange(item.key)}
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
                {numberFormatter.format(item.total)}
              </span>
            </button>
          ))}
        </nav>
      </ScrollArea>
      <Separator className="opacity-60" />
      <div className="flex flex-col gap-2 px-4 py-4">
        <Button onClick={actions.onAddBank}>Add bank</Button>
        <Button variant="outline" onClick={actions.onAddTransaction}>
          Add transaction
        </Button>
        <Button variant="outline" onClick={actions.onAddTransfer}>
          Add transfer
        </Button>
        <Button variant="outline" onClick={actions.onImportTransactions}>
          Import transactions
        </Button>
      </div>
    </aside>
  );
};

export default AppSidebar;

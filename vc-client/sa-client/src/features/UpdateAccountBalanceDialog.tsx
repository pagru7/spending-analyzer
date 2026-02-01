import { Button } from '@/components/ui/button';
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
import type {
  BankAccountResponse,
  UpdateAccountBalanceRequest,
} from '@/types/api';
import useAxios from 'axios-hooks';
import { useEffect, useState } from 'react';

interface UpdateAccountBalanceDialogProps {
  account: BankAccountResponse | null;
  bankId: string | null;
  onClose: () => void;
  onSuccess: () => Promise<unknown>;
}

const UpdateAccountBalanceDialog = ({
  account,
  bankId,
  onClose,
  onSuccess,
}: UpdateAccountBalanceDialogProps) => {
  const [balance, setBalance] = useState('');
  const [error, setError] = useState<string | null>(null);

  const [{ loading }, executeUpdate] = useAxios<BankAccountResponse>(
    {
      method: 'PATCH',
    },
    { manual: true }
  );

  useEffect(() => {
    if (account) {
      setBalance(account.balance.toString());
      setError(null);
    }
  }, [account]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!account || !bankId) return;

    const balanceValue = Number.parseFloat(balance);
    if (Number.isNaN(balanceValue)) {
      setError('Please enter a valid number');
      return;
    }

    try {
      const payload: UpdateAccountBalanceRequest = {
        newBalance: balanceValue,
      };

      await executeUpdate({
        url: `/api/banks/${bankId}/accounts/${account.id}/balance`,
        data: payload,
      });

      await onSuccess();
      onClose();
    } catch {
      setError('Failed to update balance. Please try again.');
    }
  };

  const handleClose = () => {
    if (!loading) {
      setError(null);
      onClose();
    }
  };

  return (
    <Dialog open={account !== null} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Update account balance</DialogTitle>
          <DialogDescription>
            Update the current balance for {account?.name}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="balance">Current balance</Label>
              <Input
                id="balance"
                type="number"
                step="0.01"
                value={balance}
                onChange={(e) => setBalance(e.target.value)}
                placeholder="0.00"
                disabled={loading}
                required
                autoFocus
              />
            </div>
            {error ? <p className="text-sm text-destructive">{error}</p> : null}
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? 'Updating...' : 'Update balance'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default UpdateAccountBalanceDialog;

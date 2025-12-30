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
import type { BankAccountResponse, BankResponse } from '@/types/api';
import useAxios from 'axios-hooks';
import { useForm } from 'react-hook-form';

interface AddAccountDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => Promise<void> | void;
  bank: BankResponse | null;
}

type AccountFormValues = {
  name: string;
  balance: string;
};

const AddAccountDialog = ({
  open,
  onOpenChange,
  onSuccess,
  bank,
}: AddAccountDialogProps) => {
  const form = useForm<AccountFormValues>({
    defaultValues: {
      name: '',
      balance: '0',
    },
  });

  const [{ loading, error }, executeCreateAccount] =
    useAxios<BankAccountResponse>({ method: 'POST' }, { manual: true });

  const handleSubmit = form.handleSubmit(async (values: AccountFormValues) => {
    if (!bank) return;

    const payload = {
      name: values.name.trim(),
      balance: Number.parseFloat(values.balance || '0'),
    };

    try {
      await executeCreateAccount({
        url: `/api/banks/${bank.id}/accounts`,
        data: payload,
      });
      await onSuccess();
      form.reset({ name: '', balance: '0' });
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
          if (!state) {
            form.reset();
          }
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add account to {bank?.name}</DialogTitle>
          <DialogDescription>Add a new account to this bank.</DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="account-name">Account name</Label>
            <Input
              id="account-name"
              placeholder="Checking"
              autoComplete="off"
              {...form.register('name', { required: 'Name is required' })}
            />
            {form.formState.errors.name ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.name.message}
              </p>
            ) : null}
          </div>

          <div className="space-y-2">
            <Label htmlFor="account-balance">Starting balance</Label>
            <Input
              id="account-balance"
              type="number"
              step="0.01"
              placeholder="0.00"
              autoComplete="off"
              {...form.register('balance', {
                required: 'Balance is required',
                validate: (value: string) =>
                  !Number.isNaN(Number.parseFloat(value)) ||
                  'Enter a valid number',
              })}
            />
            {form.formState.errors.balance ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.balance.message}
              </p>
            ) : null}
          </div>

          {error ? (
            <p className="text-sm text-destructive">
              Failed to add account. Please try again.
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
            <Button type="submit" disabled={loading}>
              {loading ? 'Adding...' : 'Add account'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default AddAccountDialog;

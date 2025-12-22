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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type {
  BankAccountResponse,
  CreateTransactionRequest,
  TransactionResponse,
} from '@/types/api';
import useAxios from 'axios-hooks';
import {
  Controller,
  useForm,
  type ControllerRenderProps,
} from 'react-hook-form';

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

const AddTransactionDialog = ({
  open,
  onOpenChange,
  accounts,
  onSuccess,
}: AddTransactionDialogProps) => {
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
};

export default AddTransactionDialog;

import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
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
  transactionDate: string;
  transactionFee: string;
  isIncome: boolean;
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
      transactionDate: new Date().toISOString().split('T')[0],
      transactionFee: '',
      isIncome: false,
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
      const parsedFee = values.transactionFee.trim()
        ? Number.parseFloat(values.transactionFee)
        : undefined;

      const payload: CreateTransactionRequest = {
        accountId: Number.parseInt(values.accountId, 10),
        amount: parsedAmount,
        description: values.description.trim(),
        recipient: values.recipient.trim(),
        transactionDate: values.transactionDate,
        transactionFee: parsedFee,
        isIncome: values.isIncome,
      };

      try {
        await executeCreateTransaction({ data: payload });
        await onSuccess();
        form.reset({
          description: '',
          accountId: '',
          recipient: '',
          amount: '',
          transactionDate: new Date().toISOString().split('T')[0],
          transactionFee: '',
          isIncome: false,
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
              autoComplete="off"
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
                      <SelectItem key={account.id} value={String(account.id)}>
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
              autoComplete="off"
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

          <div className="space-y-2">
            <Label htmlFor="transaction-date">Transaction Date</Label>
            <Input
              id="transaction-date"
              type="date"
              {...form.register('transactionDate', {
                required: 'Transaction date is required',
              })}
            />
            {form.formState.errors.transactionDate ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.transactionDate.message}
              </p>
            ) : null}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-amount">Amount</Label>
            <Input
              id="transaction-amount"
              type="number"
              step="0.01"
              placeholder="0.00 (positive for income, negative for expense)"
              {...form.register('amount', {
                required: 'Amount is required',
                validate: (value: string) => {
                  const numericValue = Number.parseFloat(value);
                  if (Number.isNaN(numericValue)) {
                    return 'Enter a valid number';
                  }
                  return numericValue !== 0 || 'Amount must not be zero';
                },
              })}
            />
            {form.formState.errors.amount ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.amount.message}
              </p>
            ) : null}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-fee">Transaction Fee (optional)</Label>
            <Input
              id="transaction-fee"
              type="number"
              step="0.01"
              placeholder="0.00"
              {...form.register('transactionFee', {
                validate: (value: string) => {
                  if (!value.trim()) return true;
                  const numericValue = Number.parseFloat(value);
                  if (Number.isNaN(numericValue)) {
                    return 'Enter a valid number';
                  }
                  return numericValue >= 0 || 'Fee must be zero or positive';
                },
              })}
            />
            {form.formState.errors.transactionFee ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.transactionFee.message}
              </p>
            ) : null}
          </div>

          <div className="flex items-center space-x-2">
            <Controller<TransactionFormValues, 'isIncome'>
              control={form.control}
              name="isIncome"
              render={({ field }) => (
                <Checkbox
                  id="transaction-is-income"
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              )}
            />
            <Label
              htmlFor="transaction-is-income"
              className="text-sm font-normal cursor-pointer"
            >
              Is Income
            </Label>
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

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
  CreateTransferRequest,
  TransferResponse,
} from '@/types/api';
import useAxios from 'axios-hooks';
import {
  Controller,
  useForm,
  type ControllerRenderProps,
} from 'react-hook-form';

interface AddTransferDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accounts: BankAccountResponse[];
  onSuccess: () => Promise<void> | void;
}

type TransferFormValues = {
  description: string;
  sourceAccountId: string;
  targetAccountId: string;
  value: string;
};

const AddTransferDialog = ({
  open,
  onOpenChange,
  accounts,
  onSuccess,
}: AddTransferDialogProps) => {
  const form = useForm<TransferFormValues>({
    defaultValues: {
      description: '',
      sourceAccountId: '',
      targetAccountId: '',
      value: '',
    },
  });

  const [{ loading, error }, executeCreateTransfer] =
    useAxios<TransferResponse>(
      { url: '/api/transfers', method: 'POST' },
      { manual: true }
    );

  const handleSubmit = form.handleSubmit(async (values: TransferFormValues) => {
    const payload: CreateTransferRequest = {
      description: values.description.trim(),
      sourceAccountId: values.sourceAccountId,
      targetAccountId: values.targetAccountId,
      value: Number.parseFloat(values.value || '0'),
    };

    try {
      await executeCreateTransfer({ data: payload });
      await onSuccess();
      form.reset({
        description: '',
        sourceAccountId: '',
        targetAccountId: '',
        value: '',
      });
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
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New transfer</DialogTitle>
          <DialogDescription>
            Move money between any two of your accounts.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="transfer-description">Description</Label>
            <Input
              id="transfer-description"
              placeholder="Monthly savings transfer"
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

          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label>Source account</Label>
              <Controller<TransferFormValues, 'sourceAccountId'>
                control={form.control}
                name="sourceAccountId"
                rules={{ required: 'Select a source account' }}
                render={({
                  field,
                }: {
                  field: ControllerRenderProps<
                    TransferFormValues,
                    'sourceAccountId'
                  >;
                }) => (
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={accounts.length === 0 || loading}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose source" />
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
              {form.formState.errors.sourceAccountId ? (
                <p className="text-xs text-destructive">
                  {form.formState.errors.sourceAccountId.message}
                </p>
              ) : null}
            </div>
            <div className="space-y-2">
              <Label>Target account</Label>
              <Controller<TransferFormValues, 'targetAccountId'>
                control={form.control}
                name="targetAccountId"
                rules={{
                  required: 'Select a target account',
                  validate: (value: string) =>
                    value !== form.getValues('sourceAccountId') ||
                    'Source and target must be different accounts',
                }}
                render={({
                  field,
                }: {
                  field: ControllerRenderProps<
                    TransferFormValues,
                    'targetAccountId'
                  >;
                }) => (
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={accounts.length === 0 || loading}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose target" />
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
              {form.formState.errors.targetAccountId ? (
                <p className="text-xs text-destructive">
                  {form.formState.errors.targetAccountId.message}
                </p>
              ) : null}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="transfer-value">Amount</Label>
            <Input
              id="transfer-value"
              type="number"
              step="0.01"
              min="0"
              placeholder="0.00"
              {...form.register('value', {
                required: 'Amount is required',
                validate: (value: string) =>
                  Number.parseFloat(value) > 0 ||
                  'Amount must be greater than zero',
              })}
            />
            {form.formState.errors.value ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.value.message}
              </p>
            ) : null}
          </div>

          {error ? (
            <p className="text-sm text-destructive">
              Could not create the transfer.
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
            <Button type="submit" disabled={loading || accounts.length < 2}>
              {loading ? 'Saving...' : 'Save transfer'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default AddTransferDialog;

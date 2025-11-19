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
import type { BankResponse, CreateBankRequest } from '@/types/api';
import useAxios from 'axios-hooks';
import { useFieldArray, useForm } from 'react-hook-form';

interface AddBankDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => Promise<void> | void;
}

type BankFormValues = {
  name: string;
  bankAccounts: { name: string; balance: string }[];
};

const AddBankDialog = ({
  open,
  onOpenChange,
  onSuccess,
}: AddBankDialogProps) => {
  const form = useForm<BankFormValues>({
    defaultValues: {
      name: '',
      bankAccounts: [{ name: '', balance: '' }],
    },
  });

  const { fields, append, remove } = useFieldArray<
    BankFormValues,
    'bankAccounts',
    'id'
  >({
    control: form.control,
    name: 'bankAccounts',
  });
  type BankAccountField = (typeof fields)[number];

  const [{ loading, error }, executeCreateBank] = useAxios<BankResponse>(
    { url: '/api/banks', method: 'POST' },
    { manual: true }
  );

  const handleSubmit = form.handleSubmit(async (values: BankFormValues) => {
    const payload: CreateBankRequest = {
      name: values.name.trim(),
      bankAccounts: values.bankAccounts
        .map((account): { name: string; balance: number } => ({
          name: account.name.trim(),
          balance: Number.parseFloat(account.balance || '0'),
        }))
        .filter(
          (account: { name: string; balance: number }) =>
            account.name.length > 0
        ),
    };

    if (!payload.bankAccounts || payload.bankAccounts.length === 0) {
      payload.bankAccounts = null;
    }

    try {
      await executeCreateBank({ data: payload });
      await onSuccess();
      form.reset({ name: '', bankAccounts: [{ name: '', balance: '' }] });
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
          <DialogTitle>Add a bank</DialogTitle>
          <DialogDescription>
            Track a bank and optionally include its existing accounts.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="bank-name">Bank name</Label>
            <Input
              id="bank-name"
              placeholder="My Bank"
              {...form.register('name', { required: 'Name is required' })}
            />
            {form.formState.errors.name ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.name.message}
              </p>
            ) : null}
          </div>

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <p className="text-sm font-medium">Accounts (optional)</p>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => append({ name: '', balance: '' })}
                disabled={loading}
              >
                Add account
              </Button>
            </div>

            <div className="space-y-4">
              {fields.map((field: BankAccountField, index: number) => (
                <div
                  key={field.id}
                  className="rounded-md border border-border/60 p-3"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex-1 space-y-2">
                      <Label htmlFor={`account-name-${index}`}>
                        Account name
                      </Label>
                      <Input
                        id={`account-name-${index}`}
                        placeholder="Checking"
                        {...form.register(`bankAccounts.${index}.name`, {
                          validate: (value: string) =>
                            value.trim().length > 0 ||
                            form.watch(`bankAccounts.${index}.balance`).trim()
                              .length === 0
                              ? true
                              : 'Name is required when specifying a balance',
                        })}
                      />
                      {form.formState.errors.bankAccounts?.[index]?.name ? (
                        <p className="text-xs text-destructive">
                          {
                            form.formState.errors.bankAccounts[index]?.name
                              ?.message as string
                          }
                        </p>
                      ) : null}
                    </div>
                    <Button
                      type="button"
                      size="icon-sm"
                      variant="ghost"
                      onClick={() => remove(index)}
                      disabled={fields.length === 1 || loading}
                    >
                      X
                    </Button>
                  </div>
                  <div className="mt-3 space-y-2">
                    <Label htmlFor={`account-balance-${index}`}>
                      Starting balance
                    </Label>
                    <Input
                      id={`account-balance-${index}`}
                      type="number"
                      step="0.01"
                      placeholder="0.00"
                      {...form.register(`bankAccounts.${index}.balance`, {
                        validate: (value: string) =>
                          value.trim().length === 0 ||
                          !Number.isNaN(Number.parseFloat(value)) ||
                          'Enter a valid number',
                      })}
                    />
                    {form.formState.errors.bankAccounts?.[index]?.balance ? (
                      <p className="text-xs text-destructive">
                        {
                          form.formState.errors.bankAccounts[index]?.balance
                            ?.message as string
                        }
                      </p>
                    ) : null}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {error ? (
            <p className="text-sm text-destructive">
              Could not create bank. Please try again.
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
              {loading ? 'Saving...' : 'Save bank'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default AddBankDialog;

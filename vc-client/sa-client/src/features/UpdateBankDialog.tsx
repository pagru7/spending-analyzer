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
import type { BankResponse, UpdateBankRequest } from '@/types/api';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';

interface UpdateBankDialogProps {
  bank: BankResponse | null;
  onClose: () => void;
  onSubmit: (payload: UpdateBankRequest) => Promise<void>;
  busy: boolean;
}

const UpdateBankDialog = ({
  bank,
  onClose,
  onSubmit,
  busy,
}: UpdateBankDialogProps) => {
  const form = useForm<UpdateBankRequest>({
    defaultValues: { name: bank?.name ?? '' },
  });

  const open = Boolean(bank);

  useEffect(() => {
    if (bank) {
      form.reset({ name: bank.name });
    } else {
      form.reset({ name: '' });
    }
  }, [bank, form]);

  const submitHandler = form.handleSubmit(async (values: UpdateBankRequest) => {
    if (!bank) return;
    await onSubmit({ name: values.name.trim() });
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(state: boolean) => {
        if (!state && !busy) {
          onClose();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit bank</DialogTitle>
          <DialogDescription>
            Update the name of the bank as it should appear in the app.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={submitHandler}>
          <div className="space-y-2">
            <Label htmlFor="update-bank-name">Bank name</Label>
            <Input
              id="update-bank-name"
              {...form.register('name', { required: 'Name is required' })}
            />
            {form.formState.errors.name ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.name.message}
              </p>
            ) : null}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={busy}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={busy}>
              {busy ? 'Saving...' : 'Save changes'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default UpdateBankDialog;

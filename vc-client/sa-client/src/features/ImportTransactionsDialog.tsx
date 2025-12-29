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
import type { BankAccountResponse } from '@/types/api';
import useAxios from 'axios-hooks';
import { UploadIcon } from 'lucide-react';
import { useRef, useState } from 'react';
import {
  Controller,
  useForm,
  type ControllerRenderProps,
} from 'react-hook-form';

interface ImportTransactionsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accounts: BankAccountResponse[];
  onSuccess: () => Promise<void> | void;
}

type ImportFormValues = {
  accountId: string;
  file: File | null;
};

const ImportTransactionsDialog = ({
  open,
  onOpenChange,
  accounts,
  onSuccess,
}: ImportTransactionsDialogProps) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFileName, setSelectedFileName] = useState<string>('');

  const form = useForm<ImportFormValues>({
    defaultValues: {
      accountId: '',
      file: null,
    },
  });

  const [{ loading, error }, executeImport] = useAxios(
    { url: '/api/transactions/import', method: 'POST' },
    { manual: true }
  );

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      form.setValue('file', file);
      setSelectedFileName(file.name);
    }
  };

  const handleSubmit = form.handleSubmit(async (values: ImportFormValues) => {
    if (!values.file) {
      return;
    }

    const formData = new FormData();
    formData.append('accountId', values.accountId);
    formData.append('file', values.file);

    try {
      await executeImport({
        data: formData,
      });
      await onSuccess();
      form.reset({
        accountId: '',
        file: null,
      });
      setSelectedFileName('');
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
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
            setSelectedFileName('');
            if (fileInputRef.current) {
              fileInputRef.current.value = '';
            }
          }
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Import transactions</DialogTitle>
          <DialogDescription>
            Upload a file to import transactions into a bank account.
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div className="space-y-2">
            <Label htmlFor="import-account">Bank account</Label>
            <Controller<ImportFormValues, 'accountId'>
              control={form.control}
              name="accountId"
              rules={{ required: 'Select an account' }}
              render={({
                field,
              }: {
                field: ControllerRenderProps<ImportFormValues, 'accountId'>;
              }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger id="import-account">
                    <SelectValue placeholder="Choose account" />
                  </SelectTrigger>
                  <SelectContent>
                    {accounts.length === 0 ? (
                      <SelectItem disabled value="none">
                        No active accounts
                      </SelectItem>
                    ) : (
                      accounts.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.bankName} - {account.name}
                        </SelectItem>
                      ))
                    )}
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
            <Label htmlFor="import-file">File</Label>
            <div className="flex items-center gap-2">
              <Input
                id="import-file"
                ref={fileInputRef}
                type="file"
                accept=".csv,.txt"
                onChange={handleFileChange}
                className="hidden"
              />
              <Button
                type="button"
                variant="outline"
                className="w-full"
                onClick={() => fileInputRef.current?.click()}
              >
                <UploadIcon className="mr-2 size-4" />
                {selectedFileName || 'Choose file'}
              </Button>
            </div>
            {form.formState.errors.file ? (
              <p className="text-xs text-destructive">
                {form.formState.errors.file.message}
              </p>
            ) : null}
            {!form.formState.errors.file && selectedFileName ? (
              <p className="text-xs text-muted-foreground">
                Selected: {selectedFileName}
              </p>
            ) : null}
          </div>

          {error ? (
            <div className="rounded-md bg-destructive/10 px-3 py-2">
              <p className="text-sm text-destructive">
                {error.response?.data?.message ||
                  'Failed to import transactions. Please try again.'}
              </p>
            </div>
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
            <Button type="submit" disabled={loading || !form.watch('file')}>
              {loading ? 'Importing...' : 'Import'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default ImportTransactionsDialog;

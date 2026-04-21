import useAxios from 'axios-hooks';
import { useMemo, useState } from 'react';

import {
  type BankAccountResponse,
  type BankResponse,
  type ImportedTransactionResponse,
  type TransactionResponse,
  type UpdateBankRequest,
} from '@/types/api';

export function useAppData() {
  const [bankActionError, setBankActionError] = useState<string | null>(null);
  const [busyBankId, setBusyBankId] = useState<string | null>(null);
  const [busyAccountId, setBusyAccountId] = useState<string | null>(null);

  const [
    { data: banksData, loading: banksLoading, error: banksError },
    refetchBanks,
  ] = useAxios<BankResponse[]>({ url: '/api/banks' });

  const [
    {
      data: transactionsData,
      loading: transactionsLoading,
      error: transactionsError,
    },
    refetchTransactions,
  ] = useAxios<TransactionResponse[]>({ url: '/api/transactions' });

  const [
    {
      data: importedTransactionsData,
      loading: importedTransactionsLoading,
      error: importedTransactionsError,
    },
  ] = useAxios<ImportedTransactionResponse[]>({
    url: '/api/imported-transactions',
  });

  const [, executeUpdateBank] = useAxios<BankResponse>(
    { method: 'PUT' },
    { manual: true },
  );
  const [, executeDeleteBank] = useAxios<void>(
    { method: 'DELETE' },
    { manual: true },
  );
  const [, executeDeleteAccount] = useAxios<void>(
    { method: 'DELETE' },
    { manual: true },
  );

  const accounts: BankAccountResponse[] = useMemo(
    () =>
      (banksData ?? [])
        .flatMap((bank: BankResponse) =>
          bank.accounts.map((account: BankAccountResponse) => ({
            ...account,
            bankId: bank.id,
            bankName: bank.name,
          })),
        )
        .filter((account: BankAccountResponse) => !account.isInactive),
    [banksData],
  );

  const totalBalance = useMemo(
    () =>
      accounts.reduce(
        (sum, account) =>
          sum + (Number.isFinite(account.balance) ? account.balance : 0),
        0,
      ),
    [accounts],
  );

  const refetchAll = async () => {
    await Promise.allSettled([refetchBanks(), refetchTransactions()]);
  };

  const handleBankUpdate = async (
    bankId: string,
    payload: UpdateBankRequest,
  ) => {
    setBankActionError(null);
    setBusyBankId(bankId);
    try {
      await executeUpdateBank({ url: `/api/banks/${bankId}`, data: payload });
      await refetchBanks();
    } catch {
      setBankActionError('Failed to update bank. Please try again.');
    } finally {
      setBusyBankId(null);
    }
  };

  const handleBankInactive = async (bankId: string) => {
    setBankActionError(null);
    setBusyBankId(bankId);
    try {
      await executeDeleteBank({ url: `/api/banks/${bankId}` });
      await refetchBanks();
    } catch {
      setBankActionError('Failed to update bank status. Please try again.');
    } finally {
      setBusyBankId(null);
    }
  };

  const handleDeleteAccount = async (
    account: BankAccountResponse,
    bankId: number,
  ) => {
    setBankActionError(null);
    setBusyAccountId(account.id);
    try {
      await executeDeleteAccount({
        url: `/api/banks/${bankId}/${account.id}`,
      });
      await refetchBanks();
    } catch {
      setBankActionError('Failed to delete account. Please try again.');
    } finally {
      setBusyAccountId(null);
    }
  };

  return {
    banksData,
    banksLoading,
    banksError,
    refetchBanks,
    transactionsData,
    transactionsLoading,
    transactionsError,
    refetchTransactions,
    importedTransactionsData,
    importedTransactionsLoading,
    importedTransactionsError,
    accounts,
    totalBalance,
    refetchAll,
    handleBankUpdate,
    handleBankInactive,
    handleDeleteAccount,
    busyBankId,
    busyAccountId,
    bankActionError,
  };
}

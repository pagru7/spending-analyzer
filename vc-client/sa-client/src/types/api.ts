export interface BankAccountResponse {
  id: string;
  name: string;
  creationDate: string;
  balance: number;
  isInactive: boolean;
  bankId?: string;
  bankName?: string;
}

export interface BankResponse {
  id: string;
  name: string;
  isInactive: boolean;
  bankAccounts: BankAccountResponse[];
}

export interface CreateBankAccountDto {
  name: string;
  balance: number;
}

export interface CreateBankRequest {
  name: string;
  bankAccounts?: CreateBankAccountDto[] | null;
}

export interface UpdateBankRequest {
  name: string;
}

export interface TransactionResponse {
  id: string;
  description: string;
  accountId: string;
  accountName: string;
  recipient: string;
  amount: number;
}

export interface CreateTransactionRequest {
  description: string;
  accountId: string;
  recipient: string;
  amount: number;
}

export type UpdateTransactionRequest = CreateTransactionRequest;

export interface TransferResponse {
  id: string;
  description: string;
  sourceAccountId: string;
  sourceAccountName: string;
  targetAccountId: string;
  targetAccountName: string;
  value: number;
}

export interface CreateTransferRequest {
  description: string;
  sourceAccountId: string;
  targetAccountId: string;
  value: number;
}

export type UpdateTransferRequest = CreateTransferRequest;

export interface BankAccountDetailResponse extends BankAccountResponse {
  bankId: string;
  bankName: string;
}

export interface UpdateBankAccountRequest {
  name: string;
}

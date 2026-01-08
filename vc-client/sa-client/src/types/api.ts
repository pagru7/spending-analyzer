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
  accounts: BankAccountResponse[];
}

export interface CreateBankAccountDto {
  name: string;
  balance: number;
}

export interface CreateBankRequest {
  name: string;
  accounts?: CreateBankAccountDto[] | null;
}

export interface UpdateBankRequest {
  name: string;
}

export interface TransactionResponse {
  id: number;
  description: string;
  accountId: number;
  accountName: string;
  recipient: string;
  amount: number;
  transactionDate: string;
}

export interface CreateTransactionRequest {
  accountId: number;
  amount: number;
  description: string;
  recipient: string;
  transactionDate: string;
  transactionFee?: number;
  isIncome?: boolean;
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

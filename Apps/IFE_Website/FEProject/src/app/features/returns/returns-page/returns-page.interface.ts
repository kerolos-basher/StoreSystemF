import { ReturnReason, SalesInvoiceDetail } from '../../../shared/models/inventory.models';

export interface ReturnLineDraft {
  salesInvoiceItemId: number;
  productName: string;
  availableForReturn: number;
  quantity: number;
  itemReasonType: number;
  notes: string;
}

export interface ReturnsPageState {
  invoice: SalesInvoiceDetail | null;
  returnReasons: ReturnReason[];
  lines: ReturnLineDraft[];
  returnReasonType: number;
  notes: string;
  saving: boolean;
  invoiceNumber: string;
}

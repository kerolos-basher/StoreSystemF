import {
  CreateReturnRequest,
  CustomerAutoComplete,
  ReturnInvoiceListItem,
  ReturnReason,
  SalesInvoiceDetail,
  SalesInvoiceListItem
} from '../../shared/models/inventory.models';

export interface ReturnLineDraft {
  salesInvoiceItemId: number;
  productName: string;
  availableForReturn: number;
  soldUnitPrice: number;
  unitPrice: number;
  quantity: number;
  itemReasonType: number;
  notes: string;
}

export interface SelectedInvoice {
  id: number;
  invoiceNumber: string;
  lines: ReturnLineDraft[];
}

export type {
  CreateReturnRequest,
  CustomerAutoComplete,
  ReturnInvoiceListItem,
  ReturnReason,
  SalesInvoiceDetail,
  SalesInvoiceListItem
};

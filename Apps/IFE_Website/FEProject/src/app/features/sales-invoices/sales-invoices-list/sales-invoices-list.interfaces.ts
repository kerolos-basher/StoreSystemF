import { SalesInvoiceDetail, SalesInvoiceListItem } from '../../../shared/models/inventory.models';

export interface SalesInvoicesSummary {
  totalSales: number;
  invoiceCount: number;
  itemsSold: number;
}

export type { SalesInvoiceDetail, SalesInvoiceListItem };

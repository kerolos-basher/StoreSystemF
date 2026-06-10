import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService, UpdateSalesInvoiceItem } from '../../../core/services/inventory-api.service';
import { PagedResult, SalesInvoiceDetail, SalesInvoiceListItem } from '../../../shared/models/inventory.models';

@Injectable()
export class SalesInvoicesListService {
  private readonly api = inject(InventoryApiService);

  searchInvoices(query: Record<string, string | number | boolean>): Observable<PagedResult<SalesInvoiceListItem>> {
    return this.api.searchSalesInvoices(query);
  }

  getInvoice(invoiceId: number): Observable<SalesInvoiceDetail> {
    return this.api.getSalesInvoice(invoiceId);
  }

  updateInvoice(invoiceId: number, notes: string, isDeferredPayment: boolean, items: UpdateSalesInvoiceItem[]): Observable<void> {
    return this.api.updateSalesInvoice(invoiceId, notes, isDeferredPayment, items);
  }

  deleteInvoice(invoiceId: number): Observable<void> {
    return this.api.deleteSalesInvoice(invoiceId);
  }
}

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../core/services/inventory-api.service';
import {
  CreateReturnRequest,
  CreateReturnResult,
  CustomerAutoComplete,
  PagedResult,
  ReturnInvoiceListItem,
  ReturnReason,
  SalesInvoiceDetail,
  SalesInvoiceListItem
} from '../../shared/models/inventory.models';

@Injectable()
export class ReturnsService {
  private readonly api = inject(InventoryApiService);

  getReturnReasons(): Observable<ReturnReason[]> {
    return this.api.getReturnReasons();
  }

  searchCustomers(term: string): Observable<CustomerAutoComplete[]> {
    return this.api.searchCustomersAutocomplete(term);
  }

  getCustomerInvoices(customerId: number): Observable<SalesInvoiceListItem[]> {
    return this.api.getCustomerInvoices(customerId);
  }

  getInvoiceByNumber(number: string): Observable<SalesInvoiceDetail> {
    return this.api.getSalesInvoiceByNumber(number);
  }

  getSalesInvoice(invoiceId: number): Observable<SalesInvoiceDetail> {
    return this.api.getSalesInvoice(invoiceId);
  }

  createReturn(payload: CreateReturnRequest): Observable<CreateReturnResult> {
    return this.api.createReturn(payload);
  }

  searchReturns(query: Record<string, string | number>): Observable<PagedResult<ReturnInvoiceListItem>> {
    return this.api.searchReturns(query);
  }
}

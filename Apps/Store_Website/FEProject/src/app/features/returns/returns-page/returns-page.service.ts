import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import {
  CreateReturnRequest,
  CreateReturnResult,
  ReturnReason,
  SalesInvoiceDetail
} from '../../../shared/models/inventory.models';

@Injectable()
export class ReturnsPageService {
  private readonly api = inject(InventoryApiService);

  getReturnReasons(): Observable<ReturnReason[]> {
    return this.api.getReturnReasons();
  }

  searchSalesInvoices(invoiceNumber: string): Observable<{ items: { id: string; invoiceNumber: string }[] }> {
    return this.api.searchSalesInvoices({ invoiceNumber, pageNumber: 1, pageSize: 5 });
  }

  getSalesInvoice(invoiceId: number): Observable<SalesInvoiceDetail> {
    return this.api.getSalesInvoice(invoiceId);
  }

  createReturn(payload: CreateReturnRequest): Observable<CreateReturnResult> {
    return this.api.createReturn(payload);
  }
}

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { DeferredPaymentListItem, DeferredPaymentStatement, PagedResult } from '../../../shared/models/inventory.models';

@Injectable()
export class DeferredPaymentsService {
  private readonly api = inject(InventoryApiService);

  searchPayments(query: Record<string, string | number | boolean>): Observable<PagedResult<DeferredPaymentListItem>> {
    return this.api.searchDeferredPayments(query);
  }

  registerPayment(deferredPaymentId: number, amountPaid: number, notes: string): Observable<void> {
    return this.api.registerDeferredPayment(deferredPaymentId, amountPaid, notes);
  }

  updatePaymentTransaction(
    deferredPaymentId: number,
    transactionId: number,
    amountPaid: number,
    notes: string
  ): Observable<void> {
    return this.api.updateDeferredPaymentTransaction(deferredPaymentId, transactionId, amountPaid, notes);
  }

  deletePaymentTransaction(deferredPaymentId: number, transactionId: number): Observable<void> {
    return this.api.deleteDeferredPaymentTransaction(deferredPaymentId, transactionId);
  }

  getStatement(deferredPaymentId: number): Observable<DeferredPaymentStatement> {
    return this.api.getDeferredPaymentStatement(deferredPaymentId);
  }
}

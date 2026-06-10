import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../core/services/inventory-api.service';
import {
  CreateSaleResult,
  CustomerAutoComplete,
  ProductDetailsAutoComplete,
  ProductDetailsSearch,
  SaleLineRequest
} from '../../shared/models/inventory.models';

@Injectable()
export class SalesService {
  private readonly api = inject(InventoryApiService);

  searchByBarcode(barcode: string): Observable<ProductDetailsSearch> {
    return this.api.searchProductDetailsByBarcode(barcode);
  }

  searchAutocomplete(term: string): Observable<ProductDetailsAutoComplete[]> {
    return this.api.searchProductDetailsAutocomplete(term);
  }

  searchCustomers(term: string): Observable<CustomerAutoComplete[]> {
    return this.api.searchCustomersAutocomplete(term);
  }

  createSale(
    items: SaleLineRequest[],
    customerName: string,
    customerPhone: string,
    customerId: number | null,
    notes: string,
    isDeferredPayment: boolean,
    amountPaid: number
  ): Observable<CreateSaleResult> {
    return this.api.createSale(items, customerName, customerPhone, customerId, notes, isDeferredPayment, amountPaid);
  }
}

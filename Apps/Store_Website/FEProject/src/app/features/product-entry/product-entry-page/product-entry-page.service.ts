import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import {
  Category,
  CreatePurchaseEntryRequest,
  CreatePurchaseEntryResult,
  ProductNameLookup,
  Supplier
} from '../../../shared/models/inventory.models';

@Injectable()
export class ProductEntryPageService {
  private readonly api = inject(InventoryApiService);

  getCategories(): Observable<Category[]> {
    return this.api.getCategories();
  }

  searchSuppliers(term?: string): Observable<Supplier[]> {
    return this.api.searchSuppliers(term);
  }

  searchProductNames(term: string): Observable<ProductNameLookup[]> {
    return this.api.searchProductNames(term);
  }

  createSupplier(name: string): Observable<Supplier> {
    return this.api.createSupplier(name);
  }

  createPurchaseEntry(payload: CreatePurchaseEntryRequest): Observable<CreatePurchaseEntryResult> {
    return this.api.createPurchaseEntry(payload);
  }
}

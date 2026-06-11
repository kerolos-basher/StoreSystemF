import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { Category, CreatePurchaseEntryResult, ProductDetailsAutoComplete, Supplier } from '../../../shared/models/inventory.models';

@Injectable()
export class AddProductService {
  private readonly api = inject(InventoryApiService);

  getCategories(): Observable<Category[]> { return this.api.getCategories(); }
  searchSuppliers(term?: string): Observable<Supplier[]> { return this.api.searchSuppliers(term); }
  createSupplier(name: string): Observable<Supplier> { return this.api.createSupplier(name); }
  searchAutocomplete(term: string): Observable<ProductDetailsAutoComplete[]> {
    return this.api.searchProductDetailsAutocomplete(term);
  }

  createPurchase(payload: {
    productName: string; existingProductId?: number | null; categoryId?: string | null;
    purchasePrice: number; sellingPrice: number; quantity: number;
    supplierName?: string | null; purchaseDate?: Date | null; notes?: string | null;
  }): Observable<CreatePurchaseEntryResult> {
    return this.api.createPurchaseEntry(payload);
  }
}

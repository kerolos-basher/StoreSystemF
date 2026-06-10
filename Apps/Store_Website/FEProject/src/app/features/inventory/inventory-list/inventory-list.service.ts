import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import {
  Category,
  PagedResult,
  ProductDetails,
  ProductListItem,
  ProductStatistics,
  Supplier
} from '../../../shared/models/inventory.models';

@Injectable()
export class InventoryListService {
  private readonly api = inject(InventoryApiService);

  getCategories(): Observable<Category[]> {
    return this.api.getCategories();
  }

  searchSuppliers(): Observable<Supplier[]> {
    return this.api.searchSuppliers();
  }

  getStatistics(lowStockThreshold = 10): Observable<ProductStatistics> {
    return this.api.getStatistics(lowStockThreshold);
  }

  searchProducts(query: Record<string, string | number | boolean>): Observable<PagedResult<ProductListItem>> {
    return this.api.searchProducts(query);
  }

  getProductDetails(productId: string): Observable<ProductDetails> {
    return this.api.getProductDetails(productId);
  }
}

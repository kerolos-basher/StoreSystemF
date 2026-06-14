import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { Category, ProductDetails, BarcodeLabelData, Supplier } from '../../../shared/models/inventory.models';

@Injectable()
export class ProductDetailsService {
  private readonly api = inject(InventoryApiService);

  getCategories(): Observable<Category[]> {
    return this.api.getCategories();
  }

  searchSuppliers(): Observable<Supplier[]> {
    return this.api.searchSuppliers();
  }

  reloadDetails(productId: string): Observable<ProductDetails> {
    return this.api.getProductDetails(productId);
  }

  updateProduct(productId: string, productName: string): Observable<void> {
    return this.api.updateProduct(productId, productName);
  }

  deleteProduct(productId: string): Observable<void> {
    return this.api.deleteProduct(productId);
  }

  updateProductDetails(
    productId: string,
    detailsId: string,
    payload: {
      supplierId?: string | null;
      categoryId?: string | null;
      purchasePrice: number;
      sellingPrice: number;
      quantity?: number;
      notes?: string;
    }
  ): Observable<void> {
    return this.api.updateProductDetails(productId, detailsId, {
      supplierId: payload.supplierId,
      categoryId: payload.categoryId,
      purchasePrice: payload.purchasePrice,
      sellingPrice: payload.sellingPrice,
      quantity: payload.quantity ?? 0,
      notes: payload.notes
    });
  }

  deleteProductDetails(productId: string, detailsId: string, forceDelete = false): Observable<void> {
    return this.api.deleteProductDetails(productId, detailsId, forceDelete);
  }

  getBarcodeLabel(productDetailsId: string): Observable<BarcodeLabelData> {
    return this.api.getBarcodeLabel(productDetailsId);
  }
}

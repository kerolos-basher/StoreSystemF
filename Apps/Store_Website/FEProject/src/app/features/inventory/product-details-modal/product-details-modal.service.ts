import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { ProductDetails, QRCodeData } from '../../../shared/models/inventory.models';

@Injectable()
export class ProductDetailsModalService {
  private readonly api = inject(InventoryApiService);

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
    payload: { purchasePrice: number; sellingPrice: number; quantity?: number; notes?: string }
  ): Observable<void> {
    return this.api.updateProductDetails(productId, detailsId, {
      purchasePrice: payload.purchasePrice,
      sellingPrice: payload.sellingPrice,
      quantity: payload.quantity ?? 0,
      notes: payload.notes
    });
  }

  deleteProductDetails(productId: string, detailsId: string, forceDelete = false): Observable<void> {
    return this.api.deleteProductDetails(productId, detailsId, forceDelete);
  }

  getQRCode(productDetailsId: string): Observable<QRCodeData> {
    return this.api.getQRCode(productDetailsId);
  }
}

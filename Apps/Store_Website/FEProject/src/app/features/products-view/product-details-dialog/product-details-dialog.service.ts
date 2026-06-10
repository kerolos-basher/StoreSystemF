import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { QRCodeData } from '../../../shared/models/inventory.models';

@Injectable()
export class ProductDetailsDialogService {
  private readonly api = inject(InventoryApiService);

  updateProduct(productId: string, productName: string): Observable<void> {
    return this.api.updateProduct(productId, productName);
  }

  deleteProduct(productId: string): Observable<void> {
    return this.api.deleteProduct(productId);
  }

  updateProductDetails(
    productId: string,
    detailsId: string,
    payload: { supplierId?: string | null; categoryId?: string | null; purchasePrice: number; sellingPrice: number; notes?: string }
  ): Observable<void> {
    return this.api.updateProductDetails(productId, detailsId, payload);
  }

  deleteProductDetails(productId: string, detailsId: string): Observable<void> {
    return this.api.deleteProductDetails(productId, detailsId);
  }

  getQRCode(productDetailsId: string): Observable<QRCodeData> {
    return this.api.getQRCode(productDetailsId);
  }
}

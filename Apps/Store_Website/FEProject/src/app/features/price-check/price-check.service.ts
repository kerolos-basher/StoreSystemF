import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../core/services/inventory-api.service';
import { ProductDetailsAutoComplete, ProductDetailsSearch } from '../../shared/models/inventory.models';

@Injectable()
export class PriceCheckService {
  private readonly api = inject(InventoryApiService);

  searchByBarcode(barcode: string): Observable<ProductDetailsSearch> {
    return this.api.searchProductDetailsByBarcode(barcode);
  }

  searchAutocomplete(term: string): Observable<ProductDetailsAutoComplete[]> {
    return this.api.searchProductDetailsAutocomplete(term);
  }
}

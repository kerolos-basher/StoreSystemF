import { Injectable, signal } from '@angular/core';
import { Category, ProductListItem, ProductStatistics, Supplier } from '../../shared/models/inventory.models';

@Injectable({ providedIn: 'root' })
export class InventoryStore {
  readonly categories = signal<Category[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly products = signal<ProductListItem[]>([]);
  readonly totalCount = signal(0);
  readonly statistics = signal<ProductStatistics>({
    totalProducts: 0,
    totalQuantity: 0,
    lowStockCount: 0,
    inventoryValue: 0
  });
  readonly loading = signal(false);
}

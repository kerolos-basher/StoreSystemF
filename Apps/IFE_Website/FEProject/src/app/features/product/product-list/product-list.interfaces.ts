import { ProductListItem, ProductStatistics } from '../../../shared/models/inventory.models';

export interface ProductListFilters {
  productName: string;
  barcode: string;
  supplierId: string;
  categoryId: string;
  purchasePriceFrom: string;
  purchasePriceTo: string;
  quantityFrom: string;
  quantityTo: string;
}

export interface ProductListStatistics {
  totalProducts: number;
  totalQuantity: number;
  inventoryValue: number;
}

export type { ProductListItem, ProductStatistics };

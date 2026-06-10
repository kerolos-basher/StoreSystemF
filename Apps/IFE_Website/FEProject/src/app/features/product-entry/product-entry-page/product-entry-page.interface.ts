import { Category, ProductNameLookup, Supplier } from '../../../shared/models/inventory.models';

export interface ProductEntryFormValue {
  productName: string;
  existingProductId: number | null;
  categoryId: string;
  purchasePrice: number | null;
  sellingPrice: number | null;
  quantity: number;
  supplierId: string;
  supplierName: string;
  purchaseDate: Date;
  notes: string;
}

export interface ProductEntryPageState {
  categories: Category[];
  suppliers: Supplier[];
  selectedProduct: ProductNameLookup | null;
  saving: boolean;
}

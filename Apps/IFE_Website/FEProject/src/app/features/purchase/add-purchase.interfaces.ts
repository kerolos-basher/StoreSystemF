export interface ProductAutoComplete { id: number; productName: string; }

export interface CreatePurchaseCommand {
  productName: string;
  productId?: number;
  categoryId?: number;
  supplierId?: number;
  price: number;
  suggestedSellingPrice: number;
  quantity: number;
  purchaseDate: string;
  barCode?: string;
  notes?: string;
}

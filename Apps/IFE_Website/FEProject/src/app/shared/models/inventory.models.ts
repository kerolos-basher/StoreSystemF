export interface Category {
  id: string;
  name: string;
}

export interface Supplier {
  id: string;
  name: string;
}

export interface ProductDetailLine {
  id: string;
  supplier: string;
  category: string;
  purchasePrice: number;
  sellingPrice: number;
  quantity: number;
  remainingQuantity: number;
  purchaseDate: string;
  notes?: string;
}

export interface ProductDetails {
  id: string;
  productName: string;
  barcode: string;
  totalQuantity: number;
  inventoryValue: number;
  purchaseLineCount: number;
  supplierCount: number;
  lines: ProductDetailLine[];
}

export interface ProductListItem {
  id: string;
  productName: string;
  barcode: string;
  currentQuantity: number;
  latestPurchasePrice: number;
  sellingPrice: number;
  supplier?: string;
  category: string;
  lastPurchaseDate?: string;
  purchaseLineCount: number;
  supplierCount: number;
}

export interface ProductByBarcode {
  id: number;
  productName: string;
  barcode: string;
  sellingPrice: number;
  availableQuantity: number;
  imageUrl: string;
}

export interface QRCodeData {
  productId: string;
  barcode: string;
  base64Image: string;
  contentType: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface ProductStatistics {
  totalProducts: number;
  totalQuantity: number;
  lowStockCount: number;
  inventoryValue: number;
}

export interface SaleLineRequest {
  productId: number;
  quantity: number;
  notes: string;
}

export interface CreateSaleResult {
  invoiceId: number;
  invoiceNumber: string;
  grandTotal: number;
}

export interface SalesInvoiceItem {
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  notes: string;
}

export interface SalesInvoiceListItem {
  id: string;
  invoiceNumber: string;
  saleDate: string;
  subtotal: number;
  discount: number;
  tax: number;
  grandTotal: number;
  itemCount: number;
  items: SalesInvoiceItem[];
}

export interface CartItem {
  productId: number;
  productName: string;
  barcode: string;
  unitPrice: number;
  quantity: number;
  notes: string;
  maxQuantity: number;
}

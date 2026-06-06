export interface Category {
  id: string;
  name: string;
}

export interface Supplier {
  id: string;
  name: string;
}

export interface ReturnReason {
  id: number;
  name: string;
  isReturnToStock: boolean;
}

export interface ProductNameLookup {
  id: number;
  productName: string;
}

export interface ProductDetailLine {
  id: string;
  barcode: string;
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
  productDetailsId: number;
  productName: string;
  barcode: string;
  sellingPrice: number;
  availableQuantity: number;
  imageUrl: string;
}

export interface QRCodeData {
  productId: string;
  productDetailsId: string;
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

export interface CreatePurchaseEntryRequest {
  productName: string;
  existingProductId?: number | null;
  categoryId?: string | null;
  purchasePrice: number;
  sellingPrice: number;
  quantity: number;
  supplierName?: string | null;
  purchaseDate?: Date | null;
  notes?: string | null;
}

export interface CreatePurchaseEntryResult {
  productId: number;
  productDetailsId: number;
  barcode: string;
}

export interface SaleLineRequest {
  productId: number;
  productDetailsId?: number | null;
  quantity: number;
  notes: string;
}

export interface CreateSaleResult {
  invoiceId: number;
  invoiceNumber: string;
  grandTotal: number;
}

export interface SalesInvoiceItem {
  id?: number;
  productId?: number;
  productDetailsId?: number;
  productName: string;
  quantity: number;
  returnedQuantity?: number;
  availableForReturn?: number;
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

export interface SalesInvoiceDetail {
  id: number;
  invoiceNumber: string;
  saleDate: string;
  customerId?: number | null;
  subtotal: number;
  discount: number;
  tax: number;
  grandTotal: number;
  notes: string;
  items: SalesInvoiceItem[];
}

export interface CartItem {
  productId: number;
  productDetailsId: number;
  productName: string;
  barcode: string;
  unitPrice: number;
  quantity: number;
  notes: string;
  maxQuantity: number;
}

export interface ReturnLineRequest {
  salesInvoiceItemId: number;
  quantity: number;
  itemReasonType: number;
  notes: string;
}

export interface CreateReturnRequest {
  salesInvoiceId: number;
  returnReasonType: number;
  notes: string;
  items: ReturnLineRequest[];
}

export interface CreateReturnResult {
  returnInvoiceId: number;
  returnNumber: string;
  totalAmount: number;
}

export interface ReturnInvoiceListItem {
  id: number;
  returnNumber: string;
  salesInvoiceId: number;
  salesInvoiceNumber: string;
  returnDate: string;
  totalAmount: number;
  itemCount: number;
}

export interface ReturnInvoiceItem {
  id: number;
  salesInvoiceItemId: number;
  productId: number;
  productDetailsId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  itemReasonType: number;
  isReturnToStock: boolean;
  notes: string;
}

export interface ReturnInvoiceDetail {
  id: number;
  returnNumber: string;
  salesInvoiceId: number;
  salesInvoiceNumber: string;
  returnDate: string;
  totalAmount: number;
  returnReasonType: number;
  notes: string;
  items: ReturnInvoiceItem[];
}

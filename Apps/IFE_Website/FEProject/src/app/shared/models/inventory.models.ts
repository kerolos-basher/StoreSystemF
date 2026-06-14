export interface Category { id: string; name: string; }
export interface Supplier { id: string; name: string; }
export interface ReturnReason { id: number; name: string; isReturnToStock: boolean; }

export interface ProductAutoComplete { id: number; productName: string; }
export interface ProductDetailsAutoComplete {
  productDetailsId: number;
  productName: string;
  supplierName: string;
  suggestedSellingPrice: number;
  purchasePrice: number;
  remainingQuantity: number;
  productId: number;
  barcode: string;
}

export interface CustomerAutoComplete { id: number; name: string; phone: string; }

export interface ProductDetailsSearch {
  productDetailsId: number;
  productId: number;
  productName: string;
  barcode: string;
  purchasePrice: number;
  suggestedSellingPrice: number;
  supplierName: string;
  categoryName: string;
  remainingQuantity: number;
  notes: string;
}

export interface ProductDetailLine {
  id: string;
  barcode: string;
  supplierId?: string | null;
  supplier: string;
  categoryId?: string | null;
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

export interface BarcodeLabelData {
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
  productDetailsId: number;
  quantity: number;
  unitPrice: number;
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
  stockAvailable?: number;
  purchasePrice?: number;
  unitPrice: number;
  lineTotal: number;
  notes: string;
}

export interface SalesInvoiceListItem {
  id: string;
  invoiceNumber: string;
  saleDate: string;
  customerId?: number | null;
  customerName?: string | null;
  customerPhone?: string | null;
  subtotal: number;
  grandTotal: number;
  isDeferredPayment: boolean;
  itemCount: number;
  items: SalesInvoiceItem[];
}

export interface SalesInvoiceDetail {
  id: number;
  invoiceNumber: string;
  saleDate: string;
  customerId?: number | null;
  customerName?: string | null;
  customerPhone?: string | null;
  subtotal: number;
  grandTotal: number;
  notes: string;
  isDeferredPayment: boolean;
  items: SalesInvoiceItem[];
}

export interface CartItem {
  productId: number;
  productDetailsId: number;
  productName: string;
  supplierName: string;
  barcode: string;
  unitPrice: number;
  suggestedPrice: number;
  quantity: number;
  notes: string;
  maxQuantity: number;
}

export interface ReturnLineRequest {
  salesInvoiceItemId: number;
  quantity: number;
  unitPrice: number;
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

export interface DeferredPaymentStatement {
  id: number;
  salesInvoiceId: number;
  invoiceNumber: string;
  invoiceDate: string;
  customerName: string;
  customerPhone: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  isFullyPaid: boolean;
  transactions: DeferredPaymentTransaction[];
}

export interface DeferredPaymentTransaction {
  id: number;
  amountPaid: number;
  paymentDate: string;
  notes: string;
}

export interface DeferredPaymentListItem {
  id: number;
  salesInvoiceId: number;
  invoiceNumber: string;
  invoiceDate: string;
  customerId: number;
  customerName: string;
  customerPhone: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  isFullyPaid: boolean;
}

export interface FinancialReport {
  totalSales: number;
  totalPurchaseCost: number;
  netProfit: number;
  cashSales: number;
  deferredSales: number;
  outstandingDebts: number;
  inventoryValue: number;
  topProducts: { productName: string; quantitySold: number; revenue: number }[];
  topCustomers: { customerName: string; invoiceCount: number; totalSpent: number }[];
}

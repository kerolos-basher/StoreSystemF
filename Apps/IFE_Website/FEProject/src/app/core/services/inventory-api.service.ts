import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import {
  Category, CreatePurchaseEntryRequest, CreatePurchaseEntryResult, CreateReturnRequest,
  CreateReturnResult, CreateSaleResult, CustomerAutoComplete, DeferredPaymentListItem, DeferredPaymentStatement,
  FinancialReport, PagedResult, ProductAutoComplete, ProductDetails, ProductDetailsAutoComplete,
  ProductDetailsSearch, ProductListItem, ProductStatistics, QRCodeData, ReturnInvoiceDetail,
  ReturnInvoiceListItem, ReturnReason, SaleLineRequest, SalesInvoiceDetail, SalesInvoiceListItem,
  Supplier
} from '../../shared/models/inventory.models';
import { environment } from '../../../environments/environment';

export interface UpdateSalesInvoiceItem {
  id?: number | null;
  productDetailsId: number;
  quantity: number;
  unitPrice: number;
  notes: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/categories`);
  }

  createCategory(name: string): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/categories`, { name });
  }

  updateCategory(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/categories/${id}`, { name });
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/categories/${id}`);
  }

  searchSuppliers(term?: string): Observable<Supplier[]> {
    const params = term ? new HttpParams().set('term', term) : undefined;
    return this.http.get<Supplier[]>(`${this.baseUrl}/suppliers`, { params });
  }

  createSupplier(name: string): Observable<Supplier> {
    return this.http.post<Supplier>(`${this.baseUrl}/suppliers`, { name });
  }

  updateSupplier(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/suppliers/${id}`, { name });
  }

  deleteSupplier(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/suppliers/${id}`);
  }

  getReturnReasons(): Observable<ReturnReason[]> {
    return this.http.get<ReturnReason[]>(`${this.baseUrl}/return-reasons`);
  }

  createReturnReason(name: string, isReturnToStock: boolean): Observable<ReturnReason> {
    return this.http.post<ReturnReason>(`${this.baseUrl}/return-reasons`, { name, isReturnToStock });
  }

  updateReturnReason(id: number, name: string, isReturnToStock: boolean): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/return-reasons/${id}`, { name, isReturnToStock });
  }

  deleteReturnReason(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/return-reasons/${id}`);
  }

  searchProductsAutocomplete(term: string): Observable<ProductAutoComplete[]> {
    if (term.length < 2) return of([]);
    return this.http.get<ProductAutoComplete[]>(`${this.baseUrl}/products/autocomplete`, {
      params: new HttpParams().set('q', term)
    });
  }

  searchProductDetailsAutocomplete(term: string): Observable<ProductDetailsAutoComplete[]> {
    if (term.length < 2) return of([]);
    return this.http.get<ProductDetailsAutoComplete[]>(`${this.baseUrl}/product-details/autocomplete`, {
      params: new HttpParams().set('q', term)
    });
  }

  searchCustomersAutocomplete(term: string): Observable<CustomerAutoComplete[]> {
    if (term.length < 2) return of([]);
    return this.http.get<CustomerAutoComplete[]>(`${this.baseUrl}/customers/autocomplete`, {
      params: new HttpParams().set('q', term)
    });
  }

  getCustomerInvoices(customerId: number): Observable<SalesInvoiceListItem[]> {
    return this.http.get<SalesInvoiceListItem[]>(`${this.baseUrl}/customers/${customerId}/invoices`);
  }

  createPurchaseEntry(payload: CreatePurchaseEntryRequest): Observable<CreatePurchaseEntryResult> {
    return this.http.post<CreatePurchaseEntryResult>(`${this.baseUrl}/products/purchase-entry`, payload);
  }

  searchProducts(query: Record<string, string | number | boolean>): Observable<PagedResult<ProductListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== null && value !== undefined && `${value}` !== '') {
        params = params.set(key, `${value}`);
      }
    }
    return this.http.get<PagedResult<ProductListItem>>(`${this.baseUrl}/products`, { params });
  }

  getProductDetails(productId: string): Observable<ProductDetails> {
    return this.http.get<ProductDetails>(`${this.baseUrl}/products/${productId}/details`);
  }

  updateProduct(productId: string, productName: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/products/${productId}`, { productName });
  }

  deleteProduct(productId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${productId}`);
  }

  updateProductDetails(productId: string, detailsId: string, payload: {
    supplierId?: string | null; categoryId?: string | null;
    purchasePrice: number; sellingPrice: number; quantity: number; notes?: string;
  }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/products/${productId}/details/${detailsId}`, payload);
  }

  deleteProductDetails(productId: string, detailsId: string, forceDelete = false): Observable<void> {
    const params = forceDelete ? new HttpParams().set('forceDelete', 'true') : undefined;
    return this.http.delete<void>(`${this.baseUrl}/products/${productId}/details/${detailsId}`, { params });
  }

  searchProductDetailsByBarcode(barcode: string): Observable<ProductDetailsSearch> {
    return this.http.get<ProductDetailsSearch>(`${this.baseUrl}/product-details/search-by-barcode/${encodeURIComponent(barcode)}`);
  }

  getQRCode(productDetailsId: string): Observable<QRCodeData> {
    return this.http.get<QRCodeData>(`${this.baseUrl}/products/details/${productDetailsId}/qrcode`);
  }

  getStatistics(lowStockThreshold = 10): Observable<ProductStatistics> {
    return this.http.get<ProductStatistics>(`${this.baseUrl}/products/statistics`, {
      params: new HttpParams().set('lowStockThreshold', lowStockThreshold)
    });
  }

  createSale(
    items: SaleLineRequest[],
    customerName: string,
    customerPhone: string,
    customerId: number | null,
    notes: string,
    isDeferredPayment: boolean
  ): Observable<CreateSaleResult> {
    return this.http.post<CreateSaleResult>(`${this.baseUrl}/sales`, {
      items, customerName, customerPhone, customerId, notes, isDeferredPayment
    });
  }

  searchSalesInvoices(query: Record<string, string | number | boolean>): Observable<PagedResult<SalesInvoiceListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== null && value !== undefined && `${value}` !== '') {
        params = params.set(key, `${value}`);
      }
    }
    return this.http.get<PagedResult<SalesInvoiceListItem>>(`${this.baseUrl}/sales/invoices`, { params });
  }

  getSalesInvoiceByNumber(number: string): Observable<SalesInvoiceDetail> {
    return this.http.get<SalesInvoiceDetail>(`${this.baseUrl}/sales/invoices/by-number/${encodeURIComponent(number)}`);
  }

  getSalesInvoice(invoiceId: number): Observable<SalesInvoiceDetail> {
    return this.http.get<SalesInvoiceDetail>(`${this.baseUrl}/sales/${invoiceId}`);
  }

  updateSalesInvoice(invoiceId: number, notes: string, isDeferredPayment: boolean, items: UpdateSalesInvoiceItem[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/sales/${invoiceId}`, { notes, isDeferredPayment, items });
  }

  deleteSalesInvoice(invoiceId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/sales/${invoiceId}`);
  }

  searchDeferredPayments(query: Record<string, string | number | boolean>): Observable<PagedResult<DeferredPaymentListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value === null || value === undefined || `${value}` === '') continue;
      const paramKey = key === 'customerName' ? 'customerTerm' : key;
      params = params.set(paramKey, `${value}`);
    }
    return this.http.get<PagedResult<DeferredPaymentListItem>>(`${this.baseUrl}/deferred-payments`, { params });
  }

  getDeferredPaymentStatement(id: number): Observable<DeferredPaymentStatement> {
    return this.http.get<DeferredPaymentStatement>(`${this.baseUrl}/deferred-payments/${id}/statement`);
  }

  registerDeferredPayment(deferredPaymentId: number, amountPaid: number, notes: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/deferred-payments/${deferredPaymentId}/payments`, { amountPaid, notes });
  }

  getFinancialReport(from?: string, to?: string): Observable<FinancialReport> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<FinancialReport>(`${this.baseUrl}/reports/financial`, { params });
  }

  createReturn(payload: CreateReturnRequest): Observable<CreateReturnResult> {
    return this.http.post<CreateReturnResult>(`${this.baseUrl}/returns`, payload);
  }

  searchReturns(query: Record<string, string | number>): Observable<PagedResult<ReturnInvoiceListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== null && value !== undefined && `${value}` !== '') {
        params = params.set(key, `${value}`);
      }
    }
    return this.http.get<PagedResult<ReturnInvoiceListItem>>(`${this.baseUrl}/returns`, { params });
  }

  getReturn(returnId: number): Observable<ReturnInvoiceDetail> {
    return this.http.get<ReturnInvoiceDetail>(`${this.baseUrl}/returns/${returnId}`);
  }
}

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  Category,
  CreatePurchaseEntryRequest,
  CreatePurchaseEntryResult,
  CreateReturnRequest,
  CreateReturnResult,
  CreateSaleResult,
  PagedResult,
  ProductByBarcode,
  ProductDetails,
  ProductListItem,
  ProductNameLookup,
  ProductStatistics,
  QRCodeData,
  ReturnInvoiceDetail,
  ReturnInvoiceListItem,
  ReturnReason,
  SaleLineRequest,
  SalesInvoiceDetail,
  SalesInvoiceListItem,
  Supplier
} from '../../shared/models/inventory.models';
import { environment } from '../../../environments/environment';

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

  searchProductNames(term: string, limit = 10): Observable<ProductNameLookup[]> {
    const params = new HttpParams().set('term', term).set('limit', limit);
    return this.http.get<ProductNameLookup[]>(`${this.baseUrl}/products/search-names`, { params });
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

  updateProductDetails(
    productId: string,
    detailsId: string,
    payload: { supplierId?: string | null; categoryId?: string | null; purchasePrice: number; sellingPrice: number; notes?: string }
  ): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/products/${productId}/details/${detailsId}`, payload);
  }

  deleteProductDetails(productId: string, detailsId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${productId}/details/${detailsId}`);
  }

  getProductByBarcode(barcode: string): Observable<ProductByBarcode> {
    return this.http.get<ProductByBarcode>(`${this.baseUrl}/products/by-barcode/${encodeURIComponent(barcode)}`).pipe(
      map(product => ({
        ...product,
        id: Number(product.id),
        productDetailsId: Number(product.productDetailsId)
      }))
    );
  }

  getQRCode(productDetailsId: string): Observable<QRCodeData> {
    return this.http.get<QRCodeData>(`${this.baseUrl}/products/details/${productDetailsId}/qrcode`);
  }

  getStatistics(lowStockThreshold = 10): Observable<ProductStatistics> {
    const params = new HttpParams().set('lowStockThreshold', lowStockThreshold);
    return this.http.get<ProductStatistics>(`${this.baseUrl}/products/statistics`, { params });
  }

  createSale(items: SaleLineRequest[], discount: number, tax: number, notes: string, customerId?: number | null): Observable<CreateSaleResult> {
    const payload = {
      items: items.map(x => ({
        productId: Number(x.productId),
        productDetailsId: x.productDetailsId ? Number(x.productDetailsId) : null,
        quantity: Number(x.quantity),
        notes: x.notes ?? ''
      })),
      discount: Number(discount),
      tax: Number(tax),
      notes: notes ?? '',
      customerId: customerId ?? null
    };
    return this.http.post<CreateSaleResult>(`${this.baseUrl}/sales`, payload);
  }

  searchSalesInvoices(query: Record<string, string | number>): Observable<PagedResult<SalesInvoiceListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== null && value !== undefined && `${value}` !== '') {
        params = params.set(key, `${value}`);
      }
    }
    return this.http.get<PagedResult<SalesInvoiceListItem>>(`${this.baseUrl}/sales/invoices`, { params });
  }

  getSalesInvoice(invoiceId: number): Observable<SalesInvoiceDetail> {
    return this.http.get<SalesInvoiceDetail>(`${this.baseUrl}/sales/${invoiceId}`);
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

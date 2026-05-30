import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {  Category,
  CreateSaleResult,
  PagedResult,
  ProductByBarcode,
  ProductDetails,
  ProductListItem,
  ProductStatistics,
  QRCodeData,
  SaleLineRequest,
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

  searchSuppliers(term?: string): Observable<Supplier[]> {
    const params = term ? new HttpParams().set('term', term) : undefined;
    return this.http.get<Supplier[]>(`${this.baseUrl}/suppliers`, { params });
  }

  createSupplier(name: string): Observable<Supplier> {
    return this.http.post<Supplier>(`${this.baseUrl}/suppliers`, { name });
  }

  createPurchaseEntry(payload: unknown): Observable<{ productId: string }> {
    return this.http.post<{ productId: string }>(`${this.baseUrl}/products/purchase-entry`, payload);
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

  getProductByBarcode(barcode: string): Observable<ProductByBarcode> {
    return this.http.get<ProductByBarcode>(`${this.baseUrl}/products/by-barcode/${encodeURIComponent(barcode)}`).pipe(
      map(product => ({ ...product, id: Number(product.id) }))
    );
  }

  getQRCode(productId: string): Observable<QRCodeData> {
    return this.http.get<QRCodeData>(`${this.baseUrl}/products/${productId}/qrcode`);
  }

  getStatistics(lowStockThreshold = 10): Observable<ProductStatistics> {
    const params = new HttpParams().set('lowStockThreshold', lowStockThreshold);
    return this.http.get<ProductStatistics>(`${this.baseUrl}/products/statistics`, { params });
  }

  createSale(items: SaleLineRequest[], discount: number, tax: number, notes: string): Observable<CreateSaleResult> {
    const payload = {
      items: items.map(x => ({
        productId: Number(x.productId),
        quantity: Number(x.quantity),
        notes: x.notes ?? ''
      })),
      discount: Number(discount),
      tax: Number(tax),
      notes: notes ?? ''
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
}

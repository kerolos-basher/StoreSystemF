import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { Category, ReturnReason, Supplier } from '../../../shared/models/inventory.models';

@Injectable()
export class LookupsPageService {
  private readonly api = inject(InventoryApiService);

  getCategories(): Observable<Category[]> {
    return this.api.getCategories();
  }

  createCategory(name: string): Observable<Category> {
    return this.api.createCategory(name);
  }

  updateCategory(id: string, name: string): Observable<void> {
    return this.api.updateCategory(id, name);
  }

  deleteCategory(id: string): Observable<void> {
    return this.api.deleteCategory(id);
  }

  searchSuppliers(): Observable<Supplier[]> {
    return this.api.searchSuppliers();
  }

  createSupplier(name: string): Observable<Supplier> {
    return this.api.createSupplier(name);
  }

  updateSupplier(id: string, name: string): Observable<void> {
    return this.api.updateSupplier(id, name);
  }

  deleteSupplier(id: string): Observable<void> {
    return this.api.deleteSupplier(id);
  }

  getReturnReasons(): Observable<ReturnReason[]> {
    return this.api.getReturnReasons();
  }

  createReturnReason(name: string, isReturnToStock: boolean): Observable<ReturnReason> {
    return this.api.createReturnReason(name, isReturnToStock);
  }

  updateReturnReason(id: number, name: string, isReturnToStock: boolean): Observable<void> {
    return this.api.updateReturnReason(id, name, isReturnToStock);
  }

  deleteReturnReason(id: number): Observable<void> {
    return this.api.deleteReturnReason(id);
  }
}

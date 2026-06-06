import { Injectable, computed, signal } from '@angular/core';
import { CartItem } from '../../shared/models/inventory.models';

@Injectable({ providedIn: 'root' })
export class SalesStore {
  readonly items = signal<CartItem[]>([]);
  readonly discount = signal(0);
  readonly tax = signal(0);
  readonly notes = signal('');
  readonly processing = signal(false);

  readonly subtotal = computed(() =>
    this.items().reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
  );

  readonly grandTotal = computed(() =>
    Math.max(0, this.subtotal() - this.discount() + this.tax())
  );

  readonly itemCount = computed(() =>
    this.items().reduce((sum, item) => sum + item.quantity, 0)
  );

  addOrIncrement(product: {
    id: number | string;
    productDetailsId: number | string;
    productName: string;
    barcode: string;
    sellingPrice: number;
    availableQuantity: number;
  }): void {
    const productId = Number(product.id);
    const productDetailsId = Number(product.productDetailsId);
    const existing = this.items().find(x => x.productDetailsId === productDetailsId);

    if (existing) {
      if (existing.quantity >= existing.maxQuantity) return;
      this.items.update(list =>
        list.map(x => x.productDetailsId === productDetailsId ? { ...x, quantity: x.quantity + 1 } : x)
      );
      return;
    }

    this.items.update(list => [...list, {
      productId,
      productDetailsId,
      productName: product.productName,
      barcode: product.barcode,
      unitPrice: product.sellingPrice,
      quantity: 1,
      notes: '',
      maxQuantity: product.availableQuantity
    }]);
  }

  updateQuantity(productDetailsId: number, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(productDetailsId);
      return;
    }
    this.items.update(list =>
      list.map(x => x.productDetailsId === productDetailsId
        ? { ...x, quantity: Math.min(quantity, x.maxQuantity) }
        : x)
    );
  }

  updateNotes(productDetailsId: number, notes: string): void {
    this.items.update(list =>
      list.map(x => x.productDetailsId === productDetailsId ? { ...x, notes } : x)
    );
  }

  removeItem(productDetailsId: number): void {
    this.items.update(list => list.filter(x => x.productDetailsId !== productDetailsId));
  }

  clear(): void {
    this.items.set([]);
    this.discount.set(0);
    this.tax.set(0);
    this.notes.set('');
  }
}

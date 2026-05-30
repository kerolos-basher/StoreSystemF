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

  addOrIncrement(product: { id: number | string; productName: string; barcode: string; sellingPrice: number; availableQuantity: number }): void {
    const productId = Number(product.id);
    const existing = this.items().find(x => x.productId === productId);
    if (existing) {
      if (existing.quantity >= existing.maxQuantity) return;
      this.items.update(list =>
        list.map(x => x.productId === productId ? { ...x, quantity: x.quantity + 1 } : x)
      );
      return;
    }

    this.items.update(list => [...list, {
      productId,
      productName: product.productName,
      barcode: product.barcode,
      unitPrice: product.sellingPrice,
      quantity: 1,
      notes: '',
      maxQuantity: product.availableQuantity
    }]);
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(productId);
      return;
    }
    this.items.update(list =>
      list.map(x => x.productId === productId
        ? { ...x, quantity: Math.min(quantity, x.maxQuantity) }
        : x)
    );
  }

  updateNotes(productId: number, notes: string): void {
    this.items.update(list =>
      list.map(x => x.productId === productId ? { ...x, notes } : x)
    );
  }

  removeItem(productId: number): void {
    this.items.update(list => list.filter(x => x.productId !== productId));
  }

  clear(): void {
    this.items.set([]);
    this.discount.set(0);
    this.tax.set(0);
    this.notes.set('');
  }
}

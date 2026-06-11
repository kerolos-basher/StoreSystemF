import { Injectable, computed, signal } from '@angular/core';
import { CartItem } from '../../shared/models/inventory.models';

@Injectable({ providedIn: 'root' })
export class SalesStore {
  readonly items = signal<CartItem[]>([]);
  readonly notes = signal('');
  readonly processing = signal(false);
  readonly customerName = signal('');
  readonly customerPhone = signal('');
  readonly customerId = signal<number | null>(null);
  readonly isDeferredPayment = signal(false);
  readonly amountPaid = signal(0);

  readonly subtotal = computed(() =>
    this.items().reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
  );

  readonly grandTotal = computed(() => this.subtotal());

  readonly remainingAmount = computed(() =>
    Math.max(0, this.grandTotal() - this.amountPaid())
  );

  readonly itemCount = computed(() =>
    this.items().reduce((sum, item) => sum + item.quantity, 0)
  );

  addOrIncrement(product: {
    productId: number | string;
    productDetailsId: number | string;
    productName: string;
    supplierName: string;
    barcode: string;
    sellingPrice: number;
    availableQuantity: number;
  }): boolean {
    const productId = Number(product.productId);
    const productDetailsId = Number(product.productDetailsId);
    const existing = this.items().find(x => x.productDetailsId === productDetailsId);

    if (existing) {
      if (existing.quantity >= existing.maxQuantity) return false;
      this.items.update(list =>
        list.map(x => x.productDetailsId === productDetailsId ? { ...x, quantity: x.quantity + 1 } : x)
      );
      return true;
    }

    this.items.update(list => [...list, {
      productId,
      productDetailsId,
      productName: product.productName,
      supplierName: product.supplierName,
      barcode: product.barcode,
      unitPrice: product.sellingPrice,
      suggestedPrice: product.sellingPrice,
      quantity: 1,
      notes: '',
      maxQuantity: product.availableQuantity
    }]);
    return true;
  }

  updateQuantity(productDetailsId: number, quantity: number): boolean {
    if (quantity <= 0) {
      this.removeItem(productDetailsId);
      return true;
    }

    const current = this.items().find(x => x.productDetailsId === productDetailsId);
    if (!current || quantity > current.maxQuantity) return false;

    this.items.update(list =>
      list.map(x => x.productDetailsId === productDetailsId ? { ...x, quantity } : x)
    );
    return true;
  }

  updateUnitPrice(productDetailsId: number, unitPrice: number): void {
    if (unitPrice < 0) return;
    this.items.update(list =>
      list.map(x => x.productDetailsId === productDetailsId ? { ...x, unitPrice } : x)
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
    this.notes.set('');
    this.customerName.set('');
    this.customerPhone.set('');
    this.customerId.set(null);
    this.isDeferredPayment.set(false);
    this.amountPaid.set(0);
  }
}

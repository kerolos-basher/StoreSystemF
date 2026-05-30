import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ProductDetails } from '../../shared/models/inventory.models';

@Component({
  selector: 'app-product-details-dialog',
  imports: [MatDialogModule, CurrencyPipe, DatePipe, DecimalPipe],
  templateUrl: './product-details-dialog.component.html',
  styleUrl: './product-details-dialog.component.scss'
})
export class ProductDetailsDialogComponent {
  readonly lineValue = (remaining: number, price: number) => remaining * price;

  get supplierSummary(): { supplier: string; qty: number; value: number }[] {
    const map = new Map<string, { qty: number; value: number }>();
    for (const line of this.data.details.lines) {
      const key = line.supplier || '—';
      const current = map.get(key) ?? { qty: 0, value: 0 };
      current.qty += line.remainingQuantity;
      current.value += line.remainingQuantity * line.purchasePrice;
      map.set(key, current);
    }
    return [...map.entries()].map(([supplier, stats]) => ({ supplier, ...stats }));
  }

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { details: ProductDetails }
  ) {}
}

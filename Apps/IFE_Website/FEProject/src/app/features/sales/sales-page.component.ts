import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { InventoryApiService } from '../../core/services/inventory-api.service';
import { SalesStore } from '../../core/stores/sales.store';
import { BarcodeScannerComponent } from '../../shared/components/barcode-scanner/barcode-scanner.component';

@Component({
  selector: 'app-sales-page',
  standalone: true,
  imports: [CurrencyPipe, FormsModule, MatSnackBarModule, BarcodeScannerComponent],
  templateUrl: './sales-page.component.html',
  styleUrl: './sales-page.component.scss'
})
export class SalesPageComponent {
  private readonly api = inject(InventoryApiService);
  private readonly snackBar = inject(MatSnackBar);
  readonly store = inject(SalesStore);

  readonly completing = signal(false);

  onScan(barcode: string): void {
    this.api.getProductByBarcode(barcode).subscribe({
      next: (product) => {
        if (product.availableQuantity <= 0) {
          this.snackBar.open('الكمية غير متوفرة', 'إغلاق', { duration: 2500 });
          return;
        }
        this.store.addOrIncrement(product);
      },
      error: () => this.snackBar.open('المنتج غير موجود', 'إغلاق', { duration: 2500 })
    });
  }

  completeSale(): void {
    if (this.store.items().length === 0) return;

    this.completing.set(true);
    this.api.createSale(
      this.store.items().map(x => ({
        productId: Number(x.productId),
        quantity: Number(x.quantity),
        notes: x.notes ?? ''
      })),
      this.store.discount(),
      this.store.tax(),
      this.store.notes()
    ).subscribe({
      next: (result) => {
        this.completing.set(false);
        this.store.clear();
        this.snackBar.open(`تم البيع — فاتورة ${result.invoiceNumber}`, 'إغلاق', { duration: 4000 });
      },
      error: (err) => {
        this.completing.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل إتمام البيع', 'إغلاق', { duration: 3500 });
      }
    });
  }
}

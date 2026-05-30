import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { InventoryApiService } from '../../core/services/inventory-api.service';
import { ProductByBarcode } from '../../shared/models/inventory.models';
import { BarcodeScannerComponent } from '../../shared/components/barcode-scanner/barcode-scanner.component';

@Component({
  selector: 'app-price-scanner-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DecimalPipe, MatSnackBarModule, BarcodeScannerComponent],
  templateUrl: './price-scanner-page.component.html',
  styleUrl: './price-scanner-page.component.scss'
})
export class PriceScannerPageComponent {
  private readonly api = inject(InventoryApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly loading = signal(false);
  readonly result = signal<ProductByBarcode | null>(null);
  readonly manualBarcode = signal('');

  onScan(barcode: string): void {
    this.lookup(barcode);
  }

  lookupManual(): void {
    const code = this.manualBarcode().trim();
    if (!code) return;
    this.lookup(code);
  }

  private lookup(barcode: string): void {
    this.loading.set(true);
    this.api.getProductByBarcode(barcode).subscribe({
      next: (product) => {
        this.result.set(product);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.result.set(null);
        this.snackBar.open(err?.error?.message ?? 'المنتج غير موجود', 'إغلاق', { duration: 3000 });
      }
    });
  }
}

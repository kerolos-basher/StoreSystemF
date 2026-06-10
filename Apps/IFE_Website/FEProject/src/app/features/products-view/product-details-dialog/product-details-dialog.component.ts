import { Component, Inject, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QrCodeDialogComponent } from '../../../shared/components/qr-code-dialog/qr-code-dialog.component';
import { ProductDetailsDialogData } from './product-details-dialog.interface';
import { ProductDetailsDialogService } from './product-details-dialog.service';

@Component({
  selector: 'app-product-details-dialog',
  standalone: true,
  providers: [ProductDetailsDialogService],
  imports: [MatDialogModule, MatSnackBarModule, CurrencyPipe, DatePipe, DecimalPipe, FormsModule],
  templateUrl: './product-details-dialog.component.html',
  styleUrl: './product-details-dialog.component.scss'
})
export class ProductDetailsDialogComponent {
  private readonly dialogService = inject(ProductDetailsDialogService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly editingProduct = signal(false);
  readonly editingLineId = signal<string | null>(null);
  productName = '';

  editPurchasePrice = 0;
  editSellingPrice = 0;
  editNotes = '';

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
    @Inject(MAT_DIALOG_DATA) public data: ProductDetailsDialogData
  ) {
    this.productName = data.details.productName;
  }

  startEditProduct(): void {
    this.editingProduct.set(true);
    this.productName = this.data.details.productName;
  }

  saveProduct(): void {
    this.dialogService.updateProduct(this.data.details.id, this.productName).subscribe({
      next: () => {
        this.data.details.productName = this.productName;
        this.editingProduct.set(false);
        this.data.onChanged?.();
        this.snackBar.open('تم تحديث الصنف', 'إغلاق', { duration: 2500 });
      },
      error: (err) => this.snackBar.open(err?.error?.message ?? 'فشل التحديث', 'إغلاق', { duration: 3500 })
    });
  }

  deleteProduct(): void {
    if (!confirm('حذف الصنف بالكامل؟')) return;
    this.dialogService.deleteProduct(this.data.details.id).subscribe({
      next: () => {
        this.data.onChanged?.();
        this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
      },
      error: (err) => this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 })
    });
  }

  startEditLine(lineId: string, purchasePrice: number, sellingPrice: number, notes?: string): void {
    this.editingLineId.set(lineId);
    this.editPurchasePrice = purchasePrice;
    this.editSellingPrice = sellingPrice;
    this.editNotes = notes ?? '';
  }

  saveLine(lineId: string): void {
    this.dialogService.updateProductDetails(this.data.details.id, lineId, {
      purchasePrice: this.editPurchasePrice,
      sellingPrice: this.editSellingPrice,
      notes: this.editNotes
    }).subscribe({
      next: () => {
        this.editingLineId.set(null);
        this.data.onChanged?.();
        this.snackBar.open('تم تحديث الدفعة', 'إغلاق', { duration: 2500 });
      },
      error: (err) => this.snackBar.open(err?.error?.message ?? 'فشل التحديث', 'إغلاق', { duration: 3500 })
    });
  }

  deleteLine(lineId: string): void {
    if (!confirm('حذف هذه الدفعة؟')) return;
    this.dialogService.deleteProductDetails(this.data.details.id, lineId).subscribe({
      next: () => {
        this.data.onChanged?.();
        this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
      },
      error: (err) => this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 })
    });
  }

  openQr(line: { id: string; barcode: string }): void {
    this.dialogService.getQRCode(line.id).subscribe({
      next: (qr) => {
        this.dialog.open(QrCodeDialogComponent, {
          width: '420px',
          maxWidth: '96vw',
          data: { productName: `${this.data.details.productName} — ${line.barcode}`, qr }
        });
      },
      error: (err) => this.snackBar.open(err?.error?.message ?? 'فشل QR', 'إغلاق', { duration: 3500 })
    });
  }
}

import { Component, Inject, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QrCodeDialogComponent } from '../../../shared/components/qr-code-dialog/qr-code-dialog.component';
import { ProductDetails } from '../../../shared/models/inventory.models';
import { ProductDetailsModalData } from './product-details-modal.interfaces';
import { ProductDetailsModalService } from './product-details-modal.service';

@Component({
  selector: 'app-product-details-modal',
  standalone: true,
  providers: [ProductDetailsModalService],
  imports: [MatDialogModule, MatSnackBarModule, CurrencyPipe, DatePipe, DecimalPipe, FormsModule],
  templateUrl: './product-details-modal.component.html',
  styleUrl: './product-details-modal.component.scss'
})
export class ProductDetailsModalComponent {
  private readonly modalService = inject(ProductDetailsModalService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<ProductDetailsModalComponent>);
  private changed = false;

  readonly details = signal<ProductDetails>({} as ProductDetails);
  readonly editingProduct = signal(false);
  readonly editingLineId = signal<string | null>(null);
  productName = '';

  editPurchasePrice = 0;
  editSellingPrice = 0;
  editQuantity = 0;
  editNotes = '';

  constructor(@Inject(MAT_DIALOG_DATA) public data: ProductDetailsModalData) {
    this.details.set(data.details);
    this.productName = data.details.productName;
  }

  get canDeleteProduct(): boolean {
    return this.details().lines.length === 0;
  }

  startEditProduct(): void {
    this.editingProduct.set(true);
    this.productName = this.details().productName;
  }

  saveProduct(): void {
    this.modalService.updateProduct(this.details().id, this.productName).subscribe({
      next: () => {
        this.details.update(d => ({ ...d, productName: this.productName }));
        this.editingProduct.set(false);
        this.changed = true;
        this.snackBar.open('تم تحديث الصنف', 'إغلاق', { duration: 2500 });
      },
      error: err => this.snackBar.open(err?.error?.message ?? 'فشل التحديث', 'إغلاق', { duration: 3500 })
    });
  }

  deleteProduct(): void {
    if (!this.canDeleteProduct) {
      this.snackBar.open('لا يمكن حذف الصنف — يوجد دفعات مرتبطة', 'إغلاق', { duration: 3500 });
      return;
    }
    if (!confirm('حذف الصنف بالكامل؟')) return;
    this.modalService.deleteProduct(this.details().id).subscribe({
      next: () => {
        this.changed = true;
        this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
        this.dialogRef.close(true);
      },
      error: err => this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 })
    });
  }

  startEditLine(lineId: string, purchasePrice: number, sellingPrice: number, quantity: number, notes?: string): void {
    this.editingLineId.set(lineId);
    this.editPurchasePrice = purchasePrice;
    this.editSellingPrice = sellingPrice;
    this.editQuantity = quantity;
    this.editNotes = notes ?? '';
  }

  saveLine(lineId: string): void {
    this.modalService.updateProductDetails(this.details().id, lineId, {
      purchasePrice: this.editPurchasePrice,
      sellingPrice: this.editSellingPrice,
      quantity: this.editQuantity,
      notes: this.editNotes
    }).subscribe({
      next: () => {
        this.editingLineId.set(null);
        this.reloadDetails();
        this.snackBar.open('تم تحديث الدفعة', 'إغلاق', { duration: 2500 });
      },
      error: err => this.snackBar.open(err?.error?.message ?? 'فشل التحديث', 'إغلاق', { duration: 3500 })
    });
  }

  deleteLine(lineId: string, force = false): void {
    const msg = force ? 'حذف إجباري لهذه الدفعة؟' : 'حذف هذه الدفعة؟';
    if (!confirm(msg)) return;
    this.modalService.deleteProductDetails(this.details().id, lineId, force).subscribe({
      next: () => {
        this.reloadDetails();
        this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
      },
      error: err => {
        if (!force) {
          if (confirm('فشل الحذف — هل تريد الحذف الإجباري؟')) {
            this.deleteLine(lineId, true);
          }
        } else {
          this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 });
        }
      }
    });
  }

  openQr(line: { id: string; barcode: string }): void {
    this.modalService.getQRCode(line.id).subscribe({
      next: qr => {
        this.dialog.open(QrCodeDialogComponent, {
          width: '420px',
          maxWidth: '96vw',
          data: { productName: `${this.details().productName} — ${line.barcode}`, qr }
        });
      },
      error: err => this.snackBar.open(err?.error?.message ?? 'فشل QR', 'إغلاق', { duration: 3500 })
    });
  }

  close(): void {
    this.dialogRef.close(this.changed);
  }

  private reloadDetails(): void {
    this.changed = true;
    this.modalService.reloadDetails(this.details().id).subscribe({
      next: d => this.details.set(d),
      error: () => this.snackBar.open('فشل تحديث البيانات', 'إغلاق', { duration: 2500 })
    });
  }
}

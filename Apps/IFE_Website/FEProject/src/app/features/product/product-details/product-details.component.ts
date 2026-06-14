import { Component, Inject, inject, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppDialogService } from '../../../core/services/app-dialog.service';
import { BarcodeLabelDialogComponent } from '../../../shared/components/barcode-label-dialog/barcode-label-dialog.component';
import { Category, ProductDetails, Supplier } from '../../../shared/models/inventory.models';
import { ProductDetailsData } from './product-details.interfaces';
import { ProductDetailsService } from './product-details.service';

@Component({
  selector: 'app-product-details',
  standalone: true,
  providers: [ProductDetailsService],
  imports: [MatDialogModule, MatSelectModule, MatSnackBarModule, CurrencyPipe, DatePipe, DecimalPipe, FormsModule],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.scss'
})
export class ProductDetailsComponent implements OnInit {
  private readonly detailsService = inject(ProductDetailsService);
  private readonly dialog = inject(MatDialog);
  private readonly appDialog = inject(AppDialogService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<ProductDetailsComponent>);
  private changed = false;

  readonly details = signal<ProductDetails>({} as ProductDetails);
  readonly categories = signal<Category[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly editingProduct = signal(false);
  readonly editingLineId = signal<string | null>(null);
  productName = '';

  editSupplierId = '';
  editCategoryId = '';
  editPurchasePrice = 0;
  editSellingPrice = 0;
  editQuantity = 0;
  editNotes = '';

  constructor(@Inject(MAT_DIALOG_DATA) public data: ProductDetailsData) {
    this.details.set(data.details);
    this.productName = data.details.productName;
  }

  ngOnInit(): void {
    this.detailsService.getCategories().subscribe(items => this.categories.set(items));
    this.detailsService.searchSuppliers().subscribe(items => this.suppliers.set(items));
  }

  get canDeleteProduct(): boolean {
    return this.details().lines.length === 0;
  }

  startEditProduct(): void {
    this.editingProduct.set(true);
    this.productName = this.details().productName;
  }

  saveProduct(): void {
    this.detailsService.updateProduct(this.details().id, this.productName).subscribe({
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
    this.appDialog.confirm({
      title: 'حذف الصنف',
      message: 'هل تريد حذف الصنف بالكامل؟',
      confirmText: 'حذف',
      danger: true
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.detailsService.deleteProduct(this.details().id).subscribe({
        next: () => {
          this.changed = true;
          this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
          this.dialogRef.close(true);
        },
        error: err => this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 })
      });
    });
  }

  startEditLine(
    lineId: string,
    supplierId: string | null | undefined,
    categoryId: string | null | undefined,
    purchasePrice: number,
    sellingPrice: number,
    quantity: number,
    notes?: string
  ): void {
    this.editingLineId.set(lineId);
    this.editSupplierId = supplierId ?? this.resolveSupplierIdByName(this.details().lines.find(l => l.id === lineId)?.supplier) ?? '';
    this.editCategoryId = categoryId ?? this.resolveCategoryIdByName(this.details().lines.find(l => l.id === lineId)?.category) ?? '';
    this.editPurchasePrice = purchasePrice;
    this.editSellingPrice = sellingPrice;
    this.editQuantity = quantity;
    this.editNotes = notes ?? '';
  }

  saveLine(lineId: string): void {
    this.detailsService.updateProductDetails(this.details().id, lineId, {
      supplierId: this.editSupplierId || null,
      categoryId: this.editCategoryId || null,
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

  deleteLine(lineId: string): void {
    this.appDialog.confirm({
      title: 'حذف الدفعة',
      message: 'هل تريد حذف هذه الدفعة؟ لا يمكن الحذف إذا كانت مرتبطة بفاتورة مبيعات.',
      confirmText: 'حذف',
      danger: true
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.detailsService.deleteProductDetails(this.details().id, lineId).subscribe({
        next: () => {
          this.reloadDetails();
          this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
        },
        error: err => this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 })
      });
    });
  }

  openBarcode(line: { id: string; barcode: string }): void {
    this.detailsService.getBarcodeLabel(line.id).subscribe({
      next: label => {
        this.dialog.open(BarcodeLabelDialogComponent, {
          width: '420px',
          maxWidth: '96vw',
          data: { productName: `${this.details().productName} — ${line.barcode}`, label }
        });
      },
      error: err => this.snackBar.open(err?.error?.message ?? 'فشل تحميل الباركود', 'إغلاق', { duration: 3500 })
    });
  }

  close(): void {
    this.dialogRef.close(this.changed);
  }

  private reloadDetails(): void {
    this.changed = true;
    this.detailsService.reloadDetails(this.details().id).subscribe({
      next: d => this.details.set(d),
      error: () => this.snackBar.open('فشل تحديث البيانات', 'إغلاق', { duration: 2500 })
    });
  }

  private resolveSupplierIdByName(name?: string): string | null {
    if (!name || name === '—') return null;
    return this.suppliers().find(s => s.name === name)?.id ?? null;
  }

  private resolveCategoryIdByName(name?: string): string | null {
    if (!name || name === '—') return null;
    return this.categories().find(c => c.name === name)?.id ?? null;
  }
}

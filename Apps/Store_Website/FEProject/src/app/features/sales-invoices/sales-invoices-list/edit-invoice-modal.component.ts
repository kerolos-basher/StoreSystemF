import { CurrencyPipe } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SalesInvoiceDetail, SalesInvoiceItem } from '../../../shared/models/inventory.models';
import { EditInvoiceModalData } from './edit-invoice-modal.interfaces';
import { SalesInvoicesListService } from './sales-invoices-list.service';

interface EditableItem extends SalesInvoiceItem {
  productDetailsId: number;
}

@Component({
  selector: 'app-edit-invoice-modal',
  standalone: true,
  imports: [MatDialogModule, MatSnackBarModule, MatCheckboxModule, FormsModule, CurrencyPipe],
  templateUrl: './edit-invoice-modal.component.html',
  styleUrl: './edit-invoice-modal.component.scss'
})
export class EditInvoiceModalComponent {
  private readonly service = inject(SalesInvoicesListService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<EditInvoiceModalComponent>);

  readonly invoice = signal({} as SalesInvoiceDetail);
  readonly items = signal<EditableItem[]>([]);
  readonly saving = signal(false);
  notes = '';
  isDeferredPayment = false;

  constructor(@Inject(MAT_DIALOG_DATA) private readonly data: EditInvoiceModalData) {
    this.invoice.set(this.data.invoice);
    this.notes = this.data.invoice.notes;
    this.isDeferredPayment = this.data.invoice.isDeferredPayment;
    this.items.set(this.data.invoice.items.map(x => ({
      ...x,
      productDetailsId: x.productDetailsId ?? 0
    })) as EditableItem[]);
  }

  save(): void {
    this.saving.set(true);
    this.service.updateInvoice(
      this.invoice().id,
      this.notes,
      this.isDeferredPayment,
      this.items().map(x => ({
        id: x.id ?? null,
        productDetailsId: x.productDetailsId,
        quantity: x.quantity,
        unitPrice: x.unitPrice,
        notes: x.notes ?? ''
      }))
    ).subscribe({
      next: () => {
        this.saving.set(false);
        this.snackBar.open('تم تحديث الفاتورة', 'إغلاق', { duration: 2500 });
        this.dialogRef.close(true);
      },
      error: err => {
        this.saving.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل التحديث', 'إغلاق', { duration: 3500 });
      }
    });
  }
}

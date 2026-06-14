import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AppDialogService } from '../../../core/services/app-dialog.service';
import { DeferredPaymentListItem, DeferredPaymentStatement } from '../../../shared/models/inventory.models';
import { DeferredPaymentsService } from './deferred-payments.service';
import { PaymentStatementModalData } from './payment-statement-modal.interfaces';

@Component({
  selector: 'app-payment-statement-modal',
  standalone: true,
  providers: [DeferredPaymentsService],
  imports: [MatDialogModule, MatSnackBarModule, FormsModule, DatePipe, DecimalPipe],
  templateUrl: './payment-statement-modal.component.html',
  styleUrl: './payment-statement-modal.component.scss'
})
export class PaymentStatementModalComponent implements OnInit {
  private readonly service = inject(DeferredPaymentsService);
  private readonly appDialog = inject(AppDialogService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<PaymentStatementModalComponent>);

  readonly payment = signal({} as DeferredPaymentListItem);
  readonly statement = signal<DeferredPaymentStatement | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly editingTransactionId = signal<number | null>(null);
  editAmount = 0;
  editNotes = '';

  constructor(@Inject(MAT_DIALOG_DATA) data: PaymentStatementModalData) {
    this.payment.set(data.payment);
  }

  ngOnInit(): void {
    this.loadStatement();
  }

  startEdit(transactionId: number, amountPaid: number, notes: string): void {
    this.editingTransactionId.set(transactionId);
    this.editAmount = amountPaid;
    this.editNotes = notes ?? '';
  }

  cancelEdit(): void {
    this.editingTransactionId.set(null);
  }

  saveEdit(): void {
    const transactionId = this.editingTransactionId();
    if (!transactionId || this.editAmount <= 0) {
      this.snackBar.open('أدخل مبلغاً صحيحاً', 'إغلاق', { duration: 2500 });
      return;
    }

    this.saving.set(true);
    this.service.updatePaymentTransaction(this.payment().id, transactionId, this.editAmount, this.editNotes).subscribe({
      next: () => {
        this.saving.set(false);
        this.editingTransactionId.set(null);
        this.snackBar.open('تم تحديث الدفعة', 'إغلاق', { duration: 2500 });
        this.loadStatement(true);
      },
      error: err => {
        this.saving.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل التحديث', 'إغلاق', { duration: 3500 });
      }
    });
  }

  deleteTransaction(transactionId: number): void {
    this.appDialog.confirm({
      title: 'حذف الدفعة',
      message: 'هل تريد حذف هذه الدفعة؟',
      confirmText: 'حذف',
      danger: true
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.saving.set(true);
      this.service.deletePaymentTransaction(this.payment().id, transactionId).subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open('تم حذف الدفعة', 'إغلاق', { duration: 2500 });
          this.loadStatement(true);
        },
        error: err => {
          this.saving.set(false);
          this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 });
        }
      });
    });
  }

  close(): void {
    this.dialogRef.close(true);
  }

  private loadStatement(refreshParent = false): void {
    this.loading.set(true);
    this.service.getStatement(this.payment().id).subscribe({
      next: s => {
        this.statement.set(s);
        this.loading.set(false);
        if (refreshParent) this.dialogRef.disableClose = false;
      },
      error: () => this.loading.set(false)
    });
  }
}

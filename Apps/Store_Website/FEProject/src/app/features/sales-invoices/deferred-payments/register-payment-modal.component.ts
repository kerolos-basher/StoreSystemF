import { DecimalPipe } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DeferredPaymentsService } from './deferred-payments.service';
import { RegisterPaymentModalData } from './register-payment-modal.interfaces';

@Component({
  selector: 'app-register-payment-modal',
  standalone: true,
  imports: [DecimalPipe, MatDialogModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSnackBarModule],
  templateUrl: './register-payment-modal.component.html',
  styleUrl: './register-payment-modal.component.scss'
})
export class RegisterPaymentModalComponent {
  private readonly service = inject(DeferredPaymentsService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<RegisterPaymentModalComponent>);
  private readonly fb = inject(FormBuilder);

  readonly saving = signal(false);

  readonly form = this.fb.group({
    amountPaid: [null as number | null, [Validators.required, Validators.min(0.01)]],
    notes: ['']
  });

  constructor(@Inject(MAT_DIALOG_DATA) public data: RegisterPaymentModalData) {
    this.form.patchValue({ amountPaid: data.payment.remainingAmount });
  }

  submit(): void {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    this.saving.set(true);
    this.service.registerPayment(
      this.data.payment.id,
      Number(raw.amountPaid),
      raw.notes ?? ''
    ).subscribe({
      next: () => {
        this.saving.set(false);
        this.snackBar.open('تم تسجيل الدفعة', 'إغلاق', { duration: 2500 });
        this.dialogRef.close(true);
      },
      error: err => {
        this.saving.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل التسجيل', 'إغلاق', { duration: 3500 });
      }
    });
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { DatePipe, DecimalPipe } from '@angular/common';
import { DeferredPaymentListItem } from '../../../shared/models/inventory.models';
import { DeferredPaymentsService } from './deferred-payments.service';
import { PaymentStatementModalComponent } from './payment-statement-modal.component';
import { RegisterPaymentModalComponent } from './register-payment-modal.component';

@Component({
  selector: 'app-deferred-payments',
  standalone: true,
  providers: [DeferredPaymentsService],
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './deferred-payments.component.html',
  styleUrl: './deferred-payments.component.scss'
})
export class DeferredPaymentsComponent implements OnInit {
  private readonly service = inject(DeferredPaymentsService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);

  readonly payments = signal<DeferredPaymentListItem[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly summary = signal({ totalOutstanding: 0, totalPaid: 0, openCount: 0 });

  pageNumber = 1;
  pageSize = 10;

  readonly filtersForm = this.fb.group({
    customerName: [''],
    isFullyPaid: ['false']
  });

  ngOnInit(): void {
    this.search();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  search(): void {
    this.loading.set(true);
    const raw = this.filtersForm.getRawValue();
    const payload: Record<string, string | number | boolean> = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };
    if (raw.customerName?.trim()) payload['customerName'] = raw.customerName.trim();
    if (raw.isFullyPaid === 'true') payload['isFullyPaid'] = true;
    else if (raw.isFullyPaid === 'false') payload['isFullyPaid'] = false;

    this.service.searchPayments(payload).subscribe({
      next: result => {
        this.payments.set(result.items);
        this.totalCount.set(result.totalCount);
        this.computeSummary(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  resetFilters(): void {
    this.filtersForm.reset({ customerName: '', isFullyPaid: 'false' });
    this.pageNumber = 1;
    this.search();
  }

  openStatement(payment: DeferredPaymentListItem): void {
    const ref = this.dialog.open(PaymentStatementModalComponent, {
      width: '640px',
      maxWidth: '96vw',
      panelClass: 'app-dialog',
      autoFocus: false,
      restoreFocus: true,
      data: { payment }
    });
    ref.afterClosed().subscribe(changed => {
      if (changed) this.search();
    });
  }

  openRegisterModal(payment: DeferredPaymentListItem): void {
    if (payment.isFullyPaid) return;
    const ref = this.dialog.open(RegisterPaymentModalComponent, {
      width: '480px',
      maxWidth: '96vw',
      panelClass: 'app-dialog',
      autoFocus: false,
      restoreFocus: true,
      data: { payment }
    });
    ref.afterClosed().subscribe(changed => {
      if (changed) this.search();
    });
  }

  previousPage(): void {
    if (this.pageNumber <= 1) return;
    this.pageNumber -= 1;
    this.search();
  }

  nextPage(): void {
    if (this.pageNumber >= this.totalPages) return;
    this.pageNumber += 1;
    this.search();
  }

  private computeSummary(items: DeferredPaymentListItem[]): void {
    this.summary.set({
      totalOutstanding: items.reduce((s, p) => s + p.remainingAmount, 0),
      totalPaid: items.reduce((s, p) => s + p.paidAmount, 0),
      openCount: items.filter(p => !p.isFullyPaid).length
    });
  }
}

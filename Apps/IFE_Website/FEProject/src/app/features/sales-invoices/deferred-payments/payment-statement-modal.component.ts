import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { DeferredPaymentListItem, DeferredPaymentStatement } from '../../../shared/models/inventory.models';
import { DeferredPaymentsService } from './deferred-payments.service';
import { PaymentStatementModalData } from './payment-statement-modal.interfaces';

@Component({
  selector: 'app-payment-statement-modal',
  standalone: true,
  providers: [DeferredPaymentsService],
  imports: [MatDialogModule, DatePipe, DecimalPipe],
  templateUrl: './payment-statement-modal.component.html',
  styleUrl: './payment-statement-modal.component.scss'
})
export class PaymentStatementModalComponent implements OnInit {
  private readonly service = inject(DeferredPaymentsService);

  readonly payment = signal({} as DeferredPaymentListItem);
  readonly statement = signal<DeferredPaymentStatement | null>(null);
  readonly loading = signal(true);

  constructor(@Inject(MAT_DIALOG_DATA) data: PaymentStatementModalData) {
    this.payment.set(data.payment);
  }

  ngOnInit(): void {
    this.service.getStatement(this.payment().id).subscribe({
      next: s => { this.statement.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}

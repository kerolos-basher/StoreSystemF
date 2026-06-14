import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AppDialogService } from '../../../core/services/app-dialog.service';
import { SalesInvoiceListItem } from '../../../shared/models/inventory.models';
import { EditInvoiceModalComponent } from './edit-invoice-modal.component';
import { formatLocalDate } from '../../../shared/utils/date-format';
import { SalesInvoicesListService } from './sales-invoices-list.service';

@Component({
  selector: 'app-sales-invoices-list',
  standalone: true,
  providers: [SalesInvoicesListService],
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './sales-invoices-list.component.html',
  styleUrl: './sales-invoices-list.component.scss'
})
export class SalesInvoicesListComponent implements OnInit {
  private readonly service = inject(SalesInvoicesListService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly appDialog = inject(AppDialogService);
  private readonly snackBar = inject(MatSnackBar);

  readonly invoices = signal<SalesInvoiceListItem[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly expandedId = signal<string | null>(null);
  readonly summary = signal({ totalSales: 0, invoiceCount: 0, itemsSold: 0 });

  pageNumber = 1;
  pageSize = 10;

  readonly filtersForm = this.fb.group({
    dateFrom: [null as Date | null],
    dateTo: [null as Date | null],
    invoiceNumber: ['']
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
    const payload: Record<string, string | number> = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    if (raw.dateFrom) payload['dateFrom'] = formatLocalDate(raw.dateFrom);
    if (raw.dateTo) payload['dateTo'] = formatLocalDate(raw.dateTo);
    if (raw.invoiceNumber?.trim()) payload['invoiceNumber'] = raw.invoiceNumber.trim();

    this.service.searchInvoices(payload).subscribe({
      next: result => {
        this.invoices.set(result.items);
        this.totalCount.set(result.totalCount);
        this.computeSummary(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  resetFilters(): void {
    this.filtersForm.reset({ dateFrom: null, dateTo: null, invoiceNumber: '' });
    this.pageNumber = 1;
    this.search();
  }

  toggleExpand(id: string): void {
    this.expandedId.update(current => current === id ? null : id);
  }

  editInvoice(inv: SalesInvoiceListItem, event?: Event): void {
    event?.stopPropagation();
    this.service.getInvoice(Number(inv.id)).subscribe({
      next: invoice => {
        this.dialog.open(EditInvoiceModalComponent, {
          width: '720px',
          maxWidth: '96vw',
          panelClass: 'app-dialog',
          autoFocus: false,
          restoreFocus: true,
          data: { invoice }
        }).afterClosed().subscribe(changed => {
          if (changed) this.search();
        });
      },
      error: () => this.snackBar.open('فشل تحميل الفاتورة', 'إغلاق', { duration: 2500 })
    });
  }

  deleteInvoice(inv: SalesInvoiceListItem, event?: Event): void {
    event?.stopPropagation();
    this.appDialog.confirm({
      title: 'حذف الفاتورة',
      message: `هل تريد حذف الفاتورة ${inv.invoiceNumber}؟ لا يمكن التراجع عن هذا الإجراء.`,
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      danger: true
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.service.deleteInvoice(Number(inv.id)).subscribe({
        next: () => {
          this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 });
          this.search();
        },
        error: err => this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 })
      });
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

  private computeSummary(items: SalesInvoiceListItem[]): void {
    this.summary.set({
      totalSales: items.reduce((s, i) => s + i.grandTotal, 0),
      invoiceCount: this.totalCount(),
      itemsSold: items.reduce((s, i) => s + i.items.reduce((a, x) => a + x.quantity, 0), 0)
    });
  }
}

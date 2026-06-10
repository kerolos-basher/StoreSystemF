import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { InventoryApiService } from '../../core/services/inventory-api.service';
import { SalesInvoiceListItem } from '../../shared/models/inventory.models';

@Component({
  selector: 'app-sales-invoices-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './sales-invoices-page.component.html',
  styleUrl: './sales-invoices-page.component.scss'
})
export class SalesInvoicesPageComponent implements OnInit {
  private readonly api = inject(InventoryApiService);
  private readonly fb = inject(FormBuilder);

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

    if (raw.dateFrom) payload['dateFrom'] = raw.dateFrom.toISOString();
    if (raw.dateTo) payload['dateTo'] = raw.dateTo.toISOString();
    if (raw.invoiceNumber?.trim()) payload['invoiceNumber'] = raw.invoiceNumber.trim();

    this.api.searchSalesInvoices(payload).subscribe({
      next: (result) => {
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
      invoiceCount: items.length,
      itemsSold: items.reduce((s, i) => s + i.items.reduce((a, x) => a + x.quantity, 0), 0)
    });
  }
}

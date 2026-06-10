import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, Observable, switchMap } from 'rxjs';
import {
  CustomerAutoComplete,
  ReturnInvoiceListItem,
  SalesInvoiceListItem
} from '../../shared/models/inventory.models';
import { ReturnLineDraft, SelectedInvoice } from './returns.interfaces';
import { ReturnsService } from './returns.service';

@Component({
  selector: 'app-returns',
  standalone: true,
  providers: [ReturnsService],
  imports: [
    AsyncPipe,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatSnackBarModule,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './returns.component.html',
  styleUrl: './returns.component.scss'
})
export class ReturnsComponent implements OnInit {
  private readonly service = inject(ReturnsService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  readonly saving = signal(false);
  readonly loadingHistory = signal(false);
  readonly returnReasons = signal<{ id: number; name: string; isReturnToStock: boolean }[]>([]);
  readonly selectedInvoices = signal<SelectedInvoice[]>([]);
  readonly customerInvoices = signal<SalesInvoiceListItem[]>([]);
  readonly returnsHistory = signal<ReturnInvoiceListItem[]>([]);

  filteredCustomers$!: Observable<CustomerAutoComplete[]>;
  returnReasonType = 4;
  notes = '';

  readonly searchForm = this.fb.group({
    invoiceNumber: [''],
    customerName: ['']
  });

  ngOnInit(): void {
    this.service.getReturnReasons().subscribe(items => this.returnReasons.set(items));
    this.loadHistory();

    this.filteredCustomers$ = this.searchForm.controls.customerName.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter((v): v is string => typeof v === 'string' && v.trim().length >= 2),
      switchMap(term => this.service.searchCustomers(term.trim()))
    );
  }

  loadByInvoiceNumber(): void {
    const term = (this.searchForm.controls.invoiceNumber.value ?? '').trim();
    if (!term) return;

    this.service.getInvoiceByNumber(term).subscribe({
      next: invoice => this.addInvoice(invoice.id, invoice.invoiceNumber, invoice.items),
      error: () => this.snackBar.open('الفاتورة غير موجودة', 'إغلاق', { duration: 2500 })
    });
  }

  selectCustomer(customer: CustomerAutoComplete): void {
    this.searchForm.patchValue({ customerName: customer.name });
    this.service.getCustomerInvoices(customer.id).subscribe({
      next: invoices => this.customerInvoices.set(invoices),
      error: () => this.snackBar.open('فشل تحميل فواتير العميل', 'إغلاق', { duration: 2500 })
    });
  }

  displayCustomer(item: CustomerAutoComplete | string): string {
    return typeof item === 'string' ? item : item.name;
  }

  loadCustomerInvoice(inv: SalesInvoiceListItem): void {
    this.service.getSalesInvoice(Number(inv.id)).subscribe({
      next: invoice => this.addInvoice(invoice.id, invoice.invoiceNumber, invoice.items),
      error: () => this.snackBar.open('فشل تحميل الفاتورة', 'إغلاق', { duration: 2500 })
    });
  }

  removeInvoice(invoiceId: number): void {
    this.selectedInvoices.update(list => list.filter(x => x.id !== invoiceId));
  }

  submitReturn(invoice: SelectedInvoice): void {
    const items = invoice.lines
      .filter(x => x.quantity > 0)
      .map(x => ({
        salesInvoiceItemId: x.salesInvoiceItemId,
        quantity: x.quantity,
        itemReasonType: x.itemReasonType,
        notes: x.notes
      }));

    if (items.length === 0) {
      this.snackBar.open('أدخل كمية مرتجع واحدة على الأقل', 'إغلاق', { duration: 2500 });
      return;
    }

    this.saving.set(true);
    this.service.createReturn({
      salesInvoiceId: invoice.id,
      returnReasonType: this.returnReasonType,
      notes: this.notes,
      items
    }).subscribe({
      next: result => {
        this.saving.set(false);
        this.snackBar.open(`تم تسجيل المرتجع ${result.returnNumber}`, 'إغلاق', { duration: 4000 });
        this.removeInvoice(invoice.id);
        this.loadHistory();
      },
      error: err => {
        this.saving.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل تسجيل المرتجع', 'إغلاق', { duration: 3500 });
      }
    });
  }

  loadHistory(): void {
    this.loadingHistory.set(true);
    this.service.searchReturns({ pageNumber: 1, pageSize: 20 }).subscribe({
      next: result => {
        this.returnsHistory.set(result.items);
        this.loadingHistory.set(false);
      },
      error: () => this.loadingHistory.set(false)
    });
  }

  private addInvoice(
    id: number,
    invoiceNumber: string,
    items: { id?: number; productName: string; quantity: number; returnedQuantity?: number; availableForReturn?: number }[]
  ): void {
    if (this.selectedInvoices().some(x => x.id === id)) {
      this.snackBar.open('الفاتورة مضافة بالفعل', 'إغلاق', { duration: 2000 });
      return;
    }

    const lines: ReturnLineDraft[] = items
      .filter(x => (x.availableForReturn ?? (x.quantity - (x.returnedQuantity ?? 0))) > 0)
      .map(x => ({
        salesInvoiceItemId: x.id!,
        productName: x.productName,
        availableForReturn: x.availableForReturn ?? (x.quantity - (x.returnedQuantity ?? 0)),
        quantity: 0,
        itemReasonType: this.returnReasonType,
        notes: ''
      }));

    if (lines.length === 0) {
      this.snackBar.open('لا توجد أصناف متاحة للإرجاع', 'إغلاق', { duration: 2500 });
      return;
    }

    this.selectedInvoices.update(list => [...list, { id, invoiceNumber, lines }]);
  }
}

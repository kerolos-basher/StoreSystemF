import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, Observable, switchMap } from 'rxjs';
import {
  CustomerAutoComplete,
  ReturnInvoiceListItem,
  SalesInvoiceListItem
} from '../../shared/models/inventory.models';
import { formatLocalDate } from '../../shared/utils/date-format';
import { ReturnLineDraft, SelectedInvoice } from './returns.interfaces';
import { ReturnsService } from './returns.service';

type ReturnStep = 'search' | 'select' | 'create';

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
    MatDatepickerModule,
    MatNativeDateModule,
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
  readonly searchingInvoices = signal(false);
  readonly returnReasons = signal<{ id: number; name: string; isReturnToStock: boolean }[]>([]);
  readonly searchResults = signal<SalesInvoiceListItem[]>([]);
  readonly returnsHistory = signal<ReturnInvoiceListItem[]>([]);
  readonly currentStep = signal<ReturnStep>('search');
  readonly activeInvoice = signal<SelectedInvoice | null>(null);

  filteredCustomers$!: Observable<CustomerAutoComplete[]>;
  returnReasonType = 4;
  notes = '';

  readonly searchForm = this.fb.group({
    invoiceNumber: [''],
    customerName: [''],
    productName: [''],
    saleDate: [null as Date | null]
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

  searchInvoices(): void {
    const raw = this.searchForm.getRawValue();
    const invoiceNumber = (raw.invoiceNumber ?? '').trim();

    if (invoiceNumber) {
      this.searchingInvoices.set(true);
      this.service.getInvoiceByNumber(invoiceNumber).subscribe({
        next: invoice => {
          this.searchResults.set([{
            id: String(invoice.id),
            invoiceNumber: invoice.invoiceNumber,
            saleDate: invoice.saleDate,
            customerId: invoice.customerId,
            customerName: invoice.customerName,
            customerPhone: invoice.customerPhone,
            subtotal: invoice.subtotal,
            grandTotal: invoice.grandTotal,
            isDeferredPayment: invoice.isDeferredPayment,
            itemCount: invoice.items.length,
            items: invoice.items
          }]);
          this.searchingInvoices.set(false);
          this.currentStep.set('select');
        },
        error: () => {
          this.searchingInvoices.set(false);
          this.snackBar.open('الفاتورة غير موجودة', 'إغلاق', { duration: 2500 });
        }
      });
      return;
    }

    const productName = (raw.productName ?? '').trim();
    const saleDate = raw.saleDate;
    const customerName = typeof raw.customerName === 'string' ? raw.customerName.trim() : '';

    if (!productName && !saleDate && !customerName) {
      this.snackBar.open('أدخل رقم الفاتورة أو اسم المنتج أو التاريخ أو العميل', 'إغلاق', { duration: 2500 });
      return;
    }

    const query: Record<string, string | number | boolean> = {
      pageNumber: 1,
      pageSize: 50
    };
    if (productName) query['productName'] = productName;
    if (customerName) query['customerTerm'] = customerName;
    if (saleDate) {
      const from = formatLocalDate(saleDate);
      query['dateFrom'] = from;
      query['dateTo'] = from;
    }

    this.searchingInvoices.set(true);
    this.service.searchInvoices(query).subscribe({
      next: result => {
        this.searchResults.set(result.items);
        this.searchingInvoices.set(false);
        this.currentStep.set('select');
        if (result.items.length === 0) {
          this.snackBar.open('لا توجد فواتير مطابقة', 'إغلاق', { duration: 2500 });
        }
      },
      error: () => {
        this.searchingInvoices.set(false);
        this.snackBar.open('فشل البحث', 'إغلاق', { duration: 2500 });
      }
    });
  }

  selectCustomer(customer: CustomerAutoComplete): void {
    this.searchForm.patchValue({ customerName: customer.name });
    this.searchInvoices();
  }

  displayCustomer(item: CustomerAutoComplete | string): string {
    return typeof item === 'string' ? item : item.name;
  }

  chooseInvoice(inv: SalesInvoiceListItem): void {
    this.service.getSalesInvoice(Number(inv.id)).subscribe({
      next: invoice => {
        const selected = this.buildSelectedInvoice(invoice.id, invoice.invoiceNumber, invoice.items);
        if (!selected) return;
        this.activeInvoice.set(selected);
        this.currentStep.set('create');
      },
      error: () => this.snackBar.open('فشل تحميل الفاتورة', 'إغلاق', { duration: 2500 })
    });
  }

  backToSearch(): void {
    this.currentStep.set('search');
    this.activeInvoice.set(null);
    this.searchResults.set([]);
  }

  backToSelect(): void {
    this.currentStep.set('select');
    this.activeInvoice.set(null);
  }

  submitReturn(): void {
    const invoice = this.activeInvoice();
    if (!invoice) return;

    const items = invoice.lines
      .filter(x => x.quantity > 0)
      .map(x => ({
        salesInvoiceItemId: x.salesInvoiceItemId,
        quantity: x.quantity,
        unitPrice: x.unitPrice,
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
        this.notes = '';
        this.activeInvoice.set(null);
        this.searchResults.set([]);
        this.searchForm.reset({ invoiceNumber: '', customerName: '', productName: '', saleDate: null });
        this.currentStep.set('search');
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

  private buildSelectedInvoice(
    id: number,
    invoiceNumber: string,
    items: { id?: number; productName: string; quantity: number; returnedQuantity?: number; availableForReturn?: number; unitPrice: number }[]
  ): SelectedInvoice | null {
    const lines: ReturnLineDraft[] = items
      .filter(x => (x.availableForReturn ?? (x.quantity - (x.returnedQuantity ?? 0))) > 0)
      .map(x => ({
        salesInvoiceItemId: x.id!,
        productName: x.productName,
        availableForReturn: x.availableForReturn ?? (x.quantity - (x.returnedQuantity ?? 0)),
        soldUnitPrice: x.unitPrice,
        unitPrice: x.unitPrice,
        quantity: 0,
        itemReasonType: this.returnReasonType,
        notes: ''
      }));

    if (lines.length === 0) {
      this.snackBar.open('لا توجد أصناف متاحة للإرجاع في هذه الفاتورة', 'إغلاق', { duration: 2500 });
      return null;
    }

    return { id, invoiceNumber, lines };
  }
}

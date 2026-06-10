import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ReturnLineDraft } from './returns-page.interface';
import { ReturnsPageService } from './returns-page.service';

@Component({
  selector: 'app-returns-page',
  standalone: true,
  providers: [ReturnsPageService],
  imports: [FormsModule, MatSnackBarModule],
  templateUrl: './returns-page.component.html',
  styleUrl: './returns-page.component.scss'
})
export class ReturnsPageComponent implements OnInit {
  private readonly pageService = inject(ReturnsPageService);
  private readonly snackBar = inject(MatSnackBar);

  readonly saving = signal(false);
  readonly returnReasons = signal<{ id: number; name: string; isReturnToStock: boolean }[]>([]);
  readonly lines = signal<ReturnLineDraft[]>([]);

  invoiceNumber = '';
  returnReasonType = 4;
  notes = '';
  loadedInvoiceNumber = '';
  loadedInvoiceId: number | null = null;

  ngOnInit(): void {
    this.pageService.getReturnReasons().subscribe(items => this.returnReasons.set(items));
  }

  loadInvoice(): void {
    const term = this.invoiceNumber.trim();
    if (!term) return;

    this.pageService.searchSalesInvoices(term).subscribe({
      next: (result) => {
        const match = result.items.find(x => x.invoiceNumber.toLowerCase().includes(term.toLowerCase()));
        if (!match) {
          this.snackBar.open('الفاتورة غير موجودة', 'إغلاق', { duration: 2500 });
          return;
        }

        this.pageService.getSalesInvoice(Number(match.id)).subscribe({
          next: (invoice) => {
            this.loadedInvoiceId = invoice.id;
            this.loadedInvoiceNumber = invoice.invoiceNumber;
            this.lines.set(invoice.items
              .filter(x => (x.availableForReturn ?? (x.quantity - (x.returnedQuantity ?? 0))) > 0)
              .map(x => ({
                salesInvoiceItemId: x.id!,
                productName: x.productName,
                availableForReturn: x.availableForReturn ?? (x.quantity - (x.returnedQuantity ?? 0)),
                quantity: 0,
                itemReasonType: this.returnReasonType,
                notes: ''
              })));
          },
          error: () => this.snackBar.open('فشل تحميل الفاتورة', 'إغلاق', { duration: 2500 })
        });
      },
      error: () => this.snackBar.open('فشل البحث', 'إغلاق', { duration: 2500 })
    });
  }

  submitReturn(): void {
    if (!this.loadedInvoiceId) return;

    const items = this.lines()
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
    this.pageService.createReturn({
      salesInvoiceId: this.loadedInvoiceId,
      returnReasonType: this.returnReasonType,
      notes: this.notes,
      items
    }).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.snackBar.open(`تم تسجيل المرتجع ${result.returnNumber}`, 'إغلاق', { duration: 4000 });
        this.lines.set([]);
        this.loadedInvoiceId = null;
        this.loadedInvoiceNumber = '';
        this.invoiceNumber = '';
      },
      error: (err) => {
        this.saving.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل تسجيل المرتجع', 'إغلاق', { duration: 3500 });
      }
    });
  }
}

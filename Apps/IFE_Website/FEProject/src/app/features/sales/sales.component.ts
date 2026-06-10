import { AsyncPipe, CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, Observable, switchMap } from 'rxjs';
import { SalesStore } from '../../core/stores/sales.store';
import { BarcodeScannerComponent } from '../../shared/components/barcode-scanner/barcode-scanner.component';
import { CustomerAutoComplete } from '../../shared/models/inventory.models';
import { SalesService } from './sales.service';

@Component({
  selector: 'app-sales',
  standalone: true,
  providers: [SalesService],
  imports: [
    AsyncPipe,
    CurrencyPipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatCheckboxModule,
    MatSnackBarModule,
    BarcodeScannerComponent
  ],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.scss'
})
export class SalesComponent implements OnInit {
  private readonly service = inject(SalesService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);
  readonly store = inject(SalesStore);

  readonly completing = signal(false);
  filteredCustomers$!: Observable<CustomerAutoComplete[]>;

  readonly customerForm = this.fb.group({
    customerName: [''],
    customerPhone: [''],
    isDeferredPayment: [false]
  });

  ngOnInit(): void {
    this.filteredCustomers$ = this.customerForm.controls.customerName.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter((v): v is string => typeof v === 'string' && v.trim().length >= 2),
      switchMap(term => this.service.searchCustomers(term.trim()))
    );
  }

  onScan(barcode: string): void {
    this.service.searchByBarcode(barcode).subscribe({
      next: product => {
        if (product.remainingQuantity <= 0) {
          this.snackBar.open('الكمية غير متوفرة', 'إغلاق', { duration: 2500 });
          return;
        }
        this.store.addOrIncrement({
          productId: product.productId,
          productDetailsId: product.productDetailsId,
          productName: product.productName,
          supplierName: product.supplierName,
          barcode: product.barcode,
          sellingPrice: product.suggestedSellingPrice,
          availableQuantity: product.remainingQuantity
        });
      },
      error: () => this.snackBar.open('المنتج غير موجود', 'إغلاق', { duration: 2500 })
    });
  }

  selectCustomer(customer: CustomerAutoComplete): void {
    this.customerForm.patchValue({ customerName: customer.name, customerPhone: customer.phone });
    this.store.customerId.set(customer.id);
    this.store.customerName.set(customer.name);
    this.store.customerPhone.set(customer.phone);
  }

  displayCustomer(item: CustomerAutoComplete | string): string {
    return typeof item === 'string' ? item : item.name;
  }

  onCustomerNameChange(): void {
    const name = this.customerForm.controls.customerName.value ?? '';
    this.store.customerName.set(name);
    this.store.customerId.set(null);
  }

  onCustomerPhoneChange(): void {
    this.store.customerPhone.set(this.customerForm.controls.customerPhone.value ?? '');
  }

  onDeferredChange(checked: boolean): void {
    this.store.isDeferredPayment.set(checked);
  }

  completeSale(): void {
    if (this.store.items().length === 0) return;

    const raw = this.customerForm.getRawValue();
    this.completing.set(true);
    this.service.createSale(
      this.store.items().map(x => ({
        productDetailsId: x.productDetailsId,
        quantity: x.quantity,
        unitPrice: x.unitPrice,
        notes: x.notes ?? ''
      })),
      raw.customerName ?? '',
      raw.customerPhone ?? '',
      this.store.customerId(),
      this.store.notes(),
      !!raw.isDeferredPayment
    ).subscribe({
      next: result => {
        this.completing.set(false);
        this.store.clear();
        this.customerForm.reset({ customerName: '', customerPhone: '', isDeferredPayment: false });
        this.snackBar.open(`تم البيع — فاتورة ${result.invoiceNumber}`, 'إغلاق', { duration: 4000 });
      },
      error: err => {
        this.completing.set(false);
        this.snackBar.open(err?.error?.message ?? 'فشل إتمام البيع', 'إغلاق', { duration: 3500 });
      }
    });
  }
}

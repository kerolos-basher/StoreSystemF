import { AsyncPipe, DecimalPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, Observable, switchMap } from 'rxjs';
import { SalesStore } from '../../core/stores/sales.store';
import { ReceiptPrintService } from '../../shared/services/receipt-print.service';
import { CustomerAutoComplete, ProductDetailsAutoComplete, ProductDetailsSearch } from '../../shared/models/inventory.models';
import { displayProductWithSupplier } from '../../shared/utils/product-autocomplete-display';
import { SalesService } from './sales.service';

@Component({
  selector: 'app-sales',
  standalone: true,
  providers: [SalesService],
  imports: [
    AsyncPipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.scss'
})
export class SalesComponent implements OnInit, AfterViewInit {
  private readonly service = inject(SalesService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);
  private readonly printService = inject(ReceiptPrintService);
  readonly store = inject(SalesStore);

  @ViewChild('barcodeInput') barcodeInput?: ElementRef<HTMLInputElement>;

  readonly completing = signal(false);
  filteredCustomers$!: Observable<CustomerAutoComplete[]>;
  filteredProducts$!: Observable<ProductDetailsAutoComplete[]>;

  readonly customerForm = this.fb.group({
    customerName: [''],
    customerPhone: [''],
    isDeferredPayment: [false],
    amountPaid: [{ value: 0, disabled: true }]
  });

  readonly productSearchForm = this.fb.group({
    barcode: [''],
    productName: ['']
  });

  private readonly syncPaidWithTotal = effect(() => {
    const total = this.store.grandTotal();
    const paid = this.store.amountPaid();
    if (paid > total) {
      this.store.amountPaid.set(total);
      if (this.customerForm.controls.amountPaid.enabled) {
        this.customerForm.controls.amountPaid.setValue(total, { emitEvent: false });
      }
    }
  });

  ngAfterViewInit(): void {
    this.focusBarcodeInput();
  }

  focusBarcodeInput(): void {
    setTimeout(() => this.barcodeInput?.nativeElement.focus(), 0);
  }

  ngOnInit(): void {
    this.filteredCustomers$ = this.customerForm.controls.customerName.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter((v): v is string => typeof v === 'string' && v.trim().length >= 2),
      switchMap(term => this.service.searchCustomers(term.trim()))
    );

    this.filteredProducts$ = this.productSearchForm.controls.productName.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter((v): v is string => typeof v === 'string' && v.trim().length >= 2),
      switchMap(term => this.service.searchAutocomplete(term.trim()))
    );

    this.customerForm.controls.amountPaid.valueChanges.subscribe(value => {
      const paid = Math.max(0, Number(value) || 0);
      const capped = Math.min(paid, this.store.grandTotal());
      if (paid !== capped) {
        this.customerForm.controls.amountPaid.setValue(capped, { emitEvent: false });
      }
      this.store.amountPaid.set(capped);
    });
  }

  onBarcodeKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.lookupBarcode();
    }
  }

  lookupBarcode(barcode?: string): void {
    const code = (barcode ?? this.productSearchForm.controls.barcode.value ?? '').trim();
    if (!code) return;

    this.service.searchByBarcode(code).subscribe({
      next: product => {
        this.productSearchForm.patchValue({ barcode: product.barcode, productName: product.productName });
        this.addProduct(product);
      },
      error: () => this.snackBar.open('المنتج غير موجود', 'إغلاق', { duration: 2500 })
    });
  }

  selectProduct(item: ProductDetailsAutoComplete): void {
    this.productSearchForm.patchValue({ productName: item.productName, barcode: item.barcode });
    this.addProduct({
      productDetailsId: item.productDetailsId,
      productId: item.productId,
      productName: item.productName,
      barcode: item.barcode,
      purchasePrice: item.purchasePrice,
      suggestedSellingPrice: item.suggestedSellingPrice,
      supplierName: item.supplierName,
      categoryName: '',
      remainingQuantity: item.remainingQuantity,
      notes: ''
    });
  }

  displayProduct = displayProductWithSupplier;

  private addProduct(product: ProductDetailsSearch): void {
    if (product.remainingQuantity <= 0) {
      this.snackBar.open('لا توجد كمية كافية في المخزون', 'إغلاق', { duration: 3000, panelClass: ['app-error-snackbar'] });
      return;
    }

    const added = this.store.addOrIncrement({
      productId: product.productId,
      productDetailsId: product.productDetailsId,
      productName: product.productName,
      supplierName: product.supplierName,
      barcode: product.barcode,
      sellingPrice: product.suggestedSellingPrice,
      availableQuantity: product.remainingQuantity
    });

    if (!added) {
      this.snackBar.open('لا توجد كمية كافية في المخزون', 'إغلاق', { duration: 3000, panelClass: ['app-error-snackbar'] });
    }
  }

  onQuantityChange(productDetailsId: number, quantity: number, maxQuantity: number): void {
    if (!this.store.updateQuantity(productDetailsId, quantity)) {
      this.snackBar.open(`لا توجد كمية كافية في المخزون. الحد الأقصى ${maxQuantity}`, 'إغلاق', {
        duration: 3500,
        panelClass: ['app-error-snackbar']
      });
      this.store.updateQuantity(productDetailsId, maxQuantity);
    }
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
    const amountControl = this.customerForm.controls.amountPaid;
    if (checked) {
      amountControl.enable();
      amountControl.setValue(0);
      this.store.amountPaid.set(0);
    } else {
      amountControl.disable();
      amountControl.setValue(0);
      this.store.amountPaid.set(0);
    }
  }

  completeSale(): void {
    if (this.store.items().length === 0) return;

    const raw = this.customerForm.getRawValue();
    const isDeferred = !!raw.isDeferredPayment;
    const amountPaid = isDeferred ? Math.max(0, Number(raw.amountPaid) || 0) : this.store.grandTotal();

    if (isDeferred && !raw.customerName?.trim()) {
      this.snackBar.open('يجب إدخال اسم العميل للدفع الآجل', 'إغلاق', { duration: 3000 });
      return;
    }

    if (isDeferred && amountPaid > this.store.grandTotal()) {
      this.snackBar.open('المبلغ المدفوع أكبر من إجمالي الفاتورة', 'إغلاق', { duration: 3000 });
      return;
    }

    this.completing.set(true);
    const saleItems = this.store.items().map(x => ({
      productDetailsId: x.productDetailsId,
      quantity: x.quantity,
      unitPrice: x.unitPrice,
      notes: x.notes ?? ''
    }));
    const receiptItems = this.store.items().map(x => ({
      productName: x.productName,
      quantity: x.quantity,
      unitPrice: x.unitPrice
    }));
    const grandTotal = this.store.grandTotal();

    this.service.createSale(
      saleItems,
      raw.customerName ?? '',
      raw.customerPhone ?? '',
      this.store.customerId(),
      this.store.notes(),
      isDeferred,
      amountPaid
    ).subscribe({
      next: result => {
        this.completing.set(false);
        this.printService.printSaleReceipt({
          invoiceNumber: result.invoiceNumber,
          items: receiptItems,
          grandTotal,
          amountPaid,
          remainingAmount: Math.max(0, grandTotal - amountPaid),
          isDeferred,
          customerName: raw.customerName ?? ''
        });
        this.store.clear();
        this.customerForm.reset({ customerName: '', customerPhone: '', isDeferredPayment: false, amountPaid: 0 });
        this.customerForm.controls.amountPaid.disable();
        this.productSearchForm.reset({ barcode: '', productName: '' });
        this.focusBarcodeInput();
        this.snackBar.open(`تم البيع — فاتورة ${result.invoiceNumber}`, 'إغلاق', { duration: 4000 });
      },
      error: () => {
        this.completing.set(false);
      }
    });
  }
}

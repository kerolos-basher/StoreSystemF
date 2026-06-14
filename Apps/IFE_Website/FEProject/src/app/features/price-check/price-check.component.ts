import { AsyncPipe, DecimalPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, Observable, switchMap } from 'rxjs';
import { ReceiptPrintService } from '../../shared/services/receipt-print.service';
import { ProductDetailsAutoComplete, ProductDetailsSearch } from '../../shared/models/inventory.models';
import { displayProductWithSupplier } from '../../shared/utils/product-autocomplete-display';
import { PriceCheckService } from './price-check.service';

@Component({
  selector: 'app-price-check',
  standalone: true,
  providers: [PriceCheckService],
  imports: [
    AsyncPipe, DecimalPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatSnackBarModule
  ],
  templateUrl: './price-check.component.html',
  styleUrl: './price-check.component.scss'
})
export class PriceCheckComponent implements OnInit, AfterViewInit {
  private readonly service = inject(PriceCheckService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly printService = inject(ReceiptPrintService);

  @ViewChild('barcodeInput') barcodeInput?: ElementRef<HTMLInputElement>;

  readonly loading = signal(false);
  readonly result = signal<ProductDetailsSearch | null>(null);
  filteredProducts$!: Observable<ProductDetailsAutoComplete[]>;

  readonly searchForm = this.fb.group({
    barcode: [''],
    productName: ['']
  });

  ngAfterViewInit(): void {
    setTimeout(() => this.barcodeInput?.nativeElement.focus(), 0);
  }

  ngOnInit(): void {
    this.filteredProducts$ = this.searchForm.controls.productName.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter((v): v is string => typeof v === 'string' && v.trim().length >= 2),
      switchMap(term => this.service.searchAutocomplete(term.trim()))
    );
  }

  onScan(barcode: string): void {
    this.lookup(barcode);
  }

  onBarcodeKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.lookupBarcode();
    }
  }

  printLabel(): void {
    const product = this.result();
    if (!product) return;

    this.printService.printPriceLabel({
      productName: product.productName,
      barcode: product.barcode,
      suggestedPrice: product.suggestedSellingPrice,
      purchasePrice: product.purchasePrice,
      supplierName: product.supplierName
    });
  }

  lookupBarcode(): void {
    const code = (this.searchForm.controls.barcode.value ?? '').trim();
    if (!code) return;
    this.lookup(code);
  }

  selectProduct(item: ProductDetailsAutoComplete): void {
    this.searchForm.patchValue({ productName: item.productName, barcode: item.barcode });
    this.lookup(item.barcode);
  }

  displayProduct = displayProductWithSupplier;

  private lookup(barcode: string): void {
    this.loading.set(true);
    this.service.searchByBarcode(barcode).subscribe({
      next: product => {
        this.result.set(product);
        this.searchForm.patchValue({ barcode: product.barcode, productName: product.productName });
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.result.set(null);
        this.snackBar.open(err?.error?.message ?? 'المنتج غير موجود', 'إغلاق', { duration: 3000 });
      }
    });
  }
}

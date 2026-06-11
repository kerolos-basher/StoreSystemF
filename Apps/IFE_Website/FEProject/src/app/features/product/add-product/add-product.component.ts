import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, Observable, switchMap } from 'rxjs';
import { Category, ProductDetailsAutoComplete, Supplier } from '../../../shared/models/inventory.models';
import { displayProductWithSupplier } from '../../../shared/utils/product-autocomplete-display';
import { AddProductService } from './add-product.service';

@Component({
  selector: 'app-add-product',
  standalone: true,
  providers: [AddProductService],
  imports: [
    AsyncPipe, DecimalPipe, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatAutocompleteModule, MatSnackBarModule
  ],
  templateUrl: './add-product.component.html',
  styleUrl: './add-product.component.scss'
})
export class AddProductComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AddProductService);
  private readonly snackBar = inject(MatSnackBar);

  readonly categories = signal<Category[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly saving = signal(false);
  readonly productId = signal<number | null>(null);
  readonly previewBarcode = signal('');
  filteredProducts$!: Observable<ProductDetailsAutoComplete[]>;
  filteredSuppliers$!: Observable<Supplier[]>;

  readonly form = this.fb.group({
    productName: ['', Validators.required],
    existingProductId: [null as number | null],
    categoryId: [''],
    purchasePrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
    suggestedSellingPrice: [null as number | null, [Validators.min(0.01)]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    supplierName: [''],
    purchaseDate: [new Date(), Validators.required],
    notes: ['']
  });

  ngOnInit(): void {
    this.service.getCategories().subscribe(c => this.categories.set(c));
    this.service.searchSuppliers().subscribe(s => this.suppliers.set(s));

    this.filteredProducts$ = this.form.controls.productName.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter((v): v is string => typeof v === 'string' && v.trim().length >= 2),
      switchMap(term => this.service.searchAutocomplete(term.trim()))
    );

    this.filteredSuppliers$ = this.form.controls.supplierName.valueChanges.pipe(
      debounceTime(200),
      switchMap(term => this.service.searchSuppliers((term ?? '').toString()))
    );
  }

  selectProduct(item: ProductDetailsAutoComplete): void {
    this.productId.set(item.productId);
    this.form.patchValue({ productName: item.productName, existingProductId: item.productId });
  }

  displayProduct = displayProductWithSupplier;

  onProductInput(): void {
    if (!this.productId()) return;
    this.productId.set(null);
    this.form.controls.existingProductId.setValue(null);
  }

  onSuggestedPriceBlur(): void {
    const price = this.form.controls.purchasePrice.value;
    const suggested = this.form.controls.suggestedSellingPrice.value ?? price;
    if (price) this.previewBarcode.set(`BC-preview-${price}-${suggested}`);
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const raw = this.form.getRawValue();
    this.service.createPurchase({
      productName: raw.productName ?? '',
      existingProductId: raw.existingProductId,
      categoryId: raw.categoryId || null,
      purchasePrice: Number(raw.purchasePrice),
      sellingPrice: Number(raw.suggestedSellingPrice ?? raw.purchasePrice),
      quantity: Number(raw.quantity),
      supplierName: raw.supplierName,
      purchaseDate: raw.purchaseDate,
      notes: raw.notes
    }).subscribe({
      next: (r) => {
        this.saving.set(false);
        this.previewBarcode.set(r.barcode);
        this.snackBar.open(`تم الحفظ — باركود: ${r.barcode}`, 'إغلاق', { duration: 4000 });
        this.form.patchValue({ quantity: 1, notes: '', purchasePrice: null, suggestedSellingPrice: null });
        this.productId.set(null);
      },
      error: (e) => {
        this.saving.set(false);
        this.snackBar.open(e?.error?.message ?? 'فشل الحفظ', 'إغلاق', { duration: 3500 });
      }
    });
  }
}

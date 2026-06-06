import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, Observable, startWith, switchMap } from 'rxjs';
import { Category, ProductNameLookup, Supplier } from '../../../shared/models/inventory.models';
import { ProductEntryPageService } from './product-entry-page.service';

@Component({
  selector: 'app-product-entry-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProductEntryPageService],
  imports: [
    AsyncPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatAutocompleteModule,
    MatSnackBarModule
  ],
  templateUrl: './product-entry-page.component.html',
  styleUrl: './product-entry-page.component.scss'
})
export class ProductEntryPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly pageService = inject(ProductEntryPageService);
  private readonly snackBar = inject(MatSnackBar);

  readonly categories = signal<Category[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly saving = signal(false);
  readonly selectedProduct = signal<ProductNameLookup | null>(null);
  filteredSuppliers$!: Observable<Supplier[]>;
  filteredProducts$!: Observable<ProductNameLookup[]>;

  readonly form = this.fb.group({
    productName: ['', Validators.required],
    existingProductId: [null as number | null],
    categoryId: [''],
    purchasePrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
    sellingPrice: [null as number | null, [Validators.min(0)]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    supplierId: [''],
    supplierName: [''],
    purchaseDate: [new Date(), Validators.required],
    notes: ['']
  });

  ngOnInit(): void {
    this.pageService.getCategories().subscribe((items) => this.categories.set(items));
    this.refreshSuppliers();

    this.filteredSuppliers$ = this.form.controls.supplierName.valueChanges.pipe(
      startWith(''),
      debounceTime(200),
      switchMap((term) => this.pageService.searchSuppliers(term ?? ''))
    );

    this.filteredProducts$ = this.form.controls.productName.valueChanges.pipe(
      startWith(''),
      debounceTime(250),
      switchMap((term) => this.pageService.searchProductNames((term ?? '').trim()))
    );
  }

  refreshSuppliers(): void {
    this.pageService.searchSuppliers().subscribe((items) => this.suppliers.set(items));
  }

  selectProduct(product: ProductNameLookup): void {
    this.selectedProduct.set(product);
    this.form.patchValue({
      productName: product.productName,
      existingProductId: product.id
    });
  }

  onProductNameInput(): void {
    const name = (this.form.controls.productName.value ?? '').trim();
    const selected = this.selectedProduct();
    if (selected && selected.productName !== name) {
      this.selectedProduct.set(null);
      this.form.controls.existingProductId.setValue(null);
    }
  }

  async ensureSupplier(): Promise<string | null> {
    const supplierId = this.form.controls.supplierId.value;
    if (supplierId) return supplierId;

    const name = (this.form.controls.supplierName.value ?? '').trim();
    if (!name) return null;

    const existing = this.suppliers().find(s => s.name.toLowerCase() === name.toLowerCase());
    if (existing) {
      this.form.controls.supplierId.setValue(existing.id);
      return existing.id;
    }

    const created = await new Promise<Supplier>((resolve, reject) => {
      this.pageService.createSupplier(name).subscribe({ next: resolve, error: reject });
    });

    this.refreshSuppliers();
    this.form.controls.supplierId.setValue(created.id);
    this.form.controls.supplierName.setValue(created.name);
    return created.id;
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    try {
      await this.ensureSupplier();
      const raw = this.form.getRawValue();
      const selectedSupplier = this.suppliers().find(s => s.id === raw.supplierId);

      this.pageService.createPurchaseEntry({
        productName: raw.productName ?? '',
        existingProductId: raw.existingProductId,
        categoryId: raw.categoryId || null,
        purchasePrice: Number(raw.purchasePrice),
        sellingPrice: Number(raw.sellingPrice ?? 0),
        quantity: Number(raw.quantity),
        supplierName: selectedSupplier?.name ?? raw.supplierName,
        purchaseDate: raw.purchaseDate,
        notes: raw.notes
      }).subscribe({
        next: (result) => {
          this.saving.set(false);
          this.snackBar.open(`تم الحفظ — باركود الدفعة: ${result.barcode}`, 'إغلاق', { duration: 4000 });
          this.form.patchValue({ quantity: 1, notes: '', purchasePrice: null, sellingPrice: null });
        },
        error: (err) => {
          this.saving.set(false);
          this.snackBar.open(err?.error?.message ?? 'فشل الحفظ', 'إغلاق', { duration: 3500 });
        }
      });
    } catch {
      this.saving.set(false);
      this.snackBar.open('فشل إنشاء المورد', 'إغلاق', { duration: 3000 });
    }
  }

  selectSupplier(supplier: Supplier): void {
    this.form.patchValue({ supplierId: supplier.id, supplierName: supplier.name });
  }
}

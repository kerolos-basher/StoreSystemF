import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { debounceTime } from 'rxjs';
import { ProductListItem } from '../../../shared/models/inventory.models';
import { InventoryListService } from './inventory-list.service';
import { ProductDetailsModalComponent } from '../product-details-modal/product-details-modal.component';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  providers: [InventoryListService],
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatMenuModule,
    MatSnackBarModule,
    CurrencyPipe,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './inventory-list.component.html',
  styleUrl: './inventory-list.component.scss'
})
export class InventoryListComponent implements OnInit {
  private readonly service = inject(InventoryListService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly categories = signal<{ id: string; name: string }[]>([]);
  readonly suppliers = signal<{ id: string; name: string }[]>([]);
  readonly products = signal<ProductListItem[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly statistics = signal({ totalProducts: 0, totalQuantity: 0, inventoryValue: 0 });
  readonly selectedRow = signal<ProductListItem | null>(null);

  pageSize = 10;
  pageNumber = 1;
  sortBy = 'lastPurchaseDate';
  sortDirection: 'asc' | 'desc' = 'desc';

  readonly filtersForm = this.fb.group({
    productName: [''],
    barcode: [''],
    supplierId: [''],
    categoryId: [''],
    purchasePriceFrom: [''],
    purchasePriceTo: [''],
    quantityFrom: [''],
    quantityTo: ['']
  });

  ngOnInit(): void {
    this.service.getCategories().subscribe(items => this.categories.set(items));
    this.service.searchSuppliers().subscribe(items => this.suppliers.set(items));
    this.loadStatistics();

    this.filtersForm.valueChanges.pipe(debounceTime(300)).subscribe(() => {
      this.pageNumber = 1;
      this.search();
    });

    this.search();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  loadStatistics(): void {
    this.service.getStatistics().subscribe(stats =>
      this.statistics.set({
        totalProducts: stats.totalProducts,
        totalQuantity: stats.totalQuantity,
        inventoryValue: stats.inventoryValue
      })
    );
  }

  search(): void {
    this.loading.set(true);
    const raw = this.filtersForm.getRawValue();
    const payload: Record<string, string | number | boolean> = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    for (const [key, value] of Object.entries(raw)) {
      if (value !== null && value !== undefined && `${value}` !== '') {
        payload[key] = value as string | number;
      }
    }

    this.service.searchProducts(payload).subscribe({
      next: result => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
    this.loadStatistics();
  }

  resetFilters(): void {
    this.filtersForm.reset({
      productName: '', barcode: '', supplierId: '', categoryId: '',
      purchasePriceFrom: '', purchasePriceTo: '', quantityFrom: '', quantityTo: ''
    });
    this.pageNumber = 1;
    this.search();
  }

  toggleSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'asc';
    }
    this.search();
  }

  sortIcon(column: string): string {
    if (this.sortBy !== column) return '↕';
    return this.sortDirection === 'asc' ? '↑' : '↓';
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

  selectRow(row: ProductListItem, event: Event): void {
    event.stopPropagation();
    this.selectedRow.set(row);
  }

  openDetails(row: ProductListItem): void {
    this.service.getProductDetails(row.id).subscribe({
      next: details => {
        const ref = this.dialog.open(ProductDetailsModalComponent, {
          width: '1100px',
          maxWidth: '96vw',
          maxHeight: '92vh',
          autoFocus: false,
          data: { details }
        });
        ref.afterClosed().subscribe(changed => {
          if (changed) {
            this.search();
            this.loadStatistics();
          }
        });
      },
      error: err => {
        this.snackBar.open(err?.error?.message ?? 'فشل تحميل التفاصيل', 'إغلاق', { duration: 3500 });
      }
    });
  }

  onMenuDetails(): void {
    const row = this.selectedRow();
    if (row) this.openDetails(row);
  }
}

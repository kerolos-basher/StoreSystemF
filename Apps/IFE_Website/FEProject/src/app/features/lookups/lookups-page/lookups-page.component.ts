import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Category, ReturnReason, Supplier } from '../../../shared/models/inventory.models';
import { LookupTab } from './lookups-page.interface';
import { LookupsPageService } from './lookups-page.service';

@Component({
  selector: 'app-lookups-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [LookupsPageService],
  imports: [FormsModule, MatSnackBarModule],
  templateUrl: './lookups-page.component.html',
  styleUrl: './lookups-page.component.scss'
})
export class LookupsPageComponent implements OnInit {
  private readonly pageService = inject(LookupsPageService);
  private readonly snackBar = inject(MatSnackBar);

  readonly activeTab = signal<LookupTab>('categories');
  readonly categories = signal<Category[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly returnReasons = signal<ReturnReason[]>([]);
  readonly editingId = signal<string | number | null>(null);
  readonly saving = signal(false);

  name = '';
  isReturnToStock = true;

  ngOnInit(): void {
    this.reload();
  }

  setTab(tab: LookupTab): void {
    this.activeTab.set(tab);
    this.cancelEdit();
  }

  reload(): void {
    this.pageService.getCategories().subscribe(items => this.categories.set(items));
    this.pageService.searchSuppliers().subscribe(items => this.suppliers.set(items));
    this.pageService.getReturnReasons().subscribe(items => this.returnReasons.set(items));
  }

  startEdit(id: string | number, name: string, isReturnToStock = true): void {
    this.editingId.set(id);
    this.name = name;
    this.isReturnToStock = isReturnToStock;
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.name = '';
    this.isReturnToStock = true;
  }

  save(): void {
    const trimmed = this.name.trim();
    if (!trimmed) return;

    this.saving.set(true);
    const tab = this.activeTab();
    const editingId = this.editingId();

    const done = () => {
      this.saving.set(false);
      this.cancelEdit();
      this.reload();
      this.snackBar.open('تم الحفظ', 'إغلاق', { duration: 2500 });
    };

    const fail = (err: { error?: { message?: string } }) => {
      this.saving.set(false);
      this.snackBar.open(err?.error?.message ?? 'فشل الحفظ', 'إغلاق', { duration: 3500 });
    };

    if (tab === 'categories') {
      if (editingId) {
        this.pageService.updateCategory(`${editingId}`, trimmed).subscribe({ next: done, error: fail });
      } else {
        this.pageService.createCategory(trimmed).subscribe({ next: done, error: fail });
      }
      return;
    }

    if (tab === 'suppliers') {
      if (editingId) {
        this.pageService.updateSupplier(`${editingId}`, trimmed).subscribe({ next: done, error: fail });
      } else {
        this.pageService.createSupplier(trimmed).subscribe({ next: done, error: fail });
      }
      return;
    }

    if (editingId) {
      this.pageService.updateReturnReason(Number(editingId), trimmed, this.isReturnToStock).subscribe({ next: done, error: fail });
    } else {
      this.pageService.createReturnReason(trimmed, this.isReturnToStock).subscribe({ next: done, error: fail });
    }
  }

  remove(id: string | number): void {
    if (!confirm('هل أنت متأكد من الحذف؟')) return;

    const tab = this.activeTab();
    const fail = (err: { error?: { message?: string } }) =>
      this.snackBar.open(err?.error?.message ?? 'فشل الحذف', 'إغلاق', { duration: 3500 });

    if (tab === 'categories') {
      this.pageService.deleteCategory(`${id}`).subscribe({
        next: () => { this.reload(); this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 }); },
        error: fail
      });
      return;
    }

    if (tab === 'suppliers') {
      this.pageService.deleteSupplier(`${id}`).subscribe({
        next: () => { this.reload(); this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 }); },
        error: fail
      });
      return;
    }

    this.pageService.deleteReturnReason(Number(id)).subscribe({
      next: () => { this.reload(); this.snackBar.open('تم الحذف', 'إغلاق', { duration: 2500 }); },
      error: fail
    });
  }
}

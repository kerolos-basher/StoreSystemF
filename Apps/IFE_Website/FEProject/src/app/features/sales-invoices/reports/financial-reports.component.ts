import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { DecimalPipe } from '@angular/common';
import { FinancialReport } from '../../../shared/models/inventory.models';
import { formatLocalDate } from '../../../shared/utils/date-format';
import { FinancialReportsService } from './financial-reports.service';

@Component({
  selector: 'app-financial-reports',
  standalone: true,
  providers: [FinancialReportsService],
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    DecimalPipe
  ],
  templateUrl: './financial-reports.component.html',
  styleUrl: './financial-reports.component.scss'
})
export class FinancialReportsComponent implements OnInit {
  private readonly service = inject(FinancialReportsService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly report = signal<FinancialReport | null>(null);

  readonly filtersForm = this.fb.group({
    dateFrom: [null as Date | null],
    dateTo: [null as Date | null]
  });

  ngOnInit(): void {
    this.applyPreset('month');
  }

  applyPreset(preset: 'day' | 'month' | 'year'): void {
    const now = new Date();
    const end = new Date(now);
    let start = new Date(now);

    if (preset === 'day') {
      start.setHours(0, 0, 0, 0);
    } else if (preset === 'month') {
      start = new Date(now.getFullYear(), now.getMonth(), 1);
    } else {
      start = new Date(now.getFullYear(), 0, 1);
    }

    this.filtersForm.patchValue({ dateFrom: start, dateTo: end });
    this.loadReport();
  }

  loadReport(): void {
    this.loading.set(true);
    const raw = this.filtersForm.getRawValue();
    const from = raw.dateFrom ? formatLocalDate(raw.dateFrom) : undefined;
    const to = raw.dateTo ? formatLocalDate(raw.dateTo) : undefined;

    this.service.getReport(from, to).subscribe({
      next: r => {
        this.report.set(r);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  resetFilters(): void {
    this.filtersForm.reset({ dateFrom: null, dateTo: null });
    this.loadReport();
  }
}

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryApiService } from '../../../core/services/inventory-api.service';
import { FinancialReport } from '../../../shared/models/inventory.models';

@Injectable()
export class FinancialReportsService {
  private readonly api = inject(InventoryApiService);

  getReport(from?: string, to?: string): Observable<FinancialReport> {
    return this.api.getFinancialReport(from, to);
  }
}

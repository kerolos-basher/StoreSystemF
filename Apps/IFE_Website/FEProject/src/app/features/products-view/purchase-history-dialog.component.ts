import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { PurchaseHistoryItem } from '../../shared/models/inventory.models';

@Component({
  selector: 'app-purchase-history-dialog',
  imports: [MatDialogModule, MatTableModule, DatePipe, CurrencyPipe],
  templateUrl: './purchase-history-dialog.component.html'
})
export class PurchaseHistoryDialogComponent {
  readonly columns = ['purchasePrice', 'quantity', 'supplier', 'purchaseDate', 'notes'];
  constructor(@Inject(MAT_DIALOG_DATA) public data: { history: PurchaseHistoryItem[] }) {}
}

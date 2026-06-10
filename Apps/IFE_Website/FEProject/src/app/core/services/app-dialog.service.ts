import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.interfaces';

@Injectable({ providedIn: 'root' })
export class AppDialogService {
  private readonly dialog = inject(MatDialog);

  confirm(data: ConfirmDialogData): Observable<boolean> {
    return this.dialog.open(ConfirmDialogComponent, {
      width: '440px',
      maxWidth: '92vw',
      panelClass: 'app-dialog',
      autoFocus: false,
      restoreFocus: true,
      data
    }).afterClosed().pipe(map(result => !!result));
  }
}

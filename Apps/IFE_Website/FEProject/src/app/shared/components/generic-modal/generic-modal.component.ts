import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-generic-modal',
  standalone: true,
  imports: [MatDialogModule],
  templateUrl: './generic-modal.component.html'
})
export class GenericModalComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { title: string; content: string }) {}
}

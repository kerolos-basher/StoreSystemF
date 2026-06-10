import { Component, Input } from '@angular/core';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-generic-table',
  standalone: true,
  imports: [MatTableModule],
  templateUrl: './generic-table.component.html'
})
export class GenericTableComponent<T extends Record<string, unknown>> {
  @Input({ required: true }) data: T[] = [];
  @Input({ required: true }) columns: string[] = [];
}

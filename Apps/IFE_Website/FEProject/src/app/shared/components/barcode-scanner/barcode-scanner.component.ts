import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Output,
  ViewChild,
  signal
} from '@angular/core';

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  templateUrl: './barcode-scanner.component.html',
  styleUrl: './barcode-scanner.component.scss'
})
export class BarcodeScannerComponent implements AfterViewInit {
  @Output() scanned = new EventEmitter<string>();
  @ViewChild('scannerInput') scannerInput?: ElementRef<HTMLInputElement>;

  readonly hint = signal('جاهز للمسح — وجّه الماسح الضوئي نحو الحقل');

  private scanBuffer = '';
  private lastKeyTime = 0;
  private readonly scanGapMs = 80;

  ngAfterViewInit(): void {
    this.focusInput();
  }

  focusInput(): void {
    setTimeout(() => this.scannerInput?.nativeElement.focus(), 0);
  }

  onInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value.trim();
    if (!value) return;
    this.emitScan(value);
  }

  onKeyDown(event: KeyboardEvent): void {
    const now = Date.now();

    if (event.key === 'Enter') {
      event.preventDefault();
      const inputValue = this.scannerInput?.nativeElement.value.trim() ?? '';
      const code = inputValue || this.scanBuffer.trim();
      if (code) {
        this.emitScan(code);
      }
      return;
    }

    if (event.key.length === 1) {
      if (now - this.lastKeyTime > this.scanGapMs) {
        this.scanBuffer = '';
      }
      this.scanBuffer += event.key;
      this.lastKeyTime = now;
    }
  }

  private emitScan(code: string): void {
    this.scanned.emit(code);
    this.scanBuffer = '';
    if (this.scannerInput) {
      this.scannerInput.nativeElement.value = '';
      this.focusInput();
    }
  }
}

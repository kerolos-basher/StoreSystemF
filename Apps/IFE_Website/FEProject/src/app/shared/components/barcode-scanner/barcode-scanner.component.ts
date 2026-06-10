import { Component, AfterViewInit, EventEmitter, OnDestroy, Output, signal } from '@angular/core';
import { Html5Qrcode } from 'html5-qrcode';

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  templateUrl: './barcode-scanner.component.html',
  styleUrl: './barcode-scanner.component.scss'
})
export class BarcodeScannerComponent implements AfterViewInit, OnDestroy {
  @Output() scanned = new EventEmitter<string>();

  readonly error = signal('');
  private scanner?: Html5Qrcode;
  private started = false;

  async ngAfterViewInit(): Promise<void> {
    try {
      this.scanner = new Html5Qrcode('barcode-scanner-region');
      await this.scanner.start(
        { facingMode: 'environment' },
        { fps: 10, qrbox: { width: 250, height: 250 } },
        (decoded) => this.scanned.emit(decoded),
        () => undefined
      );
      this.started = true;
    } catch {
      this.error.set('تعذر فتح الكاميرا. تأكد من منح الإذن.');
    }
  }

  async ngOnDestroy(): Promise<void> {
    if (this.scanner && this.started) {
      await this.scanner.stop().catch(() => undefined);
      this.scanner.clear();
    }
  }
}

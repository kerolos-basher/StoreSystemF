import { Component, EventEmitter, OnDestroy, Output, signal } from '@angular/core';
import { Html5Qrcode } from 'html5-qrcode';

let scannerInstanceId = 0;

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  templateUrl: './barcode-scanner.component.html',
  styleUrl: './barcode-scanner.component.scss'
})
export class BarcodeScannerComponent implements OnDestroy {
  @Output() scanned = new EventEmitter<string>();

  readonly error = signal('');
  readonly active = signal(false);
  readonly starting = signal(false);

  readonly regionId = `barcode-scanner-region-${++scannerInstanceId}`;

  private scanner?: Html5Qrcode;
  private started = false;

  async startCamera(): Promise<void> {
    if (this.starting() || this.active()) return;

    this.starting.set(true);
    this.error.set('');
    this.active.set(true);

    await new Promise<void>(resolve => setTimeout(resolve, 0));

    try {
      this.scanner = new Html5Qrcode(this.regionId);
      await this.scanner.start(
        { facingMode: 'environment' },
        { fps: 10, qrbox: { width: 250, height: 250 } },
        decoded => this.scanned.emit(decoded),
        () => undefined
      );
      this.started = true;
    } catch {
      this.error.set('تعذر فتح الكاميرا. تأكد من منح الإذن.');
      this.active.set(false);
    } finally {
      this.starting.set(false);
    }
  }

  async stopCamera(): Promise<void> {
    if (this.scanner && this.started) {
      await this.scanner.stop().catch(() => undefined);
      this.scanner.clear();
      this.scanner = undefined;
      this.started = false;
    }
    this.active.set(false);
  }

  async ngOnDestroy(): Promise<void> {
    await this.stopCamera();
  }
}

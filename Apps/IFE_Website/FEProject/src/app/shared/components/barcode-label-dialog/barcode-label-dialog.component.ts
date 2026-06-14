import { AfterViewInit, Component, ElementRef, Inject, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import JsBarcode from 'jsbarcode';
import jsPDF from 'jspdf';
import { BarcodeLabelData } from '../../models/inventory.models';

@Component({
  selector: 'app-barcode-label-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './barcode-label-dialog.component.html',
  styleUrl: './barcode-label-dialog.component.scss'
})
export class BarcodeLabelDialogComponent implements AfterViewInit {
  @ViewChild('barcodeCanvas') barcodeCanvas?: ElementRef<HTMLCanvasElement>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { productName: string; label: BarcodeLabelData },
    private readonly dialogRef: MatDialogRef<BarcodeLabelDialogComponent>
  ) {}

  get imageSrc(): string {
    return `data:${this.data.label.contentType};base64,${this.data.label.base64Image}`;
  }

  ngAfterViewInit(): void {
    if (!this.barcodeCanvas) return;

    JsBarcode(this.barcodeCanvas.nativeElement, this.data.label.barcode, {
      format: 'CODE128',
      displayValue: true,
      fontSize: 16,
      height: 80,
      margin: 12,
      background: '#ffffff',
      lineColor: '#000000'
    });
  }

  downloadImage(): void {
    const link = document.createElement('a');
    link.href = this.canvasDataUrl();
    link.download = `barcode-${this.data.label.barcode}.png`;
    link.click();
  }

  exportPdf(): void {
    const pdf = new jsPDF();
    pdf.setFont('helvetica');
    pdf.text(this.data.productName, 20, 20);
    pdf.text(`Barcode: ${this.data.label.barcode}`, 20, 30);
    pdf.addImage(this.canvasDataUrl(), 'PNG', 20, 40, 170, 60);
    pdf.save(`barcode-${this.data.label.barcode}.pdf`);
  }

  print(): void {
    const image = this.canvasDataUrl();
    const win = window.open('', '_blank');
    if (!win) return;

    win.document.write(`
      <html dir="rtl"><head><title>Barcode</title></head><body style="text-align:center;font-family:Tahoma,sans-serif">
      <h2>${this.data.productName}</h2>
      <img src="${image}" style="width:320px" />
      <p style="font-family:monospace">${this.data.label.barcode}</p>
      </body></html>`);
    win.document.close();
    win.focus();
    win.print();
  }

  close(): void {
    this.dialogRef.close();
  }

  private canvasDataUrl(): string {
    if (this.barcodeCanvas) {
      return this.barcodeCanvas.nativeElement.toDataURL('image/png');
    }

    return this.imageSrc;
  }
}

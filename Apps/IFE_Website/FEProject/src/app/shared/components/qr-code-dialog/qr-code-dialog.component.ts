import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { QRCodeData } from '../../models/inventory.models';
import jsPDF from 'jspdf';

@Component({
  selector: 'app-qr-code-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './qr-code-dialog.component.html',
  styleUrl: './qr-code-dialog.component.scss'
})
export class QrCodeDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { productName: string; qr: QRCodeData },
    private readonly dialogRef: MatDialogRef<QrCodeDialogComponent>
  ) {}

  get imageSrc(): string {
    return `data:${this.data.qr.contentType};base64,${this.data.qr.base64Image}`;
  }

  downloadImage(): void {
    const link = document.createElement('a');
    link.href = this.imageSrc;
    link.download = `qr-${this.data.qr.barcode}.png`;
    link.click();
  }

  exportPdf(): void {
    const pdf = new jsPDF();
    pdf.setFont('helvetica');
    pdf.text(this.data.productName, 20, 20);
    pdf.text(`Barcode: ${this.data.qr.barcode}`, 20, 30);
    pdf.addImage(this.imageSrc, 'PNG', 55, 40, 100, 100);
    pdf.save(`qr-${this.data.qr.barcode}.pdf`);
  }

  print(): void {
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write(`
      <html dir="rtl"><head><title>QR</title></head><body style="text-align:center;font-family:Cairo,sans-serif">
      <h2>${this.data.productName}</h2>
      <img src="${this.imageSrc}" style="width:280px;height:280px" />
      <p>${this.data.qr.barcode}</p>
      </body></html>`);
    win.document.close();
    win.focus();
    win.print();
  }

  close(): void {
    this.dialogRef.close();
  }
}

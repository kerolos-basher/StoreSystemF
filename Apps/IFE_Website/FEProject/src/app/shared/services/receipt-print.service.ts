import { Injectable } from '@angular/core';

export interface PriceLabelData {
  productName: string;
  barcode: string;
  suggestedPrice: number;
  purchasePrice?: number;
  supplierName?: string;
}

export interface SaleReceiptData {
  invoiceNumber: string;
  items: { productName: string; quantity: number; unitPrice: number }[];
  grandTotal: number;
  amountPaid?: number;
  remainingAmount?: number;
  isDeferred?: boolean;
  customerName?: string;
}

@Injectable({ providedIn: 'root' })
export class ReceiptPrintService {
  printPriceLabel(data: PriceLabelData): void {
    const html = `
      <!DOCTYPE html>
      <html dir="rtl" lang="ar">
      <head>
        <meta charset="utf-8" />
        <title>بطاقة سعر</title>
        <style>
          @page { size: 58mm auto; margin: 2mm; }
          body { font-family: Tahoma, Arial, sans-serif; font-size: 12px; margin: 0; padding: 4px; }
          .name { font-size: 14px; font-weight: bold; margin-bottom: 6px; }
          .price { font-size: 18px; font-weight: bold; margin: 8px 0; }
          .barcode { font-family: monospace; letter-spacing: 1px; margin-top: 6px; }
          .muted { color: #555; font-size: 10px; }
        </style>
      </head>
      <body>
        <div class="name">${this.escape(data.productName)}</div>
        <div class="price">${data.suggestedPrice.toFixed(2)} ج.م</div>
        ${data.supplierName ? `<div class="muted">${this.escape(data.supplierName)}</div>` : ''}
        <div class="barcode">${this.escape(data.barcode)}</div>
      </body>
      </html>`;
    this.printHtml(html);
  }

  printSaleReceipt(data: SaleReceiptData): void {
    const rows = data.items.map(item => `
      <tr>
        <td>${this.escape(item.productName)}</td>
        <td>${item.quantity}</td>
        <td>${item.unitPrice.toFixed(2)}</td>
        <td>${(item.quantity * item.unitPrice).toFixed(2)}</td>
      </tr>`).join('');

    const html = `
      <!DOCTYPE html>
      <html dir="rtl" lang="ar">
      <head>
        <meta charset="utf-8" />
        <title>فاتورة ${this.escape(data.invoiceNumber)}</title>
        <style>
          @page { size: 80mm auto; margin: 2mm; }
          body { font-family: Tahoma, Arial, sans-serif; font-size: 11px; margin: 0; padding: 4px; }
          h1 { font-size: 14px; margin: 0 0 8px; text-align: center; }
          table { width: 100%; border-collapse: collapse; }
          th, td { padding: 2px 0; text-align: right; vertical-align: top; }
          th { border-bottom: 1px dashed #000; font-size: 10px; }
          .total { font-weight: bold; font-size: 13px; margin-top: 8px; }
          .muted { color: #555; font-size: 10px; }
        </style>
      </head>
      <body>
        <h1>فاتورة مبيعات</h1>
        <div class="muted">${this.escape(data.invoiceNumber)}</div>
        ${data.customerName ? `<div class="muted">العميل: ${this.escape(data.customerName)}</div>` : ''}
        <table>
          <thead>
            <tr><th>الصنف</th><th>كم</th><th>سعر</th><th>إجمالي</th></tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
        <div class="total">الإجمالي: ${data.grandTotal.toFixed(2)} ج.م</div>
        ${data.isDeferred ? `
          <div class="muted">مدفوع: ${(data.amountPaid ?? 0).toFixed(2)} ج.م</div>
          <div class="muted">متبقي: ${(data.remainingAmount ?? 0).toFixed(2)} ج.م</div>` : ''}
      </body>
      </html>`;
    this.printHtml(html);
  }

  private printHtml(html: string): void {
    const printWindow = window.open('', '_blank', 'width=360,height=640');
    if (!printWindow) return;

    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.focus();

    printWindow.onload = () => {
      printWindow.print();
      printWindow.close();
    };

    setTimeout(() => {
      if (!printWindow.closed) {
        printWindow.print();
        printWindow.close();
      }
    }, 300);
  }

  private escape(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}

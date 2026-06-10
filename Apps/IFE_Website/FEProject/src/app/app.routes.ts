import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'entry' },
  {
    path: 'entry',
    loadComponent: () => import('./features/purchase/add-purchase.component').then(m => m.AddPurchaseComponent)
  },
  {
    path: 'products',
    loadComponent: () => import('./features/inventory/inventory-list/inventory-list.component').then(m => m.InventoryListComponent)
  },
  {
    path: 'price-scanner',
    loadComponent: () => import('./features/price-check/price-check.component').then(m => m.PriceCheckComponent)
  },
  {
    path: 'sales',
    loadComponent: () => import('./features/sales/sales.component').then(m => m.SalesComponent)
  },
  {
    path: 'sales-invoices',
    loadComponent: () => import('./features/sales-invoices/sales-invoices-list/sales-invoices-list.component').then(m => m.SalesInvoicesListComponent)
  },
  {
    path: 'deferred-payments',
    loadComponent: () => import('./features/sales-invoices/deferred-payments/deferred-payments.component').then(m => m.DeferredPaymentsComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./features/sales-invoices/reports/financial-reports.component').then(m => m.FinancialReportsComponent)
  },
  {
    path: 'returns',
    loadComponent: () => import('./features/returns/returns.component').then(m => m.ReturnsComponent)
  },
  {
    path: 'lookups',
    loadComponent: () => import('./features/lookups/lookups-page/lookups-page.component').then(m => m.LookupsPageComponent)
  }
];

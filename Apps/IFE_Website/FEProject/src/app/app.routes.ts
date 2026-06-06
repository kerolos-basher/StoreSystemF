import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'entry' },
  {
    path: 'entry',
    loadComponent: () => import('./features/product-entry/product-entry-page/product-entry-page.component').then(m => m.ProductEntryPageComponent)
  },
  {
    path: 'products',
    loadComponent: () => import('./features/products-view/products-view-page.component').then(m => m.ProductsViewPageComponent)
  },
  {
    path: 'price-scanner',
    loadComponent: () => import('./features/price-scanner/price-scanner-page.component').then(m => m.PriceScannerPageComponent)
  },
  {
    path: 'sales',
    loadComponent: () => import('./features/sales/sales-page.component').then(m => m.SalesPageComponent)
  },
  {
    path: 'sales-invoices',
    loadComponent: () => import('./features/sales-invoices/sales-invoices-page.component').then(m => m.SalesInvoicesPageComponent)
  },
  {
    path: 'returns',
    loadComponent: () => import('./features/returns/returns-page/returns-page.component').then(m => m.ReturnsPageComponent)
  },
  {
    path: 'lookups',
    loadComponent: () => import('./features/lookups/lookups-page/lookups-page.component').then(m => m.LookupsPageComponent)
  }
];

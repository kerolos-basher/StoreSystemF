import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LoadingService } from './core/services/loading.service';

export interface NavItem {
  path: string;
  label: string;
  icon: string;
  section?: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatProgressBarModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  readonly sidebarOpen = signal(false);
  readonly sidebarCollapsed = signal(false);

  readonly navItems: NavItem[] = [
    { path: '/entry', label: 'إضافة وشراء', icon: '➕', section: 'المخزون' },
    { path: '/products', label: 'المخزون', icon: '📦', section: 'المخزون' },
    { path: '/price-scanner', label: 'فحص السعر', icon: '🏷️', section: 'المخزون' },
    { path: '/sales', label: 'المبيعات', icon: '🛒', section: 'المبيعات' },
    { path: '/sales-invoices', label: 'فواتير المبيعات', icon: '🧾', section: 'المبيعات' },
    { path: '/returns', label: 'المرتجعات', icon: '↩️', section: 'المبيعات' },
    { path: '/deferred-payments', label: 'الدفع الآجل', icon: '💳', section: 'المالية' },
    { path: '/reports', label: 'التقارير المالية', icon: '📊', section: 'المالية' },
    { path: '/lookups', label: 'القوائم المرجعية', icon: '⚙️', section: 'الإعدادات' }
  ];

  constructor(public loading: LoadingService) {}

  toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  toggleSidebarCollapse(): void {
    this.sidebarCollapsed.update(v => !v);
  }
}

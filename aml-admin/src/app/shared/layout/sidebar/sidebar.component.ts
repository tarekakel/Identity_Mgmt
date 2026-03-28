import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';

const SIDEBAR_COLLAPSED_KEY = 'sidebarCollapsed';

type NavItem = {
  labelKey: string;
  route: string;
  icon?: string;
  exact?: boolean;
  queryParams?: Record<string, string>;
};

type NavGroup = {
  id: string;
  labelKey: string;
  icon?: string;
  items: NavItem[];
};

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  collapsed = signal(false);
  openGroupId = signal<string | null>(null);

  readonly navGroups: NavGroup[] = [
    {
      id: 'main',
      labelKey: 'nav.main',
      icon: '📊',
      items: [
        { labelKey: 'app.dashboard', route: '/dashboard', icon: '📊', exact: true },
        { labelKey: 'app.customers', route: '/customers', icon: '👤' },
        { labelKey: 'app.sanctionsScreening', route: '/sanctions-screening', icon: '🔍' },
        { labelKey: 'app.sanctionLists', route: '/sanction-lists', icon: '📑' },
        { labelKey: 'app.riskAssignment', route: '/risk-assignment', icon: '⚠' },
        { labelKey: 'app.cases', route: '/cases', icon: '📄' },
        { labelKey: 'app.auditLogs', route: '/audit-logs', icon: '📋' },
        { labelKey: 'app.sanctionActionHistory', route: '/sanction-action-history', icon: '📜' }
      ]
    },
    {
      id: 'screening',
      labelKey: 'nav.screening',
      icon: '🧪',
      items: [
        { labelKey: 'screeningMenu.individual', route: '/screening/individual', icon: '👤' },
        { labelKey: 'screeningMenu.corporate', route: '/screening/corporate', icon: '🏢' },
        {
          labelKey: 'screeningMenu.individualBulkUpload',
          route: '/screening/individual-bulk-upload',
          queryParams: { kind: 'ind' },
          icon: '⬆'
        },
        {
          labelKey: 'screeningMenu.corporateBulkUpload',
          route: '/screening/corporate-bulk-upload',
          queryParams: { kind: 'cor' },
          icon: '⬆'
        },
        { labelKey: 'screeningMenu.instantSanctionScreening', route: '/screening/instant-sanction-screening', icon: '🔍' },
        { labelKey: 'screeningMenu.internalWatchlist', route: '/screening/internal-watchlist', icon: '🧾' }
      ]
    },
    {
      id: 'kyc',
      labelKey: 'nav.kyc',
      icon: '🪪',
      items: [{ labelKey: 'nav.kyc', route: '/kyc', icon: '🪪', exact: true }]
    },
    {
      id: 'caseManagement',
      labelKey: 'nav.caseManagement',
      icon: '🗂',
      items: [{ labelKey: 'nav.caseManagement', route: '/case-management', icon: '🗂', exact: true }]
    },
    {
      id: 'riskAssessment',
      labelKey: 'nav.riskAssessment',
      icon: '⚠',
      items: [{ labelKey: 'nav.riskAssessment', route: '/risk-assessment', icon: '⚠', exact: true }]
    },
    {
      id: 'ewra',
      labelKey: 'nav.ewra',
      icon: '🧮',
      items: [{ labelKey: 'nav.ewra', route: '/ewra', icon: '🧮', exact: true }]
    },
    {
      id: 'reports',
      labelKey: 'nav.reports',
      icon: '📈',
      items: [
        { labelKey: 'nav.reports', route: '/reports', icon: '📈', exact: true },
        { labelKey: 'nav.customerBulkUploadLogs', route: '/reports/customer-bulk-upload-logs', icon: '📋' }
      ]
    },
    {
      id: 'masters',
      labelKey: 'nav.masters',
      icon: '🛠',
      items: [
        { labelKey: 'nav.mastersOverview', route: '/masters', icon: '📋', exact: true },
        { labelKey: 'masters.segments.countries', route: '/masters/countries', icon: '🌍' },
        { labelKey: 'masters.segments.nationalities', route: '/masters/nationalities', icon: '🏳' },
        { labelKey: 'masters.segments.genders', route: '/masters/genders', icon: '⚥' },
        { labelKey: 'masters.segments.customerTypes', route: '/masters/customer-types', icon: '👥' },
        { labelKey: 'masters.segments.customerStatuses', route: '/masters/customer-statuses', icon: '📌' },
        { labelKey: 'masters.segments.documentTypes', route: '/masters/document-types', icon: '📄' },
        { labelKey: 'masters.segments.occupations', route: '/masters/occupations', icon: '💼' },
        { labelKey: 'masters.segments.sourceOfFunds', route: '/masters/source-of-funds', icon: '💰' }
      ]
    },
    {
      id: 'support',
      labelKey: 'nav.support',
      icon: '💬',
      items: [{ labelKey: 'nav.support', route: '/support', icon: '💬', exact: true }]
    }
  ];

  ngOnInit(): void {
    try {
      const stored = localStorage.getItem(SIDEBAR_COLLAPSED_KEY);
      if (stored !== null) {
        this.collapsed.set(stored === 'true');
      }
    } catch {
      // ignore localStorage errors
    }

    this.openGroupForUrl(this.router.url);
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((e) => {
        this.openGroupForUrl(e.urlAfterRedirects);
      });
  }

  toggleCollapsed(): void {
    this.collapsed.update(c => !c);
    try {
      localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(this.collapsed()));
    } catch {
      // ignore
    }
  }

  toggleGroup(groupId: string): void {
    if (this.collapsed()) return;
    this.openGroupId.update((current) => (current === groupId ? null : groupId));
  }

  isGroupOpen(groupId: string): boolean {
    return this.openGroupId() === groupId;
  }

  private openGroupForUrl(url: string): void {
    const groupId = this.findGroupIdByUrl(url);
    if (groupId) this.openGroupId.set(groupId);
  }

  private findGroupIdByUrl(url: string): string | null {
    for (const group of this.navGroups) {
      for (const item of group.items) {
        const match = item.exact ? url === item.route : url.startsWith(item.route);
        if (match) return group.id;
      }
    }
    return null;
  }
}

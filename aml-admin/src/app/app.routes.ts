import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './shared/layout/main-layout/main-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'screening/new',
        loadComponent: () => import('./features/screening/screening-stepper/screening-stepper.component').then((m) => m.ScreeningStepperComponent)
      },
      {
        path: 'screening/individual',
        loadComponent: () =>
          import('./features/screening/individual-screening/individual-screening.component').then(
            (m) => m.IndividualScreeningComponent
          )
      },
      {
        path: 'screening/corporate',
        loadComponent: () =>
          import('./features/screening/corporate-screening/corporate-screening.component').then((m) => m.CorporateScreeningComponent)
      },
      {
        path: 'screening/individual-bulk-upload',
        data: { bulkKind: 'ind' },
        loadComponent: () =>
          import('./features/screening/bulk-upload/bulk-upload.component').then((m) => m.BulkUploadComponent)
      },
      {
        path: 'screening/corporate-bulk-upload',
        data: { bulkKind: 'cor' },
        loadComponent: () =>
          import('./features/screening/bulk-upload/bulk-upload.component').then((m) => m.BulkUploadComponent)
      },
      {
        path: 'screening/instant-sanction-screening',
        loadComponent: () =>
          import('./features/screening/instant-sanction-screening/instant-sanction-screening.component').then(
            (m) => m.InstantSanctionScreeningComponent
          )
      },
      {
        path: 'screening/internal-watchlist',
        loadComponent: () =>
          import('./features/screening/internal-watchlist/internal-watchlist.component').then((m) => m.InternalWatchlistComponent)
      },
      {
        path: 'kyc',
        loadComponent: () => import('./features/kyc/kyc/kyc.component').then((m) => m.KycComponent)
      },
      {
        path: 'kyc/individual/:customerId',
        loadComponent: () => import('./features/kyc/individual-kyc/individual-kyc.component').then((m) => m.IndividualKycComponent)
      },
      {
        path: 'case-management',
        loadComponent: () =>
          import('./features/case-management/case-management/case-management.component').then((m) => m.CaseManagementComponent)
      },
      {
        path: 'risk-assessment',
        loadComponent: () =>
          import('./features/risk-assessment/risk-assessment/risk-assessment.component').then((m) => m.RiskAssessmentComponent)
      },
      {
        path: 'ewra',
        loadComponent: () => import('./features/ewra/ewra/ewra.component').then((m) => m.EwraComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports/reports.component').then((m) => m.ReportsComponent)
      },
      {
        path: 'reports/customer-bulk-upload-logs',
        loadComponent: () =>
          import('./features/reports/customer-bulk-upload-logs/customer-bulk-upload-logs.component').then(
            (m) => m.CustomerBulkUploadLogsComponent
          )
      },
      {
        path: 'masters/:segment/new',
        loadComponent: () =>
          import('./features/masters/master-lookup-form/master-lookup-form.component').then((m) => m.MasterLookupFormComponent)
      },
      {
        path: 'masters/:segment/:id/edit',
        loadComponent: () =>
          import('./features/masters/master-lookup-form/master-lookup-form.component').then((m) => m.MasterLookupFormComponent)
      },
      {
        path: 'masters/:segment',
        loadComponent: () =>
          import('./features/masters/master-lookup-list/master-lookup-list.component').then((m) => m.MasterLookupListComponent)
      },
      {
        path: 'masters',
        loadComponent: () => import('./features/masters/masters/masters.component').then((m) => m.MastersComponent)
      },
      {
        path: 'support',
        loadComponent: () => import('./features/support/support/support.component').then((m) => m.SupportComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customers-list/customers-list.component').then((m) => m.CustomersListComponent)
      },
      {
        path: 'customers/new',
        loadComponent: () => import('./features/customers/customer-form/customer-form.component').then((m) => m.CustomerFormComponent)
      },
      {
        path: 'customers/:id/edit',
        loadComponent: () => import('./features/customers/customer-form/customer-form.component').then((m) => m.CustomerFormComponent)
      },
      {
        path: 'sanctions-screening',
        loadComponent: () => import('./features/sanctions-screening/sanctions-screening-list/sanctions-screening-list.component').then((m) => m.SanctionsScreeningListComponent)
      },
      {
        path: 'sanctions-screening/new',
        loadComponent: () => import('./features/sanctions-screening/sanctions-screening-form/sanctions-screening-form.component').then((m) => m.SanctionsScreeningFormComponent)
      },
      {
        path: 'sanctions-screening/:id/edit',
        loadComponent: () => import('./features/sanctions-screening/sanctions-screening-form/sanctions-screening-form.component').then((m) => m.SanctionsScreeningFormComponent)
      },
      {
        path: 'sanction-lists',
        loadComponent: () => import('./features/sanction-lists/sanction-list-upload/sanction-list-upload.component').then((m) => m.SanctionListUploadComponent)
      },
      {
        path: 'risk-assignment',
        loadComponent: () => import('./features/risk-assignment/risk-assignment-list/risk-assignment-list.component').then((m) => m.RiskAssignmentListComponent)
      },
      {
        path: 'risk-assignment/new',
        loadComponent: () => import('./features/risk-assignment/risk-assignment-form/risk-assignment-form.component').then((m) => m.RiskAssignmentFormComponent)
      },
      {
        path: 'risk-assignment/:id/edit',
        loadComponent: () => import('./features/risk-assignment/risk-assignment-form/risk-assignment-form.component').then((m) => m.RiskAssignmentFormComponent)
      },
      {
        path: 'cases',
        loadComponent: () => import('./features/cases/cases-list/cases-list.component').then((m) => m.CasesListComponent)
      },
      {
        path: 'cases/new',
        loadComponent: () => import('./features/cases/case-form/case-form.component').then((m) => m.CaseFormComponent)
      },
      {
        path: 'cases/:id/edit',
        loadComponent: () => import('./features/cases/case-form/case-form.component').then((m) => m.CaseFormComponent)
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./features/audit-logs/audit-logs-list/audit-logs-list.component').then((m) => m.AuditLogsListComponent)
      },
      {
        path: 'sanction-action-history',
        loadComponent: () => import('./features/sanction-action-history/sanction-action-history-list.component').then((m) => m.SanctionActionHistoryListComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

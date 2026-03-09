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
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

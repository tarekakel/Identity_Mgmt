import { Routes } from '@angular/router';

export const permissionsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./permissions-list/permissions-list.component').then((m) => m.PermissionsListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./permission-form/permission-form.component').then((m) => m.PermissionFormComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./permission-form/permission-form.component').then((m) => m.PermissionFormComponent)
  }
];

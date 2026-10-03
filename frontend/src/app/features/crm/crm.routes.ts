import { Routes } from '@angular/router';

export const crmRoutes: Routes = [
  {
    path: '',
    redirectTo: 'customers',
    pathMatch: 'full'
  },
  {
    path: 'customers',
    loadComponent: () => import('./components/customer-list/customer-list.component').then(m => m.CustomerListComponent)
  },
  {
    path: 'leads',
    loadComponent: () => import('./components/lead-list/lead-list.component').then(m => m.LeadListComponent)
  },
  {
    path: 'opportunities',
    loadComponent: () => import('./components/opportunity-list/opportunity-list.component').then(m => m.OpportunityListComponent)
  },
  {
    path: 'activities',
    loadComponent: () => import('./components/activity-list/activity-list.component').then(m => m.ActivityListComponent)
  }
];

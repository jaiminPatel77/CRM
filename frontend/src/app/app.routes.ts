import { Routes } from '@angular/router';
import { authGuard } from './models/route-gaurds/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./features/auth/components/login/login.component').then(m => m.LoginComponent)
    },
    {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(r => r.authRoutes)
    },
    {
        path: '',
        loadComponent: () => import('./features/master/components/home/home.component').then(m => m.HomeComponent),
        children: [
            {
                path: 'dashboard',
                data: { preload: true },
                canActivate: [authGuard],
                loadComponent: () => import('./features/master/components/dashboard/dashboard.component').then(m => m.DashboardComponent)
            },
            {
                path: 'profile',
                data: { preload: true },
                canActivate: [authGuard],
                loadComponent: () => import('./features/master/components/profile/profile.component').then(m => m.ProfileComponent)
            },
            {
                path: 'admin',
                canActivate: [authGuard],
                loadChildren: () => import('./features/admin/admin.routes').then(r => r.adminRoutes)
            },
            {
                path: 'crm',
                canActivate: [authGuard],
                loadChildren: () => import('./features/crm/crm.routes').then(r => r.crmRoutes)
            }
        ]
    },
    { path: '**', redirectTo: 'auth/login' }
];


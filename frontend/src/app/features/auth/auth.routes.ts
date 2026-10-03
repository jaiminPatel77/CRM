import { Routes } from "@angular/router";
import { HomeComponent } from "./components/home/home.component";

export const authRoutes: Routes = [
  {
    path: '',
    component: HomeComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent),
      },
      {
        path: 'login',
        loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent),
      },
      {
        path: 'forgot-password',
        loadComponent: () => import('./components/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
      },
      {
        path: 'forgot-password-confirm',
        loadComponent: () => import('./components/forgot-password-confirm/forgot-password-confirm.component').then(m => m.ForgotPasswordConfirmComponent),
      },
      {
        path: 'set-password',
        loadComponent: () => import('./components/set-password/set-password.component').then(m => m.SetPasswordComponent),
      }
    ]
  }
]
import { Routes } from '@angular/router';
import { authorizeGuard } from './guards/authorize.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent) },
  { path: 'verify-otp', loadComponent: () => import('./components/verify-otp/verify-otp.component').then(m => m.VerifyOtpComponent) },
  { path: 'forgot-password', loadComponent: () => import('./components/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
  { path: 'reset-password', loadComponent: () => import('./components/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
  { path: 'new-password', loadComponent: () => import('./components/new-password/new-password.component').then(m => m.NewPasswordComponent) },
  { path: 'dashboard', loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [authorizeGuard] },
  { path: '**', redirectTo: 'login' }
];

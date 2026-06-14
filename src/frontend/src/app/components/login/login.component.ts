import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-center">
      <div class="card">
        <div class="card-title">Welcome back 👋</div>
        <div class="card-sub">Sign in to your TodoAuth account</div>

        <div *ngIf="errorMsg" class="alert alert-error">{{ errorMsg }}</div>
        <div *ngIf="needsVerification" class="alert alert-info">
          Please verify your email first.
          <button class="btn-ghost" (click)="goToVerify()">Verify now →</button>
        </div>

        <form [formGroup]="form" (ngSubmit)="onLogin()">
          <div class="field">
            <label>Email Address</label>
            <input type="email" formControlName="email" placeholder="user@example.com"
              [class.error]="f['email'].invalid && f['email'].touched" />
            <div class="field-error" *ngIf="f['email'].invalid && f['email'].touched">Valid email required</div>
          </div>

          <div class="field">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••••"
              [class.error]="f['password'].invalid && f['password'].touched" />
            <div class="field-error" *ngIf="f['password'].invalid && f['password'].touched">Min 6 characters</div>
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="isLoading">
            {{ isLoading ? 'Signing in…' : 'Sign In' }}
          </button>
        </form>

        <button class="link" (click)="goToForgotPassword()">Forgot password?</button>
        <hr class="divider" />
        <button class="link" (click)="goToRegister()">Don't have an account? Register →</button>
      </div>
    </div>
  `
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  isLoading = false;
  errorMsg = '';
  needsVerification = false;
  private pendingEmail = '';

  get f() { return this.form.controls; }

  onLogin() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true;
    this.errorMsg = '';
    this.needsVerification = false;

    const { email, password } = this.form.value;
    this.authService.login(email!, password!).subscribe({
      next: () => { this.isLoading = false; this.router.navigate(['/dashboard']); },
      error: err => {
        this.isLoading = false;
        const msg = err.error?.message || 'Login failed.';
        if (err.error?.requiresVerification) {
          this.needsVerification = true;
          this.pendingEmail = email!;
        } else {
          this.errorMsg = msg;
        }
      }
    });
  }

  goToForgotPassword() { this.router.navigate(['/forgot-password']); }
  goToRegister() { this.router.navigate(['/register']); }
  goToVerify() { this.router.navigate(['/verify-otp'], { queryParams: { email: this.pendingEmail } }); }
}

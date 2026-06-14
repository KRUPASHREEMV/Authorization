import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-center">
      <div class="card">
        <div class="card-title">Reset Password</div>
        <div class="card-sub">Enter your email — we'll send a reset OTP</div>

        <div *ngIf="errorMsg" class="alert alert-error">{{ errorMsg }}</div>
        <div *ngIf="successMsg" class="alert alert-success">{{ successMsg }}</div>

        <form [formGroup]="form" (ngSubmit)="onRequestReset()">
          <div class="field">
            <label>Email Address</label>
            <input type="email" formControlName="email" placeholder="john@example.com"
              [class.error]="f['email'].invalid && f['email'].touched" />
            <div class="field-error" *ngIf="f['email'].invalid && f['email'].touched">Valid email required</div>
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="isLoading">
            {{ isLoading ? 'Sending OTP…' : 'Send Reset OTP' }}
          </button>
        </form>

        <hr class="divider" />
        <button class="link" (click)="goToLogin()">← Back to Login</button>
      </div>
    </div>
  `
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  isLoading = false;
  errorMsg = '';
  successMsg = '';

  get f() { return this.form.controls; }

  onRequestReset() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true;
    this.errorMsg = '';

    const email = this.form.value.email!;
    this.authService.requestPasswordReset(email).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMsg = 'OTP sent to your email!';
        setTimeout(() => this.router.navigate(['/reset-password'], { queryParams: { email } }), 1000);
      },
      error: err => {
        this.isLoading = false;
        // Generic message — don't reveal whether email exists
        if (err.status === 429) {
          this.errorMsg = 'Too many requests. Please wait 1 hour.';
        } else {
          this.successMsg = 'If an account with this email exists, an OTP has been sent.';
          setTimeout(() => this.router.navigate(['/reset-password'], { queryParams: { email } }), 1500);
        }
      }
    });
  }

  goToLogin() { this.router.navigate(['/login']); }
}

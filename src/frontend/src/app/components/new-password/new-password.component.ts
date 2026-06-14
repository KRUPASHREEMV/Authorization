import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const pw = control.get('newPassword')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw && cpw && pw !== cpw ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-new-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-center">
      <div class="card">
        <div class="card-title">Set New Password</div>
        <div class="card-sub">OTP verified ✓ — choose your new password</div>

        <div class="step-badges">
          <div class="step-badge done">① Email sent ✓</div>
          <div class="step-badge done">② OTP verified ✓</div>
          <div class="step-badge active">③ New Password ←</div>
        </div>

        <div *ngIf="errorMsg" class="alert alert-error">{{ errorMsg }}</div>
        <div *ngIf="successMsg" class="alert alert-success">{{ successMsg }}</div>

        <form [formGroup]="form" (ngSubmit)="onSaveNewPassword()">
          <div class="field">
            <label>New Password</label>
            <input type="password" formControlName="newPassword" placeholder="••••••••"
              [class.error]="f['newPassword'].invalid && f['newPassword'].touched" />
            <div class="field-error" *ngIf="f['newPassword'].invalid && f['newPassword'].touched">
              Password must be at least 8 characters
            </div>
          </div>

          <div class="field">
            <label>Confirm New Password</label>
            <input type="password" formControlName="confirmPassword" placeholder="••••••••"
              [class.error]="f['confirmPassword'].touched && form.hasError('passwordMismatch')" />
            <div class="field-error" *ngIf="f['confirmPassword'].touched && form.hasError('passwordMismatch')">
              Passwords do not match
            </div>
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="isLoading">
            {{ isLoading ? 'Saving…' : 'Save New Password' }}
          </button>
        </form>
      </div>
    </div>
  `
})
export class NewPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  form = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  email = '';
  isLoading = false;
  errorMsg = '';
  successMsg = '';

  get f() { return this.form.controls; }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'];
      // Safety: if email or resetToken missing, redirect back
      if (!this.email || !this.authService.getResetToken()) {
        this.router.navigate(['/forgot-password']);
      }
    });
  }

  onSaveNewPassword() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true;
    this.errorMsg = '';

    this.authService.resetPassword(this.email, this.form.value.newPassword!).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMsg = 'Password changed successfully!';
        setTimeout(() => this.router.navigate(['/login']), 1200);
      },
      error: err => {
        this.isLoading = false;
        if (err.error?.message?.includes('expired') || err.error?.message?.includes('Invalid')) {
          this.errorMsg = 'Reset token expired. Please restart the password reset flow.';
          setTimeout(() => this.router.navigate(['/forgot-password']), 2000);
        } else {
          this.errorMsg = err.error?.message || 'Failed to reset password.';
        }
      }
    });
  }
}

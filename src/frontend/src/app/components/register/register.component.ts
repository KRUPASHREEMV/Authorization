import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const pw = control.get('password')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw && cpw && pw !== cpw ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-center">
      <div class="card">
        <div class="card-title">Create Account</div>
        <div class="card-sub">Email OTP verification required after signup</div>

        <div *ngIf="errorMsg" class="alert alert-error">{{ errorMsg }}</div>

        <form [formGroup]="form" (ngSubmit)="onRegister()">
          <div class="field-row">
            <div class="field">
              <label>First Name</label>
              <input formControlName="firstName" placeholder="John"
                [class.error]="f['firstName'].invalid && f['firstName'].touched" />
              <div class="field-error" *ngIf="f['firstName'].invalid && f['firstName'].touched">Min 2 characters</div>
            </div>
            <div class="field">
              <label>Last Name</label>
              <input formControlName="lastName" placeholder="Doe"
                [class.error]="f['lastName'].invalid && f['lastName'].touched" />
              <div class="field-error" *ngIf="f['lastName'].invalid && f['lastName'].touched">Min 2 characters</div>
            </div>
          </div>

          <div class="field">
            <label>Email Address</label>
            <input type="email" formControlName="email" placeholder="john@example.com"
              [class.error]="f['email'].invalid && f['email'].touched" />
            <div class="field-error" *ngIf="f['email'].invalid && f['email'].touched">Invalid email format</div>
          </div>

          <div class="field">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••••"
              [class.error]="f['password'].invalid && f['password'].touched" />
            <div class="field-error" *ngIf="f['password'].invalid && f['password'].touched">Password must be at least 8 characters</div>
          </div>

          <div class="field">
            <label>Confirm Password</label>
            <input type="password" formControlName="confirmPassword" placeholder="••••••••"
              [class.error]="(f['confirmPassword'].touched && form.hasError('passwordMismatch'))" />
            <div class="field-error" *ngIf="f['confirmPassword'].touched && form.hasError('passwordMismatch')">
              Passwords do not match
            </div>
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="isLoading">
            {{ isLoading ? 'Creating account…' : 'Create Account' }}
          </button>
        </form>

        <hr class="divider" />
        <button class="link" (click)="goToLogin()">Already have an account? Login →</button>
      </div>
    </div>
  `
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  isLoading = false;
  errorMsg = '';

  get f() { return this.form.controls; }

  onRegister() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true;
    this.errorMsg = '';

    const { email, password, firstName, lastName } = this.form.value;
    this.authService.register({ email: email!, password: password!, firstName: firstName!, lastName: lastName! }).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/verify-otp'], { queryParams: { email } });
      },
      error: err => {
        this.isLoading = false;
        this.errorMsg = err.error?.message || 'Registration failed.';
      }
    });
  }

  goToLogin() { this.router.navigate(['/login']); }
}

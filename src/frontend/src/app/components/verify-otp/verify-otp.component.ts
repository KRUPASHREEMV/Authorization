import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-center">
      <div class="card">
        <div class="card-title">Verify Your Email</div>
        <div class="card-sub">Code sent to <span style="color:var(--accent)">{{ email }}</span></div>

        <div class="alert alert-info">📧 OTP expires in 10 minutes</div>
        <div *ngIf="errorMsg" class="alert alert-error">{{ errorMsg }}</div>
        <div *ngIf="successMsg" class="alert alert-success">{{ successMsg }}</div>

        <form [formGroup]="form" (ngSubmit)="onVerifyOTP()">
          <div class="field">
            <label>6-Digit OTP Code</label>
            <input class="otp-input" formControlName="otp" placeholder="______" maxlength="6"
              [class.error]="f['otp'].invalid && f['otp'].touched" />
            <div class="field-error" *ngIf="f['otp'].invalid && f['otp'].touched">Must be exactly 6 digits</div>
          </div>

          <button type="submit" class="btn btn-primary" [disabled]="isLoading">
            {{ isLoading ? 'Verifying…' : 'Verify OTP' }}
          </button>
        </form>

        <div class="resend-row">
          <button class="btn-ghost" (click)="onResendOTP()" [disabled]="cooldown > 0 || isResending">
            🔄 {{ cooldown > 0 ? 'Resend in ' + cooldown + 's' : (isResending ? 'Sending…' : 'Resend OTP') }}
          </button>
          <span class="resend-hint">Max 3/hour</span>
        </div>
      </div>
    </div>
  `
})
export class VerifyOtpComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  form = this.fb.group({
    otp: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6), Validators.pattern(/^\d{6}$/)]]
  });

  email = '';
  isLoading = false;
  isResending = false;
  errorMsg = '';
  successMsg = '';
  cooldown = 0;
  private cooldownTimer: any;

  get f() { return this.form.controls; }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'];
      if (!this.email) this.router.navigate(['/register']);
    });
  }

  onVerifyOTP() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true;
    this.errorMsg = '';

    this.authService.verifyOTP(this.email, this.form.value.otp!).subscribe({
      next: () => { this.isLoading = false; this.router.navigate(['/dashboard']); },
      error: err => {
        this.isLoading = false;
        this.errorMsg = err.error?.message || 'Invalid or expired OTP.';
        this.form.patchValue({ otp: '' });
      }
    });
  }

  onResendOTP() {
    if (this.cooldown > 0) return;
    this.isResending = true;
    this.errorMsg = '';

    this.authService.resendOTP(this.email, 'Registration').subscribe({
      next: () => {
        this.isResending = false;
        this.successMsg = 'New OTP sent to your inbox!';
        this.startCooldown();
      },
      error: err => {
        this.isResending = false;
        this.errorMsg = err.status === 429 ? 'Too many requests. Wait 1 hour.' : (err.error?.message || 'Failed to resend OTP.');
      }
    });
  }

  private startCooldown() {
    this.cooldown = 60;
    clearInterval(this.cooldownTimer);
    this.cooldownTimer = setInterval(() => {
      this.cooldown--;
      if (this.cooldown <= 0) { clearInterval(this.cooldownTimer); this.successMsg = ''; }
    }, 1000);
  }
}

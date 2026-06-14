import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of, map } from 'rxjs';
import { Router } from '@angular/router';

export interface AuthResponse {
  token: string;
  email: string;
  roles: string[];
  // No refreshToken field — arrives as HttpOnly Secure Cookie via Set-Cookie header
}

export interface RegistrationResponse {
  message: string;
  requiresVerification: boolean;
}

export interface PasswordResetResponse {
  message: string;
  requiresOTPVerification: boolean;
}

export interface ResendOTPResponse {
  message: string;
}

export interface VerifyResetOtpResponse {
  resetToken: string; // Stored in service state — NEVER placed in URL
  message: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = '/api/auth';

  // Access token: Angular MEMORY only (not localStorage, sessionStorage, or cookies)
  // Refresh token: HttpOnly Secure Cookie — browser manages it, XSS cannot read it
  private token: string | null = null;

  // resetToken stored in service state after Step ② — NEVER in URL
  private resetToken: string | null = null;

  private currentUserSignal = signal<{ email: string; name?: string; roles: string[] } | null>(null);
  currentUser = this.currentUserSignal.asReadonly();

  // APP_INITIALIZER: guards suspend until this becomes true
  private isInitializedSignal = signal<boolean>(false);
  isInitialized = this.isInitializedSignal.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  /**
   * Called ONCE by APP_INITIALIZER before any route guard renders.
   * Tries to restore session from the HttpOnly refresh cookie.
   * Sets isInitialized = true regardless of success/failure.
   */
  initializeAuth(): Promise<void> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/refresh`, {}, { withCredentials: true })
      .pipe(
        tap(response => {
          this.token = response.token;
          this.currentUserSignal.set({ email: response.email, roles: response.roles });
        }),
        catchError(() => {
          this.token = null;
          this.currentUserSignal.set(null);
          return of(null);
        }),
        map(() => void 0)
      )
      .toPromise()
      .finally(() => this.isInitializedSignal.set(true)) as Promise<void>;
  }

  register(userData: { email: string; password: string; firstName: string; lastName: string }): Observable<RegistrationResponse> {
    return this.http.post<RegistrationResponse>(`${this.apiUrl}/register`, userData);
  }

  // withCredentials: true — browser stores the HttpOnly refresh cookie from Set-Cookie response header
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, { email, password }, { withCredentials: true })
      .pipe(tap(response => {
        this.token = response.token;
        this.currentUserSignal.set({ email: response.email, roles: response.roles });
      }));
  }

  verifyOTP(email: string, otp: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/verify-otp`, { email, otp }, { withCredentials: true })
      .pipe(tap(response => {
        this.token = response.token;
        this.currentUserSignal.set({ email: response.email, roles: response.roles });
      }));
  }

  resendOTP(email: string, purpose: string): Observable<ResendOTPResponse> {
    return this.http.post<ResendOTPResponse>(`${this.apiUrl}/resend-otp`, { email, purpose });
  }

  // Step ① of password reset
  requestPasswordReset(email: string): Observable<PasswordResetResponse> {
    return this.http.post<PasswordResetResponse>(`${this.apiUrl}/request-password-reset`, { email });
  }

  // Step ②: Submit OTP → receive resetToken → store in service state (NOT in URL)
  verifyResetOtp(email: string, otp: string): Observable<VerifyResetOtpResponse> {
    return this.http
      .post<VerifyResetOtpResponse>(`${this.apiUrl}/verify-reset-otp`, { email, otp })
      .pipe(tap(response => (this.resetToken = response.resetToken)));
  }

  // Step ③: Submit new password using the resetToken from Step ②
  resetPassword(email: string, newPassword: string): Observable<PasswordResetResponse> {
    if (!this.resetToken) throw new Error('Reset token missing. Please restart the password reset flow.');
    return this.http
      .post<PasswordResetResponse>(`${this.apiUrl}/reset-password`, {
        email,
        resetToken: this.resetToken,
        newPassword
      })
      .pipe(tap(() => (this.resetToken = null)));
  }

  // Called by ErrorInterceptor on 401. Browser auto-sends HttpOnly cookie.
  refreshAccessToken(): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/refresh`, {}, { withCredentials: true })
      .pipe(tap(response => {
        this.token = response.token;
        this.currentUserSignal.set({ email: response.email, roles: response.roles });
      }));
  }

  logout(): void {
    const email = this.currentUserSignal()?.email;
    this.http.post(`${this.apiUrl}/logout`, { email }, { withCredentials: true }).subscribe();
    this.token = null;
    this.resetToken = null;
    this.currentUserSignal.set(null);
    this.router.navigate(['/login']);
  }

  storeToken(token: string): void { this.token = token; }
  getToken(): string | null { return this.token; }
  getResetToken(): string | null { return this.resetToken; }
  isLoggedIn(): boolean { return this.token !== null; }
  getCurrentUser() { return this.currentUserSignal(); }
}

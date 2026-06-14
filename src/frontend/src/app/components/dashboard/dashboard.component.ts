import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-center">
      <div class="card" style="max-width:480px">
        <div style="background:#3d1c0040;border:1px solid #5a2a00;border-radius:7px;padding:9px 13px;font-size:0.72rem;color:var(--orange);text-align:center;margin-bottom:18px;">
          🛡 Protected by AuthorizeGuard — JWT verified
        </div>

        <div style="display:flex;align-items:center;gap:14px;margin-bottom:18px;">
          <div style="width:48px;height:48px;border-radius:50%;background:linear-gradient(135deg,var(--accent),var(--accent2));display:flex;align-items:center;justify-content:center;font-size:1.2rem;font-weight:900;color:#fff;flex-shrink:0;">
            {{ initial }}
          </div>
          <div>
            <div style="font-weight:800;font-size:0.95rem;">{{ name }}</div>
            <div style="font-size:0.74rem;color:var(--muted2);">{{ email }}</div>
            <div style="margin-top:5px;display:flex;gap:6px;align-items:center;">
              <span *ngFor="let role of roles" style="font-size:0.62rem;font-weight:700;padding:2px 8px;border-radius:100px;background:#1e3a5f;color:var(--accent);border:1px solid #2a4f80;">{{ role }}</span>
              <span style="font-size:0.62rem;color:var(--green);">✓ Verified</span>
            </div>
          </div>
        </div>

        <div style="display:flex;gap:8px;margin-bottom:16px;">
          <div style="flex:1;background:var(--surface3);border:1px solid var(--border);border-radius:8px;padding:10px 8px;text-align:center;">
            <div style="font-size:1rem;font-weight:800;color:var(--accent);">✓</div>
            <div style="font-size:0.6rem;color:var(--muted2);margin-top:2px;">Email Verified</div>
          </div>
          <div style="flex:1;background:var(--surface3);border:1px solid var(--border);border-radius:8px;padding:10px 8px;text-align:center;">
            <div style="font-size:0.75rem;font-weight:800;color:var(--accent);">JWT</div>
            <div style="font-size:0.6rem;color:var(--muted2);margin-top:2px;">Active</div>
          </div>
          <div style="flex:1;background:var(--surface3);border:1px solid var(--border);border-radius:8px;padding:10px 8px;text-align:center;">
            <div style="font-size:1rem;font-weight:800;color:var(--accent);">6h</div>
            <div style="font-size:0.6rem;color:var(--muted2);margin-top:2px;">Token Life</div>
          </div>
        </div>

        <div class="alert alert-success">✓ Authenticated — JWT sent via Authorization header on every request</div>

        <button class="btn btn-danger" (click)="onLogout()">Sign Out</button>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  email = '';
  name = '';
  roles: string[] = [];
  initial = '?';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    const user = this.authService.getCurrentUser();
    if (!user) { this.router.navigate(['/login']); return; }
    this.email = user.email;
    this.name = user.name || user.email;
    this.roles = user.roles;
    this.initial = this.email[0]?.toUpperCase() ?? '?';
  }

  onLogout() {
    this.authService.logout();
  }
}

import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { ClaimService } from '../../services/claim.service';
import { Dashboard as DashboardModel } from '../../models/dashboard.model';
import { Claim } from '../../models/claim.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  private readonly claimService =
    inject(ClaimService);

  private readonly cdr =
    inject(ChangeDetectorRef);

  dashboard: DashboardModel | null = null;

  claims: Claim[] = [];

  loading = true;

  errorMessage = '';

  ngOnInit(): void {
    this.loadDashboard();
    this.loadClaims();
  }

  // -----------------------------------------------
  // LOAD DASHBOARD SUMMARY
  // -----------------------------------------------

  loadDashboard(): void {
    this.claimService
      .getDashboard()
      .subscribe({
        next: (data) => {
          console.log(
            'Dashboard received:',
            data
          );

          this.dashboard = data;

          this.cdr.detectChanges();
        },

        error: (error) => {
          console.error(
            'Dashboard API error:',
            error
          );

          this.errorMessage =
            'Unable to load dashboard data.';

          this.cdr.detectChanges();
        }
      });
  }

  // -----------------------------------------------
  // LOAD CLAIMS
  // -----------------------------------------------

  loadClaims(): void {
    this.claimService
      .getClaims()
      .subscribe({
        next: (data) => {
          console.log(
            'Claims received:',
            data
          );

          this.claims = data;

          this.loading = false;

          this.cdr.detectChanges();
        },

        error: (error) => {
          console.error(
            'Claims API error:',
            error
          );

          this.errorMessage =
            'Unable to load claims.';

          this.loading = false;

          this.cdr.detectChanges();
        }
      });
  }

  // -----------------------------------------------
  // REFRESH DASHBOARD
  // -----------------------------------------------

  refreshDashboard(): void {
    this.loading = true;

    this.errorMessage = '';

    this.loadDashboard();

    this.loadClaims();
  }
}
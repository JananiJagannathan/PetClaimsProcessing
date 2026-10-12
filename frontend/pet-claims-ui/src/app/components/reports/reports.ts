import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';

import {
  FormBuilder,
  ReactiveFormsModule
} from '@angular/forms';

import { RouterLink } from '@angular/router';

import { Claim } from '../../models/claim.model';

import {
  ClaimService,
  ClaimSearch,
  MonthlyReport
} from '../../services/claim.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './reports.html',
  styleUrl: './reports.css'
})
export class Reports implements OnInit {

  private readonly fb =
    inject(FormBuilder);

  private readonly claimService =
    inject(ClaimService);

  private readonly cdr =
    inject(ChangeDetectorRef);

  claims: Claim[] = [];

  monthlyReports: MonthlyReport[] = [];

  loading = true;

  errorMessage = '';

  readonly filterForm =
    this.fb.nonNullable.group({

      status: [''],

      fromDate: [''],

      toDate: [''],

      minAmount: [0],

      maxAmount: [0],

      policyNumber: [''],

      clinic: ['']
    });

  ngOnInit(): void {

    this.loadClaims();

    this.loadMonthlyReports();
  }

  // -----------------------------------------------
  // LOAD / FILTER CLAIMS
  // -----------------------------------------------

  loadClaims(): void {

    this.loading = true;

    this.errorMessage = '';

    const values =
      this.filterForm.getRawValue();

    const search: ClaimSearch = {};

    if (values.status) {
      search.status = values.status;
    }

    if (values.fromDate) {
      search.fromDate = values.fromDate;
    }

    if (values.toDate) {
      search.toDate = values.toDate;
    }

    if (values.minAmount > 0) {
      search.minAmount =
        values.minAmount;
    }

    if (values.maxAmount > 0) {
      search.maxAmount =
        values.maxAmount;
    }

    if (values.policyNumber.trim()) {
      search.policyNumber =
        values.policyNumber.trim();
    }

    if (values.clinic.trim()) {
      search.clinic =
        values.clinic.trim();
    }

    this.claimService
      .getClaims(search)
      .subscribe({

        next: (claims) => {

          this.claims = claims;

          this.loading = false;

          this.cdr.detectChanges();
        },

        error: (error) => {

          console.error(
            'Claim report error:',
            error
          );

          this.errorMessage =
            'Unable to load claim history.';

          this.loading = false;

          this.cdr.detectChanges();
        }

      });
  }

  // -----------------------------------------------
  // MONTHLY REPORT
  // -----------------------------------------------

  loadMonthlyReports(): void {

    this.claimService
      .getMonthlyReport()
      .subscribe({

        next: (reports) => {

          this.monthlyReports =
            reports;

          this.cdr.detectChanges();
        },

        error: (error) => {

          console.error(
            'Monthly report error:',
            error
          );

        }

      });
  }

  // -----------------------------------------------
  // CLEAR FILTERS
  // -----------------------------------------------

  clearFilters(): void {

    this.filterForm.reset({

      status: '',

      fromDate: '',

      toDate: '',

      minAmount: 0,

      maxAmount: 0,

      policyNumber: '',

      clinic: ''
    });

    this.loadClaims();
  }


  // -----------------------------------------------
  // REPORT CALCULATIONS
  // -----------------------------------------------

  get submittedCount(): number {

    return this.claims
      .filter(
        claim =>
          claim.status === 'Submitted'
      )
      .length;
  }


  get underReviewCount(): number {

    return this.claims
      .filter(
        claim =>
          claim.status === 'Under Review'
      )
      .length;
  }


  get approvedCount(): number {

    return this.claims
      .filter(
        claim =>
          claim.status === 'Approved'
      )
      .length;
  }


  get rejectedCount(): number {

    return this.claims
      .filter(
        claim =>
          claim.status === 'Rejected'
      )
      .length;
  }


  get averageClaimAmount(): number {

    if (this.claims.length === 0) {
      return 0;
    }

    const total =
      this.claims.reduce(
        (sum, claim) =>
          sum + claim.claimedAmount,
        0
      );

    return total /
      this.claims.length;
  }


  get highestValueClaims(): Claim[] {

    return [
      ...this.claims
    ]
      .sort(
        (a, b) =>
          b.claimedAmount -
          a.claimedAmount
      )
      .slice(0, 5);
  }
}
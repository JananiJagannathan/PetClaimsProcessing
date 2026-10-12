import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Claim } from '../models/claim.model';
import { Dashboard } from '../models/dashboard.model';


// --------------------------------------------------
// CREATE CLAIM REQUEST
// --------------------------------------------------

export interface CreateClaimRequest {
  policyNumber: string;
  treatmentDate: string;
  clinic: string;
  diagnosis: string;
  treatmentDescription: string;
  claimedAmount: number;
  invoiceNumber: string;
  documentName: string;
}


// --------------------------------------------------
// UPDATE CLAIM REQUEST
// --------------------------------------------------

export interface UpdateClaimRequest {
  treatmentDate: string;
  clinic: string;
  diagnosis: string;
  treatmentDescription: string;
  claimedAmount: number;
  invoiceNumber: string;
}


// --------------------------------------------------
// APPROVAL REQUEST
// --------------------------------------------------

export interface ApproveClaimRequest {
  requestedApprovedAmount: number | null;
  reason: string;
}


// --------------------------------------------------
// REJECTION REQUEST
// --------------------------------------------------

export interface RejectClaimRequest {
  reason: string;
}


// --------------------------------------------------
// MORE INFORMATION REQUEST
// --------------------------------------------------

export interface MoreInformationRequest {
  reason: string;
}


// --------------------------------------------------
// CLAIM SEARCH FILTER
// --------------------------------------------------

export interface ClaimSearch {
  status?: string;
  fromDate?: string;
  toDate?: string;
  minAmount?: number;
  maxAmount?: number;
  policyNumber?: string;
  clinic?: string;
}


// --------------------------------------------------
// MONTHLY REPORT
// --------------------------------------------------

export interface MonthlyReport {
  year: number;
  month: number;
  monthName: string;
  claimCount: number;
  totalClaimedAmount: number;
  totalApprovedAmount: number;
  averageClaimAmount: number;
}


// --------------------------------------------------
// CLAIM SERVICE
// --------------------------------------------------

@Injectable({
  providedIn: 'root'
})
export class ClaimService {

  private readonly http =
    inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5287/api/Claims';


  // ------------------------------------------------
  // DASHBOARD
  // ------------------------------------------------

  getDashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(
      `${this.apiUrl}/dashboard`
    );
  }


  // ------------------------------------------------
  // GET / SEARCH CLAIMS
  // ------------------------------------------------

  getClaims(
    search?: ClaimSearch
  ): Observable<Claim[]> {

    let params =
      new HttpParams();

    if (search?.status) {
      params =
        params.set(
          'status',
          search.status
        );
    }

    if (search?.fromDate) {
      params =
        params.set(
          'fromDate',
          search.fromDate
        );
    }

    if (search?.toDate) {
      params =
        params.set(
          'toDate',
          search.toDate
        );
    }

    if (
      search?.minAmount !== undefined
    ) {
      params =
        params.set(
          'minAmount',
          search.minAmount.toString()
        );
    }

    if (
      search?.maxAmount !== undefined
    ) {
      params =
        params.set(
          'maxAmount',
          search.maxAmount.toString()
        );
    }

    if (search?.policyNumber) {
      params =
        params.set(
          'policyNumber',
          search.policyNumber
        );
    }

    if (search?.clinic) {
      params =
        params.set(
          'clinic',
          search.clinic
        );
    }

    return this.http.get<Claim[]>(
      this.apiUrl,
      { params }
    );
  }


  // ------------------------------------------------
  // GET CLAIM DETAILS
  // ------------------------------------------------

  getClaim(
    claimNumber: string
  ): Observable<Claim> {

    return this.http.get<Claim>(
      `${this.apiUrl}/${claimNumber}`
    );
  }


  // ------------------------------------------------
  // CREATE CLAIM
  // ------------------------------------------------

  createClaim(
    request: CreateClaimRequest
  ): Observable<Claim> {

    return this.http.post<Claim>(
      this.apiUrl,
      request
    );
  }


  // ------------------------------------------------
  // UPDATE CLAIM
  // ------------------------------------------------

  updateClaim(
    claimNumber: string,
    request: UpdateClaimRequest
  ): Observable<Claim> {

    return this.http.put<Claim>(
      `${this.apiUrl}/${claimNumber}`,
      request
    );
  }


  // ------------------------------------------------
  // MOVE CLAIM TO REVIEW
  // ------------------------------------------------

  moveToReview(
    claimNumber: string
  ): Observable<Claim> {

    return this.http.post<Claim>(
      `${this.apiUrl}/${claimNumber}/review`,
      {}
    );
  }


  // ------------------------------------------------
  // APPROVE / PARTIALLY APPROVE
  // ------------------------------------------------

  approveClaim(
    claimNumber: string,
    request: ApproveClaimRequest
  ): Observable<Claim> {

    return this.http.post<Claim>(
      `${this.apiUrl}/${claimNumber}/approve`,
      request
    );
  }


  // ------------------------------------------------
  // REJECT CLAIM
  // ------------------------------------------------

  rejectClaim(
    claimNumber: string,
    request: RejectClaimRequest
  ): Observable<Claim> {

    return this.http.post<Claim>(
      `${this.apiUrl}/${claimNumber}/reject`,
      request
    );
  }


  // ------------------------------------------------
  // REQUEST MORE INFORMATION
  // ------------------------------------------------

  requestMoreInformation(
    claimNumber: string,
    request: MoreInformationRequest
  ): Observable<Claim> {

    return this.http.post<Claim>(
      `${this.apiUrl}/${claimNumber}/more-information`,
      request
    );
  }


  // ------------------------------------------------
  // MONTHLY REPORT
  // ------------------------------------------------

  getMonthlyReport():
    Observable<MonthlyReport[]> {

    return this.http.get<MonthlyReport[]>(
      `${this.apiUrl}/reports/monthly`
    );
  }
}
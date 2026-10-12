import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Policy } from '../models/policy.model';


@Injectable({
  providedIn: 'root'
})
export class PolicyService {

  private readonly http =
    inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5287/api/Policies';


  // ------------------------------------------------
  // GET POLICY BY POLICY NUMBER
  // ------------------------------------------------

  getPolicy(
    policyNumber: string
  ): Observable<Policy> {

    return this.http.get<Policy>(
      `${this.apiUrl}/${policyNumber}`
    );
  }


  // ------------------------------------------------
  // GET ALL POLICIES
  // ------------------------------------------------

  getPolicies(): Observable<Policy[]> {

    return this.http.get<Policy[]>(
      this.apiUrl
    );
  }
}
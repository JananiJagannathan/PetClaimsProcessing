import {
  ChangeDetectorRef,
  Component,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { RouterLink } from '@angular/router';

import { Policy } from '../../models/policy.model';
import { PolicyService } from '../../services/policy.service';

import {
  ClaimService,
  CreateClaimRequest
} from '../../services/claim.service';

import { Claim } from '../../models/claim.model';


@Component({
  selector: 'app-submit-claim',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './submit-claim.html',
  styleUrl: './submit-claim.css'
})
export class SubmitClaim {

  private readonly fb =
    inject(FormBuilder);

  private readonly policyService =
    inject(PolicyService);

  private readonly claimService =
    inject(ClaimService);

  private readonly cdr =
    inject(ChangeDetectorRef);


  // --------------------------------------------------
  // PAGE STATE
  // --------------------------------------------------

  policy: Policy | null = null;

  checkingPolicy = false;

  submitting = false;

  policyError = '';

  submitError = '';

  successClaim: Claim | null = null;


  // --------------------------------------------------
  // CLAIM FORM
  // --------------------------------------------------

  readonly claimForm =
    this.fb.nonNullable.group({

      // POL + exactly 3 digits
      // Example: POL001
      policyNumber: [
        '',
        [
          Validators.required,
          Validators.pattern(/^POL\d{3}$/)
        ]
      ],

      // Loaded automatically from policy
      petName: [
        {
          value: '',
          disabled: true
        }
      ],

      treatmentDate: [
        '',
        Validators.required
      ],

      clinic: [
        '',
        [
          Validators.required,
          Validators.minLength(2)
        ]
      ],

      diagnosis: [
        '',
        [
          Validators.required,
          Validators.minLength(2)
        ]
      ],

      treatmentDescription: [
        '',
        [
          Validators.required,
          Validators.minLength(3)
        ]
      ],

      claimedAmount: [
        0,
        [
          Validators.required,
          Validators.min(0.01)
        ]
      ],

      invoiceNumber: [
        '',
        Validators.required
      ],

      // Optional
      documentName: ['']
    });


  // --------------------------------------------------
  // SHORTCUT FOR FORM CONTROLS
  // --------------------------------------------------

  get f() {
    return this.claimForm.controls;
  }


  // --------------------------------------------------
  // CHECK POLICY ELIGIBILITY
  // --------------------------------------------------

  checkPolicy(): void {

    const policyNumber =
      this.f.policyNumber.value
        .trim();

    this.policy = null;

    this.policyError = '';

    this.submitError = '';

    this.successClaim = null;

    this.f.petName.setValue('');


    // Validate policy number before API call
    if (this.f.policyNumber.invalid) {

      this.f.policyNumber
        .markAsTouched();

      return;
    }


    if (!policyNumber) {

      this.f.policyNumber
        .markAsTouched();

      return;
    }


    this.checkingPolicy = true;


    this.policyService
      .getPolicy(policyNumber)
      .subscribe({

        next: (policy) => {

          this.policy =
            policy;

          this.checkingPolicy =
            false;

          this.f.petName.setValue(
            policy.petName
          );


          // Policy exists but is not active
          if (
            policy.status.toLowerCase()
            !== 'active'
          ) {

            this.policyError =
              'This policy is not active.';

          }


          this.cdr.detectChanges();
        },


        error: () => {

          this.checkingPolicy =
            false;

          this.policy =
            null;

          this.f.petName
            .setValue('');

          this.policyError =
            'Policy not found. Please check the policy number.';

          this.cdr.detectChanges();
        }

      });
  }


  // --------------------------------------------------
  // TREATMENT DATE VALIDATION
  // --------------------------------------------------

  isTreatmentDateOutsidePolicy(): boolean {

    // We cannot check the policy period
    // until the user checks a policy.
    if (!this.policy) {
      return false;
    }


    const treatmentDate =
      this.f.treatmentDate.value;


    // Required validator handles empty values.
    if (!treatmentDate) {
      return false;
    }


    const selectedDate =
      new Date(treatmentDate);

    const policyStart =
      new Date(
        this.policy.startDate
      );

    const policyEnd =
      new Date(
        this.policy.endDate
      );


    return (
      selectedDate < policyStart ||
      selectedDate > policyEnd
    );
  }


  // --------------------------------------------------
  // SUBMIT CLAIM
  // --------------------------------------------------

  submitClaim(): void {

    this.submitError = '';

    this.policyError = '';

    this.successClaim = null;


    // -----------------------------------------------
    // BASIC FORM VALIDATION
    // -----------------------------------------------

    if (this.claimForm.invalid) {

      this.claimForm
        .markAllAsTouched();

      return;
    }


    // -----------------------------------------------
    // POLICY MUST BE CHECKED FIRST
    // -----------------------------------------------

    if (!this.policy) {

      this.policyError =
        'Check policy eligibility before submitting.';

      return;
    }


    // -----------------------------------------------
    // POLICY MUST BE ACTIVE
    // -----------------------------------------------

    if (
      this.policy.status.toLowerCase()
      !== 'active'
    ) {

      this.policyError =
        'Only active policies can submit claims.';

      return;
    }


    // -----------------------------------------------
    // TREATMENT DATE MUST BE INSIDE POLICY PERIOD
    // -----------------------------------------------

    if (
      this.isTreatmentDateOutsidePolicy()
    ) {

      this.f.treatmentDate
        .markAsTouched();

      this.submitError =
        'Treatment date must be within the policy period.';

      return;
    }


    // -----------------------------------------------
    // CREATE REQUEST
    // -----------------------------------------------

    const raw =
      this.claimForm
        .getRawValue();


    const request:
      CreateClaimRequest = {

        policyNumber:
          raw.policyNumber.trim(),

        treatmentDate:
          raw.treatmentDate,

        clinic:
          raw.clinic.trim(),

        diagnosis:
          raw.diagnosis.trim(),

        treatmentDescription:
          raw.treatmentDescription.trim(),

        claimedAmount:
          Number(
            raw.claimedAmount
          ),

        invoiceNumber:
          raw.invoiceNumber.trim(),

        documentName:
          raw.documentName.trim()
      };


    // -----------------------------------------------
    // CALL BACKEND
    // -----------------------------------------------

    this.submitting = true;


    this.claimService
      .createClaim(request)
      .subscribe({

        next: (claim) => {

          this.submitting =
            false;

          this.successClaim =
            claim;

          this.submitError =
            '';

          this.policyError =
            '';

          this.cdr.detectChanges();
        },


        error: (error) => {

          console.error(
            'Claim submission error:',
            error
          );

          this.submitting =
            false;

          this.submitError =
            error?.error?.message
            ?? 'Unable to submit claim.';

          this.cdr.detectChanges();
        }

      });
  }
}
import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';

import { Claim } from '../../models/claim.model';

import {
  ClaimService
} from '../../services/claim.service';


@Component({
  selector: 'app-claim-review',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './claim-review.html',
  styleUrl: './claim-review.css'
})
export class ClaimReview implements OnInit {

  private readonly route =
    inject(ActivatedRoute);

  private readonly claimService =
    inject(ClaimService);

  private readonly fb =
    inject(FormBuilder);

  private readonly cdr =
    inject(ChangeDetectorRef);

  claim: Claim | null = null;

  claimNumber = '';

  loading = true;

  processing = false;

  errorMessage = '';

  successMessage = '';

  actionType:
    'approve' |
    'partial' |
    'reject' |
    'more-info' |
    null = null;


  // -----------------------------------------------
  // DECISION FORM
  // -----------------------------------------------

  readonly decisionForm =
    this.fb.nonNullable.group({

      approvedAmount: [
        0,
        [
          Validators.min(0.01)
        ]
      ],

      reason: [
        '',
        [
          Validators.required,
          Validators.minLength(3)
        ]
      ]

    });


  get f() {
    return this.decisionForm.controls;
  }


  // -----------------------------------------------
  // INITIAL LOAD
  // -----------------------------------------------

  ngOnInit(): void {

    this.claimNumber =
      this.route.snapshot
        .paramMap
        .get('claimNumber') ?? '';

    if (!this.claimNumber) {

      this.errorMessage =
        'Claim number is missing.';

      this.loading = false;

      return;
    }

    this.loadClaim();
  }


  // -----------------------------------------------
  // LOAD CLAIM DETAILS
  // -----------------------------------------------

  loadClaim(): void {

    this.loading = true;

    this.errorMessage = '';

    this.claimService
      .getClaim(this.claimNumber)
      .subscribe({

        next: (claim) => {

          this.claim = claim;

          this.loading = false;

          this.cdr.detectChanges();
        },

        error: (error) => {

          console.error(
            'Claim details error:',
            error
          );

          this.errorMessage =
            error?.error?.message
            ?? 'Unable to load claim details.';

          this.loading = false;

          this.cdr.detectChanges();
        }

      });
  }


  // -----------------------------------------------
  // START REVIEW
  // -----------------------------------------------

  startReview(): void {

    if (!this.claim) {
      return;
    }

    this.processing = true;

    this.clearMessages();

    this.claimService
      .moveToReview(
        this.claim.claimNumber
      )
      .subscribe({

        next: (claim) => {

          this.claim = claim;

          this.processing = false;

          this.successMessage =
            'Claim moved to Under Review successfully.';

          this.cdr.detectChanges();
        },

        error: (error) => {

          this.handleError(error);

        }

      });
  }


  // -----------------------------------------------
  // SHOW APPROVE FORM
  // -----------------------------------------------

  showApprove(): void {

    this.clearMessages();

    this.actionType = 'approve';

    this.decisionForm.reset({
      approvedAmount: 0,
      reason: ''
    });
  }


  // -----------------------------------------------
  // SHOW PARTIAL APPROVAL FORM
  // -----------------------------------------------

  showPartialApproval(): void {

    this.clearMessages();

    this.actionType = 'partial';

    this.decisionForm.reset({
      approvedAmount: 0,
      reason: ''
    });
  }


  // -----------------------------------------------
  // SHOW REJECT FORM
  // -----------------------------------------------

  showReject(): void {

    this.clearMessages();

    this.actionType = 'reject';

    this.decisionForm.reset({
      approvedAmount: 0,
      reason: ''
    });
  }


  // -----------------------------------------------
  // SHOW MORE INFORMATION FORM
  // -----------------------------------------------

  showMoreInformation(): void {

    this.clearMessages();

    this.actionType = 'more-info';

    this.decisionForm.reset({
      approvedAmount: 0,
      reason: ''
    });
  }


  // -----------------------------------------------
  // CANCEL ACTION
  // -----------------------------------------------

  cancelAction(): void {

    this.actionType = null;

    this.decisionForm.reset({
      approvedAmount: 0,
      reason: ''
    });

    this.clearMessages();
  }


  // -----------------------------------------------
  // APPROVE FULL AMOUNT
  // -----------------------------------------------

  confirmApprove(): void {

    if (!this.claim) {
      return;
    }

    const reason =
      this.f.reason.value.trim();

    if (!reason) {

      this.f.reason.markAsTouched();

      return;
    }

    this.processing = true;

    this.clearMessages();

    this.claimService
      .approveClaim(
        this.claim.claimNumber,
        {
          requestedApprovedAmount: null,
          reason: reason
        }
      )
      .subscribe({

        next: (claim) => {

          this.actionCompleted(
            claim,
            'Claim approved successfully.'
          );

        },

        error: (error) => {

          this.handleError(error);

        }

      });
  }


  // -----------------------------------------------
  // PARTIALLY APPROVE
  // -----------------------------------------------

  confirmPartialApproval(): void {

    if (!this.claim) {
      return;
    }

    const amount =
      Number(
        this.f.approvedAmount.value
      );

    const reason =
      this.f.reason.value.trim();


    if (amount <= 0) {

      this.f.approvedAmount
        .markAsTouched();

      return;
    }


    if (!reason) {

      this.f.reason
        .markAsTouched();

      return;
    }


    this.processing = true;

    this.clearMessages();


    this.claimService
      .approveClaim(
        this.claim.claimNumber,
        {
          requestedApprovedAmount:
            amount,

          reason:
            reason
        }
      )
      .subscribe({

        next: (claim) => {

          this.actionCompleted(
            claim,
            'Claim partially approved successfully.'
          );

        },

        error: (error) => {

          this.handleError(error);

        }

      });
  }


  // -----------------------------------------------
  // REJECT
  // -----------------------------------------------

  confirmReject(): void {

    if (!this.claim) {
      return;
    }

    const reason =
      this.f.reason.value.trim();

    if (!reason) {

      this.f.reason.markAsTouched();

      return;
    }

    this.processing = true;

    this.clearMessages();

    this.claimService
      .rejectClaim(
        this.claim.claimNumber,
        {
          reason: reason
        }
      )
      .subscribe({

        next: (claim) => {

          this.actionCompleted(
            claim,
            'Claim rejected successfully.'
          );

        },

        error: (error) => {

          this.handleError(error);

        }

      });
  }


  // -----------------------------------------------
  // REQUEST MORE INFORMATION
  // -----------------------------------------------

  confirmMoreInformation(): void {

    if (!this.claim) {
      return;
    }

    const reason =
      this.f.reason.value.trim();

    if (!reason) {

      this.f.reason.markAsTouched();

      return;
    }

    this.processing = true;

    this.clearMessages();

    this.claimService
      .requestMoreInformation(
        this.claim.claimNumber,
        {
          reason: reason
        }
      )
      .subscribe({

        next: (claim) => {

          this.actionCompleted(
            claim,
            'Additional information requested successfully.'
          );

        },

        error: (error) => {

          this.handleError(error);

        }

      });
  }


  // -----------------------------------------------
  // STATUS HELPERS
  // -----------------------------------------------

  isSubmitted(): boolean {

    return this.claim?.status
      === 'Submitted';
  }


  isUnderReview(): boolean {

    return this.claim?.status
      === 'Under Review';
  }


  isMoreInformationRequired(): boolean {

    return this.claim?.status
      === 'More Information Required';
  }


  isFinalised(): boolean {

    const status =
      this.claim?.status;

    return (
      status === 'Approved' ||
      status === 'Partially Approved' ||
      status === 'Rejected'
    );
  }


  // -----------------------------------------------
  // CALCULATED REIMBURSEMENT
  // -----------------------------------------------

  get calculatedReimbursement(): number {

    if (!this.claim) {
      return 0;
    }

    return (
      this.claim.eligibleAmount *
      this.claim.reimbursementPercentage
      / 100
    );
  }


  // -----------------------------------------------
  // COMMON SUCCESS HANDLER
  // -----------------------------------------------

  private actionCompleted(
    claim: Claim,
    message: string
  ): void {

    this.claim = claim;

    this.processing = false;

    this.actionType = null;

    this.successMessage =
      message;

    this.decisionForm.reset({
      approvedAmount: 0,
      reason: ''
    });

    // Reload so decision history is definitely
    // retrieved from the backend/database.
    this.loadClaim();

    this.cdr.detectChanges();
  }


  // -----------------------------------------------
  // COMMON ERROR HANDLER
  // -----------------------------------------------

  private handleError(
    error: any
  ): void {

    console.error(
      'Claim action error:',
      error
    );

    this.processing = false;

    this.errorMessage =
      error?.error?.message
      ?? 'Unable to process this claim.';

    this.cdr.detectChanges();
  }


  // -----------------------------------------------
  // CLEAR UI MESSAGES
  // -----------------------------------------------

  private clearMessages(): void {

    this.errorMessage = '';

    this.successMessage = '';
  }
}
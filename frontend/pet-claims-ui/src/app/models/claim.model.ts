export interface ClaimDecision {
  decision: string;
  decisionDate: string;
  reason: string;
  approvedAmount: number;
}

export interface Claim {
  claimNumber: string;

  policyNumber: string;

  petName: string;

  petType: string;
  
  breed: string;

  treatmentDate: string;

  clinic: string;

  diagnosis: string;

  treatmentDescription: string;

  claimedAmount: number;

  approvedAmount: number;

  invoiceNumber: string;

  status: string;

  createdDate: string;

  policyStartDate: string;

  policyEndDate: string;

  coverage: string;

  annualLimit: number;

  deductible: number;

  reimbursementPercentage: number;

  eligibleAmount: number;

  remainingAnnualLimit: number;

  decisionHistory: ClaimDecision[];
}
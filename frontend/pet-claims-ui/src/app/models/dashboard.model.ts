export interface Dashboard {
  submittedCount: number;
  underReviewCount: number;
  approvedCount: number;
  partiallyApprovedCount: number;
  rejectedCount: number;
  moreInformationRequiredCount: number;

  totalClaimedAmount: number;
  totalApprovedAmount: number;
}
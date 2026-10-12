import { Routes } from '@angular/router';

import { Dashboard } from './components/dashboard/dashboard';
import { SubmitClaim } from './components/submit-claim/submit-claim';
import { ClaimReview } from './components/claim-review/claim-review';
import { Reports } from './components/reports/reports';
import { ImportExport } from './components/import-export/import-export';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },

  {
    path: 'dashboard',
    component: Dashboard
  },

  {
    path: 'submit-claim',
    component: SubmitClaim
  },

  {
    path: 'claims/:claimNumber',
    component: ClaimReview
  },

  {
    path: 'reports',
    component: Reports
  },

  {
    path: 'import-export',
    component: ImportExport
  }
];
import {
  Injectable,
  inject
} from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ExcelService {

  private readonly http =
    inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5287/api/excel/claims';

  exportClaims() {

    return this.http.get(
      `${this.apiUrl}/export`,
      {
        responseType: 'blob'
      }
    );
  }

  importClaims(
    file: File
  ) {

    const formData =
      new FormData();

    formData.append(
      'file',
      file
    );

    return this.http.post(
      `${this.apiUrl}/import`,
      formData
    );
  }

}
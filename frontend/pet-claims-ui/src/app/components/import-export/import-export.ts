import {
  Component,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { ExcelService } from '../../services/excel.service';

@Component({
  selector: 'app-import-export',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './import-export.html',
  styleUrl: './import-export.css'
})
export class ImportExport {

  private readonly excelService =
    inject(ExcelService);

  selectedFile: File | null = null;

  exportMessage = '';
  exportError = false;

  importMessage = '';
  importError = false;

  onFileSelected(event: Event): void {

    const input =
      event.target as HTMLInputElement;

    if (
      input.files &&
      input.files.length > 0
    ) {

      const file =
        input.files[0];

      if (
        !file.name
          .toLowerCase()
          .endsWith('.xlsx')
      ) {

        this.selectedFile = null;

        this.importError = true;

        this.importMessage =
          'Only .xlsx Excel files are supported.';

        input.value = '';

        return;
      }

      this.selectedFile = file;

      this.importError = false;

      this.importMessage = '';
    }
  }

  exportClaims(): void {

    this.excelService
      .exportClaims()
      .subscribe({

        next: (blob) => {

          const url =
            window.URL.createObjectURL(blob);

          const link =
            document.createElement('a');

          link.href = url;

          link.download =
            'PetClaims.xlsx';

          link.click();

          window.URL.revokeObjectURL(url);

          this.exportError = false;

          this.exportMessage =
            'Claims exported successfully.';
        },

        error: () => {

          this.exportError = true;

          this.exportMessage =
            'Export failed.';
        }

      });
  }

  importClaims(): void {

    if (!this.selectedFile) {

      this.importError = true;

      this.importMessage =
        'Please select an Excel file.';

      return;
    }

    this.excelService
      .importClaims(this.selectedFile)
      .subscribe({

        next: (response: any) => {

          this.importError = false;

          this.importMessage =
            response.message;
        },

        error: (error) => {

          this.importError = true;

          this.importMessage =
            error?.error?.message ??
            'Import failed.';
        }

      });
  }
}
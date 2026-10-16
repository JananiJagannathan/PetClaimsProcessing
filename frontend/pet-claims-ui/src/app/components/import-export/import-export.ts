import {
  Component,
  inject,
  ChangeDetectorRef
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
  private readonly cdr =
  inject(ChangeDetectorRef);

  selectedFile: File | null = null;

  exportMessage = '';
  exportError = false;

  importMessage = '';
  importError = false;

  isImporting = false;
  isExporting = false;
  onFileSelected(event: Event): void {

    const input =
      event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {

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

  if (this.isExporting) {
    return;
  }

  this.isExporting = true;

  this.exportError = false;
  this.exportMessage = '';

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

         this.isExporting = false;

this.exportError = false;

this.exportMessage =
  'Claims exported successfully.';
    this.cdr.detectChanges();
        },

       error: () => {

  this.isExporting = false;

  this.exportError = true;

  this.exportMessage =
    'Export failed.';
      this.cdr.detectChanges(); 
}
      });
  }

  importClaims(): void {

    if (this.isImporting) {
      return;
    }

    if (!this.selectedFile) {

      this.importError = true;

      this.importMessage =
        'Please select an Excel file.';

      return;
    }

    this.isImporting = true;

    this.importError = false;

    this.importMessage = '';

    this.excelService
      .importClaims(this.selectedFile)
      .subscribe({

        next: (response: any) => {

  this.isImporting = false;

  this.importError = false;

  this.importMessage =
    response?.message ??
    'Claims imported successfully.';

  this.selectedFile = null;

  this.cdr.detectChanges();
},
        error: (error) => {

  this.isImporting = false;


  this.importError = true;

  this.importMessage =
    error?.error?.message ??
    'Import failed.';

  this.selectedFile = null;

  this.cdr.detectChanges();

  console.log('Loading:', this.isImporting);
  console.log('Error:', this.importError);
  console.log('Message:', this.importMessage);
}

      });
  }
}
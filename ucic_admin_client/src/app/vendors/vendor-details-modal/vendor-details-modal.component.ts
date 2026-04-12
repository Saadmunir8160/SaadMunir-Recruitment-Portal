import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../environments/environment';
import { VendorsService } from '../services/vendors.service';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SuccessSnackbarComponent } from '../../shared/components/success-snackbar/success-snackbar.component';

@Component({
  selector: 'app-vendor-details-modal',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="vendor-details-modal">
      <div class="modal-header">
        <h2 mat-dialog-title class="modal-title">Vendor Details</h2>
        <button mat-button class="close-icon" mat-dialog-close>&times;</button>
      </div>
      <mat-dialog-content>
        <div class="content-wrapper">
          <div class="info-columns">
            <!-- Left Column -->
            <div class="info-column">
              <div class="info-group">
                <h3>Basic Information</h3>
                <div class="info-row">
                  <div class="info-item">
                    <strong>Company Name:</strong>
                    <span>{{ data.vendor?.name }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Address:</strong>
                    <span>{{ data.vendor?.address }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Phone:</strong>
                    <span>{{ data.vendor?.phoneNo }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Fax:</strong>
                    <span>{{ data.vendor?.faxNo }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Email:</strong>
                    <span>{{ data.vendor?.email }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Contact Person:</strong>
                    <span>{{ data.vendor?.contactPerson }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Contact Person Email:</strong>
                    <span>{{ data.vendor?.contactPersonEmail }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Contact Person Phone:</strong>
                    <span>{{ data.vendor?.contactPersonPhoneNo }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Category:</strong>
                    <span>{{ data.vendor?.category }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Right Column -->
            <div class="info-column">
              <div class="info-group">
                <h3>Business Details</h3>
                <div class="info-row">
                  <div class="info-item">
                    <strong>Business Partner ID:</strong>
                    <span class="bp-id">{{ data.vendor?.bpId || 'Not Assigned' }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Employees:</strong>
                    <span>{{ data.vendor?.employees }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Tax Registration No:</strong>
                    <span>{{ data.vendor?.taxRegistrationNo }}</span>
                  </div>
                  <div class="info-item">
                    <strong>CR No:</strong>
                    <span>{{ data.vendor?.crNo }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Product Details:</strong>
                    <span>{{ data.vendor?.productDetails }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Annual Turnover:</strong>
                    <span>{{ data.vendor?.annualTurnover }}</span>
                  </div>
                  <div class="info-item">
                    <strong>Major Customers:</strong>
                    <span>{{ data.vendor?.majorCustomers }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <div class="documents-section">
            <h3>Documents</h3>
            <div class="documents-list">
              <a *ngIf="data.vendor?.crCertificateFilePath" [href]="data.vendor.crCertificateFilePath" target="_blank" class="document-link">
                <i class="bi bi-file-earmark-text"></i>
                <span>CR Certificate</span>
              </a>
              <a *ngIf="data.vendor?.vatCertificateFilePath" [href]="data.vendor.vatCertificateFilePath" target="_blank" class="document-link">
                <i class="bi bi-file-earmark-text"></i>
                <span>VAT Certificate</span>
              </a>
              <a *ngIf="data.vendor?.iSO9001_2015FilePath" [href]="data.vendor.iSO9001_2015FilePath" target="_blank" class="document-link">
                <i class="bi bi-file-earmark-text"></i>
                <span>ISO 9001:2015 Certificate</span>
              </a>
              <a *ngIf="data.vendor?.iSO14001_2015FilePath" [href]="data.vendor.iSO14001_2015FilePath" target="_blank" class="document-link">
                <i class="bi bi-file-earmark-text"></i>
                <span>ISO 14001:2015 Certificate</span>
              </a>
              <a *ngIf="data.vendor?.iSO45001_2018FilePath" [href]="data.vendor.iSO45001_2018FilePath" target="_blank" class="document-link">
                <i class="bi bi-file-earmark-text"></i>
                <span>ISO 45001:2018 Certificate</span>
              </a>
              <a *ngIf="data.vendor?.companyProfileFilePath" [href]="data.vendor.companyProfileFilePath" target="_blank" class="document-link">
                <i class="bi bi-file-earmark-text"></i>
                <span>Company Profile</span>
              </a>
            </div>
          </div>
        </div>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button *ngIf="!data.vendor?.isApproved" 
                mat-raised-button 
                class="approve-button"
                [disabled]="isApproving || isDeleting"
                (click)="approveVendor()">
          <div class="d-flex align-items-center">
            <div class="spinner-border spinner-border-sm me-2" role="status" *ngIf="isApproving">
              <span class="visually-hidden">Loading...</span>
            </div>
            <iconify-icon icon="solar:check-circle-bold" class="me-1" *ngIf="!isApproving"></iconify-icon>
            {{ isApproving ? 'Approving...' : 'Approve' }}
          </div>
        </button>
        <button *ngIf="!data.vendor?.isApproved"
                mat-raised-button 
                class="delete-button"
                [disabled]="isApproving || isDeleting"
                (click)="deleteVendor()">
          <div class="d-flex align-items-center">
            <div class="spinner-border spinner-border-sm me-2" role="status" *ngIf="isDeleting">
              <span class="visually-hidden">Loading...</span>
            </div>
            <iconify-icon icon="solar:trash-bin-trash-bold" class="me-1" *ngIf="!isDeleting"></iconify-icon>
            {{ isDeleting ? 'Deleting...' : 'Delete' }}
          </div>
        </button>
        <button mat-button mat-dialog-close class="close-button" [disabled]="isApproving || isDeleting">Close</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
      max-width: 1200px;
    }

    ::ng-deep .mat-mdc-dialog-container {
      padding: 0 !important;
    }

    .vendor-details-modal {
      padding: 0;
      width: 100%;
      background: white;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 8px 32px rgba(0,0,0,0.1);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      background: #f8f9fa;
      border-bottom: 1px solid #e0e0e0;
    }

    .modal-title {
      color: #2c3e50;
      margin: 0;
      font-size: 24px;
      font-weight: 600;
    }

    .close-icon {
      font-size: 24px;
      line-height: 1;
      padding: 4px;
      min-width: auto;
      color: #666;
      border-radius: 50%;
      transition: all 0.2s ease;
    }

    .close-icon:hover {
      background: rgba(0,0,0,0.1);
    }

    .content-wrapper {
      display: flex;
      flex-direction: column;
      gap: 24px;
      padding: 24px;
      background: white;
    }

    .info-columns {
      display: flex;
      gap: 24px;
    }

    .info-column {
      flex: 1;
    }

    .info-group {
      background: #f8f9fa;
      padding: 24px;
      border-radius: 12px;
      height: 100%;
      border: 1px solid #e0e0e0;
      transition: all 0.2s ease;
    }

    .info-group:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .info-group h3 {
      color: #2c3e50;
      margin: 0 0 20px 0;
      font-size: 18px;
      font-weight: 600;
      padding-bottom: 12px;
      border-bottom: 2px solid #e0e0e0;
    }

    .info-row {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .info-item {
      display: flex;
      flex-direction: row;
      gap: 12px;
      align-items: flex-start;
    }

    .info-item strong {
      color: #4a5568;
      font-size: 14px;
      font-weight: 600;
      min-width: 160px;
    }

    .info-item span {
      color: #2d3748;
      font-size: 14px;
      flex: 1;
      word-break: break-word;
    }

    .bp-id {
      color: #2563eb !important;
      font-weight: 600 !important;
    }

    .documents-section {
      background: #f8f9fa;
      padding: 24px;
      border-radius: 12px;
      border: 1px solid #e0e0e0;
      transition: all 0.2s ease;
    }

    .documents-section:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .documents-section h3 {
      color: #2c3e50;
      margin: 0 0 20px 0;
      font-size: 18px;
      font-weight: 600;
      padding-bottom: 12px;
      border-bottom: 2px solid #e0e0e0;
    }

    .documents-list {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
    }

    .document-link {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      background: white;
      border-radius: 8px;
      color: #3182ce;
      text-decoration: none;
      transition: all 0.2s ease;
      border: 1px solid #e2e8f0;
      font-size: 14px;
      font-weight: 500;
    }

    .document-link:hover {
      background: #f8fafc;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
      color: #2563eb;
    }

    .document-link i {
      font-size: 20px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
      border-top: 1px solid #e0e0e0;
      background: #f8f9fa;
      gap: 12px;
    }

    .close-button {
      padding: 8px 24px;
      font-size: 14px;
      border-radius: 6px;
      font-weight: 500;
      border: 1px solid #e2e8f0;
      color: #4a5568;
      background-color: white;
      transition: all 0.2s ease;
    }

    .close-button:hover {
      background-color: #f8fafc;
      border-color: #cbd5e0;
      color: #2563eb;
      box-shadow: 0 2px 8px rgba(0,0,0,0.05);
    }

    .approve-button {
      padding: 8px 24px;
      font-size: 14px;
      border-radius: 6px;
      font-weight: 500;
      border: 1px solid #e2e8f0;
      background-color: white;
      color: #4a5568;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .approve-button:hover:not(:disabled) {
      background-color: #f0fdf4;
      border-color: #86efac;
      color: #15803d;
      box-shadow: 0 2px 8px rgba(34, 197, 94, 0.2);
    }

    .approve-button:active:not(:disabled) {
      transform: translateY(0);
      box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);
    }

    .approve-button:disabled {
      background-color: #f8fafc;
      color: #94a3b8;
      border-color: #e2e8f0;
      cursor: not-allowed;
    }

    .approve-button iconify-icon {
      font-size: 18px;
    }

    .spinner-border {
      width: 1rem;
      height: 1rem;
      border-width: 0.15em;
    }

    mat-dialog-content {
      margin: 0;
      padding: 0;
      max-height: calc(100vh - 180px) !important;
      overflow-y: auto;
    }

    ::ng-deep .centered-dialog {
      mat-dialog-container {
        border-radius: 12px !important;
        overflow: hidden !important;
      }
    }

    .delete-button {
      padding: 8px 24px;
      font-size: 14px;
      border-radius: 6px;
      font-weight: 500;
      border: 1px solid #e2e8f0;
      background-color: white;
      color: #dc2626;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .delete-button:hover:not(:disabled) {
      background-color: #fef2f2;
      border-color: #fecaca;
      color: #dc2626;
      box-shadow: 0 2px 8px rgba(220, 38, 38, 0.2);
    }

    .delete-button:active:not(:disabled) {
      transform: translateY(0);
      box-shadow: 0 2px 4px rgba(220, 38, 38, 0.2);
    }

    .delete-button:disabled {
      background-color: #f8fafc;
      color: #94a3b8;
      border-color: #e2e8f0;
      cursor: not-allowed;
    }

    .delete-button iconify-icon {
      font-size: 18px;
    }
  `]
})
export class VendorDetailsModalComponent {
  isApproving = false;
  isDeleting = false;

  constructor(
    public dialogRef: MatDialogRef<VendorDetailsModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { vendor: any },
    private vendorsService: VendorsService,
    private snackBar: MatSnackBar
  ) {
    dialogRef.updateSize('1000px', 'auto');
    dialogRef.addPanelClass('centered-dialog');
  }

  getFullPath(relativePath: string): string {
    
    if (!relativePath) return '';
    
    // Extract just the relative path part after 'wwwroot'
    const wwwrootIndex = relativePath.toLowerCase().indexOf('wwwroot');
    let cleanPath = '';
    
    if (wwwrootIndex !== -1) {
      // If path contains 'wwwroot', take everything after it
      cleanPath = relativePath.substring(wwwrootIndex + 'wwwroot'.length);
    } else {
      // If no 'wwwroot' in path, use the path as is
      cleanPath = relativePath;
    }
    
    // Remove any leading slashes or backslashes
    cleanPath = cleanPath.replace(/^[/\\]+/, '');
    
    // Replace any backslashes with forward slashes
    cleanPath = cleanPath.replace(/\\/g, '/');
    
    // Construct the final URL
    return `${environment.apiUrl}/wwwroot/${cleanPath}`;
  }

  approveVendor(): void {
    if (confirm('Are you sure you want to approve this vendor?')) {
      this.isApproving = true;
      this.vendorsService.approveVendors(this.data.vendor.vendorId).subscribe({
        next: (response) => {
          // Update the vendor status locally
          this.data.vendor.isApproved = true;
          this.data.vendor.bpId = response;
          
          // Show success message with custom snackbar
          this.snackBar.openFromComponent(SuccessSnackbarComponent, {
            data: {
              message: `Vendor is approved and created in LN with Business Partner ID ${response}`
            },
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
            panelClass: ['custom-snackbar']
          });
          
          // Close the dialog
          this.dialogRef.close({ success: true, bpId: response });
        },
        error: (err) => {
          console.error('Error approving vendor:', err);
          this.snackBar.open('Error approving vendor', 'Close', { duration: 5000 });
        },
        complete: () => {
          this.isApproving = false;
        }
      });
    }
  }

  deleteVendor(): void {
    if (confirm('Are you sure you want to delete this vendor? This action cannot be undone.')) {
      this.isDeleting = true;
      this.vendorsService.deleteVendor(this.data.vendor.vendorId).subscribe({
        next: (success) => {
          if (success) {
            // Show success message
            this.snackBar.openFromComponent(SuccessSnackbarComponent, {
              data: {
                message: 'Vendor deleted successfully'
              },
              duration: 5000,
              horizontalPosition: 'center',
              verticalPosition: 'top',
              panelClass: ['custom-snackbar']
            });
            
            // Close the dialog with success
            this.dialogRef.close({ success: true, deleted: true });
          }
        },
        error: (err) => {
          console.error('Error deleting vendor:', err);
          this.snackBar.open('Error deleting vendor', 'Close', { duration: 5000 });
        },
        complete: () => {
          this.isDeleting = false;
        }
      });
    }
  }
} 
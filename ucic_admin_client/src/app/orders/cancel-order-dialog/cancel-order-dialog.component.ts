import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-cancel-order-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, MatButtonModule],
  templateUrl: './cancel-order-dialog.component.html'
})
export class CancelOrderDialogComponent {
  reason: string;
  options: string[] = ['Out of Stock', 'Price difference', 'None'];

  constructor(
    public dialogRef: MatDialogRef<CancelOrderDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    if (data?.options && Array.isArray(data.options)) {
      this.options = data.options;
    }
    this.reason = this.options[0];
  }

  confirm(): void {
    this.dialogRef.close(this.reason);
  }

  cancel(): void {
    this.dialogRef.close();
  }
}

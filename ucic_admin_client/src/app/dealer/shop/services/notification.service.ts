import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private snackBar: MatSnackBar) {}

  /**
   * Show success notification
   */
  success(message: string, duration: number = 3000): void {
    this.show(message, 'success-snackbar', duration);
  }

  /**
   * Show error notification
   */
  error(message: string, duration: number = 4000): void {
    this.show(message, 'error-snackbar', duration);
  }

  /**
   * Show info notification
   */
  info(message: string, duration: number = 3000): void {
    this.show(message, 'info-snackbar', duration);
  }

  /**
   * Show warning notification
   */
  warning(message: string, duration: number = 3000): void {
    this.show(message, 'warning-snackbar', duration);
  }

  /**
   * Show a generic notification
   */
  private show(message: string, panelClass: string, duration: number): void {
    const config: MatSnackBarConfig = {
      duration: duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: [panelClass, 'custom-snackbar']
    };

    this.snackBar.open(message, '✕', config);
  }
}

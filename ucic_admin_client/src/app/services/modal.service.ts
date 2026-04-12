import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private showChangePasswordModal = new BehaviorSubject<boolean>(false);
  
  showChangePasswordModal$ = this.showChangePasswordModal.asObservable();

  constructor() { }

  openChangePasswordModal(): void {
    this.showChangePasswordModal.next(true);
  }

  closeChangePasswordModal(): void {
    this.showChangePasswordModal.next(false);
  }

  toggleChangePasswordModal(): void {
    this.showChangePasswordModal.next(!this.showChangePasswordModal.value);
  }
}

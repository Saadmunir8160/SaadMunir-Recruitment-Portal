import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { ModalService } from '../../services/modal.service';
import { RouterModule } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

declare function initSwitchListener(): void;

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})

export class HeaderComponent implements OnInit {
  isDropdownOpen = false;
  fullName: string = '';

  constructor(
    private authService: AuthService,
    private modalService: ModalService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    if (isPlatformBrowser(this.platformId)) {
      this.fullName = localStorage.getItem('FullName') || '';
    }
  }

  ngOnInit(): void {
    // Initialize any necessary setup
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  toggleMenu(): void {
    const htmlElement = document.querySelector('html');
    if (htmlElement) {
      const currentSize = htmlElement.getAttribute('data-menu-size');
      // Toggle between expanded and condensed (icon-only) states
      const newSize = currentSize === 'condensed' ? 'expanded' : 'condensed';
      
      // Simply set the attribute - let CSS handle the transitions
      htmlElement.setAttribute('data-menu-size', newSize);
    }
  }

  changePassword(): void {
    this.isDropdownOpen = false;
    this.modalService.openChangePasswordModal();
  }

  logout(): void {
    this.authService.logout();
  }
}

import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "../header/header.component";
import { FooterComponent } from "../footer/footer.component";
import { ChangePasswordComponent } from '../../users/change-password/change-password.component';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../services/modal.service';
import { isPlatformBrowser } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-main-layout',
  imports: [SidebarComponent, RouterOutlet, HeaderComponent, FooterComponent, ChangePasswordComponent, CommonModule],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent implements OnInit {
  showChangePasswordModal$: Observable<boolean>;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private modalService: ModalService
  ) {
    this.showChangePasswordModal$ = this.modalService.showChangePasswordModal$;
  }

  ngOnInit(): void {
    // Initialize the default menu size on component load
    if (isPlatformBrowser(this.platformId)) {
      const htmlElement = document.querySelector('html');
      if (htmlElement && !htmlElement.getAttribute('data-menu-size')) {
        htmlElement.setAttribute('data-menu-size', 'expanded');
      }
    }
  }
}

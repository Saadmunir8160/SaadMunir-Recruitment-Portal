import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { RoleService } from '../../services/role.service';
import { filter } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SidebarComponent implements OnInit {
  menuItems: any[] = [];
  currentRoute: string = '';

  constructor(
    private roleService: RoleService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit() {
    this.menuItems = this.roleService.getMenuItems();

    // Subscribe to router events to track current route
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentRoute = event.url;
    });

    // Set initial route
    this.currentRoute = this.router.url;
  }

  isMenuActive(menuItem: any): boolean {
    if (!menuItem.route) return false;

    // Check if current route starts with the menu item route
    return this.currentRoute.startsWith(menuItem.route);
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('token');
      localStorage.removeItem('userRole');
      location.reload();
    }
  }

  toggleMenu(): void {
    const htmlElement = document.querySelector('html');
    if (htmlElement) {
      const currentSize = htmlElement.getAttribute('data-menu-size');
      // Toggle between expanded and condensed (icon-only) states
      // We don't use 'hidden' as it completely hides the sidebar
      const newSize = currentSize === 'condensed' ? 'expanded' : 'condensed';
      htmlElement.setAttribute('data-menu-size', newSize);
    }
  }
}

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { DealerAuthService, DealerInfo } from '../services/dealer-auth.service';
import { CartService } from '../shop/services/cart.service';
import { CartSummary } from '../shop/models/cart-item.model';
import { Subscription, filter } from 'rxjs';
import { LanguageSwitchComponent } from '../components/language-switch/language-switch.component';
import { TranslateModule } from '@ngx-translate/core';
import { DealerTranslationService } from '../services/translation.service';

@Component({
  selector: 'app-dealer-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, LanguageSwitchComponent, TranslateModule],
  templateUrl: './dealer-layout.component.html',
  styleUrl: './dealer-layout.component.scss'
})
export class DealerLayoutComponent implements OnInit, OnDestroy {
  isSidebarCollapsed = false;
  dealerInfo: DealerInfo | null = null;
  cartItemCount = 0;
  currentRoute = '';
  private cartSubscription: Subscription = new Subscription();
  private routerSubscription: Subscription = new Subscription();
  private dealerSubscription: Subscription = new Subscription();

  constructor(
    public router: Router,
    private authService: AuthService,
    private dealerAuthService: DealerAuthService,
    private cartService: CartService,
    private translationService: DealerTranslationService
  ) {}

  ngOnInit(): void {
    // Initialize translation service
    this.translationService.initializeLanguage();
    
    // Set sidebar collapsed state based on screen size
    this.checkScreenSize();
    
    this.loadDealerInfo();
    
    // Subscribe to cart summary updates to get item count
    this.cartSubscription = this.cartService.getCartSummary().subscribe((summary: CartSummary) => {
      this.cartItemCount = summary.itemCount;
    });

    // Subscribe to router events to track active route
    this.routerSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.currentRoute = this.extractRouteFromUrl(event.url);
    });

    // Set initial route
    this.currentRoute = this.extractRouteFromUrl(this.router.url);
  }

  ngOnDestroy(): void {
    this.cartSubscription.unsubscribe();
    this.routerSubscription.unsubscribe();
    this.dealerSubscription.unsubscribe();
  }

  private checkScreenSize(): void {
    // Collapse sidebar on mobile devices (screen width < 1024px)
    this.isSidebarCollapsed = window.innerWidth < 1024;
  }

  private extractRouteFromUrl(url: string): string {
    // Extract the main route section from URL like '/dealer/drivers' -> 'drivers'
    const segments = url.split('/');
    if (segments.length >= 3 && segments[1] === 'dealer') {
      return segments[2];
    }
    return '';
  }

  private loadDealerInfo(): void {
    // Subscribe to current dealer info from the authentication service
    this.dealerSubscription = this.dealerAuthService.currentDealer$.subscribe((dealer: DealerInfo | null) => {
      this.dealerInfo = dealer;
      
      // If no dealer info from service, try to get from localStorage as fallback
      if (!dealer) {
        this.loadDealerInfoFromStorage();
      }
    });
  }

  private loadDealerInfoFromStorage(): void {
    // Fallback: get basic user info from localStorage
    const storedUser = localStorage.getItem('user');
    const storedFullName = localStorage.getItem('FullName');
    
    if (storedUser) {
      try {
        const userData = JSON.parse(storedUser);
        // Create a basic dealer info object from available data
        this.dealerInfo = {
          id: userData.userId || 0,
          dealerCode: userData.dealerCode ,
          companyName: storedFullName || userData.name || 'Unknown Dealer',
          email: userData.email || '',
          contactPerson: userData.name || '',
          phone: '',
          status: 'Active' as const,
          role: userData.role || 'dealer',
          permissions: [],
          profileComplete: false,
          isVerified: true
        };
      } catch (error) {
        console.error('Error parsing stored user data:', error);
      }
    }
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  navigateTo(route: string): void {
    this.router.navigate([`/dealer/${route}`]);
  }

  isActiveRoute(route: string): boolean {
    return this.currentRoute === route;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

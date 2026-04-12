import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DealerDailyLimits } from '../shop/models/order.model';

export interface DailyUsage {
  date: string;
  totalBagsUsed: number;
  totalTonsUsed: number;
  ordersCount: number;
}

export interface LimitCheckRequest {
  dealerId: number;
  requestedBags: number;
  requestedTons: number;
}

export interface LimitCheckResponse {
  canProceed: boolean;
  exceedsLimit: boolean;
  remainingBags: number;
  remainingTons: number;
  errorMessage?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DailyLimitsService {
  private apiUrl = environment.apiUrl; // Remove the extra /api since environment.apiUrl already contains it
  private currentLimitsSubject = new BehaviorSubject<DealerDailyLimits | null>(null);
  
  // Observable for components to subscribe to limit changes
  public currentLimits$ = this.currentLimitsSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get current dealer's daily limits and usage (uses authenticated dealer context)
   */
  getDealerDailyLimits(dealerId?: number): Observable<DealerDailyLimits> {
    // Note: dealerId parameter is ignored, backend uses current authenticated dealer
    return this.http.get<DealerDailyLimits>(`${this.apiUrl}/DealerPortal/daily-limits`)
      .pipe(
        tap(limits => this.currentLimitsSubject.next(limits))
      );
  }

  /**
   * Check if a potential order would exceed daily limits
   */
  checkOrderLimits(request: LimitCheckRequest): Observable<LimitCheckResponse> {
    return this.http.post<LimitCheckResponse>(`${this.apiUrl}/dealers/${request.dealerId}/check-limits`, request);
  }

  /**
   * Get dealer's usage history for a specific date range
   */
  getDealerUsageHistory(dealerId: number, startDate: string, endDate: string): Observable<DailyUsage[]> {
    const params = { startDate, endDate };
    return this.http.get<DailyUsage[]>(`${this.apiUrl}/dealers/${dealerId}/usage-history`, { params });
  }

  /**
   * Update current order quantities in the limits (for real-time tracking)
   */
  updateCurrentOrderQuantities(dealerId: number, bags: number, tons: number): Observable<DealerDailyLimits> {
    // Note: dealerId parameter is ignored, backend uses current authenticated dealer
    const request = { currentOrderTons: tons, currentOrderBags: bags };
    return this.http.patch<DealerDailyLimits>(`${this.apiUrl}/DealerPortal/current-order`, request)
      .pipe(
        tap(limits => this.currentLimitsSubject.next(limits))
      );
  }

  /**
   * Reset current order quantities (when cart is cleared or order is cancelled)
   */
  resetCurrentOrderQuantities(dealerId: number): Observable<DealerDailyLimits> {
    return this.updateCurrentOrderQuantities(dealerId, 0, 0);
  }

  /**
   * Calculate total bags/tons from cart items based on product units
   */
  calculateCartTotals(cartItems: any[]): { totalBags: number; totalTons: number } {
    let totalBags = 0;
    let totalTons = 0;

    for (const item of cartItems) {
      if (item.unit === 'bags') {
        totalBags += item.quantity;
        // Convert to tons if bagsPerTon is available
        if (item.bagsPerTon && item.bagsPerTon > 0) {
          totalTons += item.quantity / item.bagsPerTon;
        }
      } else if (item.unit === 'tons') {
        totalTons += item.quantity;
        // Convert to bags if bagsPerTon is available
        if (item.bagsPerTon && item.bagsPerTon > 0) {
          totalBags += item.quantity * item.bagsPerTon;
        }
      }
    }

    return { totalBags, totalTons };
  }

  /**
   * Validate if cart quantities are within daily limits
   */
  validateCartAgainstLimits(cartItems: any[], currentLimits: DealerDailyLimits): {
    isValid: boolean;
    exceedsBags: boolean;
    exceedsTons: boolean;
    errorMessage?: string;
  } {
    const cartTotals = this.calculateCartTotals(cartItems);
    
    const exceedsBags = (currentLimits.usedTodayBags + cartTotals.totalBags) > currentLimits.totalLimitBags;
    const exceedsTons = (currentLimits.usedTodayTons + cartTotals.totalTons) > currentLimits.totalLimitTons;

    let errorMessage = '';
    if (exceedsBags && exceedsTons) {
      errorMessage = 'Cart exceeds both daily bags and tons limits';
    } 
    // else if (exceedsBags) {
    //   errorMessage = `Cart exceeds daily bags limit. Available: ${currentLimits.remainingTodayBags} bags`;
    // } 
    else if (exceedsTons) {
      errorMessage = `Cart exceeds daily tons limit. Available: ${currentLimits.remainingTodayTons} tons`;
    }

    return {
      isValid: !exceedsBags && !exceedsTons,
      exceedsBags,
      exceedsTons,
      errorMessage: errorMessage || undefined
    };
  }

  /**
   * Get current cached limits without making API call
   */
  getCurrentLimits(): DealerDailyLimits | null {
    return this.currentLimitsSubject.value;
  }
}
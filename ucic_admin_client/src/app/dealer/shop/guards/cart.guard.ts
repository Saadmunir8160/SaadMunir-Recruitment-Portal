import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { CartService } from '../services/cart.service';

@Injectable({
  providedIn: 'root'
})
export class CartGuard implements CanActivate {
  constructor(
    private cartService: CartService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> {
    // This guard is used for checkout route protection
    // It prevents access to checkout when cart is empty
    return this.cartService.getCartItems().pipe(
      take(1),
      map(cartItems => {
        if (cartItems.length === 0) {
          // If cart is empty, redirect to cart page with message
          this.router.navigate(['/dealer/shop/cart']);
          return false;
        }
        return true;
      })
    );
  }
}
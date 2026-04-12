import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CartGuard } from './guards/cart.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'cart', pathMatch: 'full' },
  
  // Cart Review (simplified shopping process)
  { 
    path: 'cart', 
    loadComponent: () => import('./components/step3-cart/cart.component')
      .then(m => m.CartComponent)
  },
  
  // Checkout
  { 
    path: 'checkout', 
    loadComponent: () => import('./components/step4-checkout/step4-checkout.component')
      .then(m => m.Step4CheckoutComponent),
    canActivate: [CartGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ShopRoutingModule { }

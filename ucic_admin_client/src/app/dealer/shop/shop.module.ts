import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { ShopRoutingModule } from './shop-routing.module';

// Services
import { ShopService } from './services/shop.service';
import { CartService } from './services/cart.service';
import { DealerOrdersService } from './services/dealer-orders.service';

@NgModule({
  declarations: [
    // All components are now standalone
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    ShopRoutingModule
  ],
  providers: [
    ShopService,
    CartService,
    DealerOrdersService
  ]
})
export class ShopModule { }

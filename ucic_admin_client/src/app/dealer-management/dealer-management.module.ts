import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DealerManagementRoutingModule } from './dealer-management-routing.module';
import { DealerManagementComponent } from './dealer-management.component';
import { DealerOverviewComponent } from './components/dealer-overview/dealer-overview.component';
import { DealerProductsComponent } from './components/dealer-products/dealer-products.component';
import { DealerOrdersComponent } from './dealer-orders/dealer-orders.component';
import { DealerDriversComponent } from './dealer-drivers/dealer-drivers.component';
import { DealerVehiclesComponent } from './dealer-vehicles/dealer-vehicles.component';

@NgModule({
  imports: [
    CommonModule,
    DealerManagementRoutingModule,
    DealerManagementComponent,
    DealerOverviewComponent,
    DealerProductsComponent,
    DealerOrdersComponent,
    DealerDriversComponent,
    DealerVehiclesComponent
  ]
})
export class DealerManagementModule { }
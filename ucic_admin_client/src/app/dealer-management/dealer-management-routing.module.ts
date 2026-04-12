import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DealerManagementComponent } from './dealer-management.component';
import { DealerOverviewComponent } from './components/dealer-overview/dealer-overview.component';
import { DealerProductsComponent } from './components/dealer-products/dealer-products.component';
import { DealerOrdersComponent } from './dealer-orders/dealer-orders.component';
import { DealerDriversComponent } from './dealer-drivers/dealer-drivers.component';
import { DealerVehiclesComponent } from './dealer-vehicles/dealer-vehicles.component';

const routes: Routes = [
  {
    path: '',
    component: DealerManagementComponent,
    children: [
      { path: '', redirectTo: 'overview', pathMatch: 'full' },
      { path: 'overview', component: DealerOverviewComponent },
      { path: 'dealers', loadComponent: () => import('./components/dealers/dealers.component').then(c => c.DealersComponent) },
      { path: 'products', component: DealerProductsComponent },
      { path: 'products/create', loadComponent: () => import('./components/dealer-products/product-form/product-form.component').then(c => c.ProductFormComponent) },
      { path: 'products/edit/:id', loadComponent: () => import('./components/dealer-products/product-form/product-form.component').then(c => c.ProductFormComponent) },
      { path: 'orders', component: DealerOrdersComponent },
      { path: 'drivers', component: DealerDriversComponent },
      { path: 'vehicles', component: DealerVehiclesComponent },
      { path: 'areas', loadComponent: () => import('./dealer-areas/dealer-areas.component').then(c => c.DealerAreasComponent) },
      { path: 'addresses', loadComponent: () => import('./components/dealer-addresses/dealer-addresses.component').then(c => c.DealerAddressesComponent) },
      { path: 'daily-limits', loadComponent: () => import('./daily-limits/daily-limits.component').then(c => c.DailyLimitsComponent) },
      { path: 'support-tickets', loadComponent: () => import('./components/support-tickets/support-tickets.component').then(c => c.SupportTicketsComponent) }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DealerManagementRoutingModule { }
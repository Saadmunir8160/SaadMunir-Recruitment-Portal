import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DealerLayoutComponent } from './layout/dealer-layout.component';

const routes: Routes = [
  {
    path: '',
    component: DealerLayoutComponent,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dealer-dashboard.component').then(m => m.DealerDashboardComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./shop/components/step1-products/products.component').then(m => m.ProductsComponent)
      },
      {
        path: 'product-details/:id',
        loadComponent: () => import('./shop/components/step2-product-details/product-details.component').then(m => m.ProductDetailsComponent)
      },
      {
        path: 'shop',
        loadChildren: () => import('./shop/shop-routing.module').then(m => m.routes)
      },
      {
        path: 'orders',
        loadComponent: () => import('./order-history/order-history.component').then(m => m.OrderHistoryComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./dealer-profile/dealer-profile.component').then(m => m.DealerProfileComponent)
      },
      {
        path: 'support',
        loadComponent: () => import('./support/support.component').then(m => m.SupportComponent)
      },
      {
        path: 'addresses',
        children: [
          {
            path: '',
            loadComponent: () => import('./components/addresses/addresses-list/addresses-list.component').then(m => m.AddressesListComponent)
          },
          {
            path: 'create',
            loadComponent: () => import('./components/addresses/address-create/address-create.component').then(m => m.AddressCreateComponent)
          },
          {
            path: 'edit/:id',
            loadComponent: () => import('./components/addresses/address-edit/address-edit.component').then(m => m.AddressEditComponent)
          }
        ]
      },
      {
        path: 'drivers',
        children: [
          {
            path: '',
            loadComponent: () => import('./components/drivers/drivers-list/drivers-list.component').then(m => m.DriversListComponent)
          },
          {
            path: 'create',
            loadComponent: () => import('./components/drivers/driver-create/driver-create.component').then(m => m.DriverCreateComponent)
          },
          {
            path: 'edit/:id',
            loadComponent: () => import('./components/drivers/driver-edit/driver-edit.component').then(m => m.DriverEditComponent)
          }
        ]
      },
      {
        path: 'vehicles',
        children: [
          {
            path: '',
            loadComponent: () => import('./components/vehicles/vehicles-list/vehicles-list.component').then(m => m.VehiclesListComponent)
          },
          {
            path: 'create',
            loadComponent: () => import('./components/vehicles/vehicle-create/vehicle-create.component').then(m => m.VehicleCreateComponent)
          },
          {
            path: 'edit/:id',
            loadComponent: () => import('./components/vehicles/vehicle-edit/vehicle-edit.component').then(m => m.VehicleEditComponent)
          }
        ]
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DealerRoutingModule { }

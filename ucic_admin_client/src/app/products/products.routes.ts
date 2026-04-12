import { Routes } from '@angular/router';
import { RoleGuard } from '../guards/role.guard';
import { UserRole } from '../services/role.service';
import { ProductsListComponent } from './products-list/products-list.component';
import { ProductCreateComponent } from './product-create/product-create.component';
import { ProductEditComponent } from './product-edit/product-edit.component';

export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    component: ProductsListComponent,
    canActivate: [RoleGuard],
    data: { roles: [UserRole.ADMIN] }
  },
  {
    path: 'create',
    component: ProductCreateComponent,
    canActivate: [RoleGuard],
    data: { roles: [UserRole.ADMIN] }
  },
  {
    path: 'edit/:id',
    component: ProductEditComponent,
    canActivate: [RoleGuard],
    data: { roles: [UserRole.ADMIN] }
  }
]; 
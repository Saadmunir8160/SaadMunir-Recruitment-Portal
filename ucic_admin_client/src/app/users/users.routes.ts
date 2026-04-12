import { Routes } from '@angular/router';
import { RoleGuard } from '../guards/role.guard';
import { UserRole } from '../services/role.service';
import { UserListComponent } from './user-list/user-list.component';
import { UserCreateComponent } from './user-create/user-create.component';
import { UserEditComponent } from './user-edit/user-edit.component';

export const USERS_ROUTES: Routes = [
  {
    path: '',
    component: UserListComponent,
    canActivate: [RoleGuard],
    data: { roles: [UserRole.ADMIN] }
  },
  {
    path: 'create',
    component: UserCreateComponent,
    canActivate: [RoleGuard],
    data: { roles: [UserRole.ADMIN] }
  },
  {
    path: 'edit/:id',
    component: UserEditComponent,
    canActivate: [RoleGuard],
    data: { roles: [UserRole.ADMIN] }
  }
]; 
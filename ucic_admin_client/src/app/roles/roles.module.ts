import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { RoleListComponent } from './role-list/role-list.component';
import { RoleCreateComponent } from './role-create/role-create.component';
import { RoleEditComponent } from './role-edit/role-edit.component';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: '', component: RoleListComponent },
      { path: 'create', component: RoleCreateComponent },
      { path: 'edit/:id', component: RoleEditComponent }
    ]),
    RoleListComponent,
    RoleCreateComponent,
    RoleEditComponent
  ]
})
export class RolesModule { } 
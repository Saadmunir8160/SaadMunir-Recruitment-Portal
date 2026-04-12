import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { PromotionListComponent } from './list/promotion-list.component';
import { PromotionCreateComponent } from './create/promotion-create.component';
import { PromotionEditComponent } from './edit/promotion-edit.component';
import { PromotionService } from '../services/promotion.service';

const routes: Routes = [
  { path: 'list', component: PromotionListComponent },
  { path: 'create', component: PromotionCreateComponent },
  { path: 'edit/:id', component: PromotionEditComponent },
  { path: '', redirectTo: 'list', pathMatch: 'full' }
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    PromotionListComponent,
    PromotionCreateComponent,
    PromotionEditComponent
  ],
  providers: [PromotionService]
})
export class PromotionModule { } 
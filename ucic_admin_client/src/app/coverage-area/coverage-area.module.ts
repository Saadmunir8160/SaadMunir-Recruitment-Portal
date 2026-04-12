import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { CoverageAreaListComponent } from './list/coverage-area-list.component';
import { CoverageAreaCreateComponent } from './create/coverage-area-create.component';
import { CoverageAreaEditComponent } from './edit/coverage-area-edit.component';
import { CoverageAreaService } from '../services/coverage-area.service';

const routes: Routes = [
  { path: 'list', component: CoverageAreaListComponent },
  { path: 'create', component: CoverageAreaCreateComponent },
  { path: 'edit/:id', component: CoverageAreaEditComponent },
  { path: '', redirectTo: 'list', pathMatch: 'full' }
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    CoverageAreaListComponent,
    CoverageAreaCreateComponent,
    CoverageAreaEditComponent
  ],
  providers: [CoverageAreaService]
})
export class CoverageAreaModule { } 
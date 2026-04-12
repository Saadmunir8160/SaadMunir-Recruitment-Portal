import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { JobApplicationsListComponent } from './job-applications-list.component';

const routes: Routes = [
  { path: 'list', component: JobApplicationsListComponent },
  { path: '', redirectTo: 'list', pathMatch: 'full' }
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    JobApplicationsListComponent
  ]
})
export class JobApplicationsModule { } 
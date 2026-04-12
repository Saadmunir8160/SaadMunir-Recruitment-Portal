import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NewsListComponent } from './news-list/news-list.component';
import { NewsCreateComponent } from './news-create/news-create.component';
import { NewsViewComponent } from './news-view/news-view.component';
import { NewsEditComponent } from './news-edit/news-edit.component';

const routes: Routes = [
  { path: '', component: NewsListComponent },
  { path: 'create', component: NewsCreateComponent },
  { path: 'view/:id', component: NewsViewComponent },
  { path: 'edit/:id', component: NewsEditComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NewsRoutingModule { }

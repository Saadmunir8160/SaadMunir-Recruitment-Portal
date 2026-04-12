import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DealerRoutingModule } from './dealer-routing.module';
import { 
  DealerAuthService,
  DealerDashboardService,
  DealerProfileService,
  DealerSupportService
} from './services';
import { DealerTranslationService } from './services/translation.service';

@NgModule({
  imports: [
    CommonModule,
    DealerRoutingModule
  ],
  providers: [
    DealerAuthService,
    DealerDashboardService,
    DealerProfileService,
    DealerSupportService,
    DealerTranslationService
  ]
})
export class DealerModule { }

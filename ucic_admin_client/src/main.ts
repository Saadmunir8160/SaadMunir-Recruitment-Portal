import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

// Import Iconify for icon support
import '@iconify/iconify';

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));

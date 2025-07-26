import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { SimpleAppComponent } from './app/app-simple';

bootstrapApplication(SimpleAppComponent, appConfig)
  .catch((err) => console.error(err));

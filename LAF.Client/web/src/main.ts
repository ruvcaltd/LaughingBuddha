import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community';
import { importProvidersFrom } from '@angular/core';
import { DxDataGridModule } from 'devextreme-angular';

bootstrapApplication(App,appConfig)
  .catch((err) => console.error(err));

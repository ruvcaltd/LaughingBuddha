import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'position-view',
    loadComponent: () => import('./pages/position-view/position-view').then(m => m.PositionsView),
    canActivate: [AuthGuard]
  },
  {
    path: 'repo-rates',
    loadComponent: () => import('./pages/repo-rates/repo-rates').then(m => m.RepoRates),
    canActivate: [AuthGuard]
  },
  {
    path: 'submitted-trades',
    loadComponent: () => import('./pages/submitted-trades/submitted-trades').then(m => m.SubmittedTrades),
    canActivate: [AuthGuard]
  },
  {
    path: 'order-basket',
    loadComponent: () => import('./pages/order-basket/order-basket').then(m => m.OrderBasket),
    canActivate: [AuthGuard]
  },
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  }
];

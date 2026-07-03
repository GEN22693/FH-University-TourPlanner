import { Routes } from '@angular/router';

import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Tourlist } from './pages/tourlist/tourlist';
import { TourDetail } from './pages/tour-detail/tour-detail';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login,
  },
  {
    path: 'register',
    component: Register,
  },
  {
    path: 'tours',
    component: Tourlist,
    canActivate: [authGuard],
  },
  {
    path: 'tours/:id',
    component: TourDetail,
    canActivate: [authGuard],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

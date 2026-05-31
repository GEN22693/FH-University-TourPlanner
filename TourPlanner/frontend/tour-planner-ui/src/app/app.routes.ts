import { Routes } from '@angular/router';

import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Tourlist } from './pages/tourlist/tourlist';
import { TourDetail } from './pages/tour-detail/tour-detail';

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
  },
  {
    path: 'tours/:id',
    component: TourDetail,
  },
];

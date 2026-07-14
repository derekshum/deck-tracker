import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'games',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./games/game-list/game-list.component').then(m => m.GameListComponent)
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./games/game-form/game-form.component').then(m => m.GameFormComponent)
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./games/game-form/game-form.component').then(m => m.GameFormComponent)
      }
    ]
  },
  {
    path: '',
    redirectTo: 'games',
    pathMatch: 'full'
  }
];

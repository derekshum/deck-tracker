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
    },
    {
      path: ':gameId/card-types',
      children: [
        {
          path: '',
          loadComponent: () =>
            import('./games/card-types/card-type-list/card-type-list.component').then(m => m.CardTypeListComponent)
        },
        {
          path: 'new',
          loadComponent: () =>
            import('./games/card-types/card-type-form/card-type-form.component').then(m => m.CardTypeFormComponent)
        },
        {
          path: ':cardTypeId/edit',
          loadComponent: () =>
            import('./games/card-types/card-type-form/card-type-form.component').then(m => m.CardTypeFormComponent)
        }
      ]
    }
  ]
},
  {
    path: '',
    redirectTo: 'games',
    pathMatch: 'full'
  }
];

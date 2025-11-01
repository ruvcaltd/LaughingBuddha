import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { CollateralTypeDto, CollateralTypesClient } from '../../api/client';

export const CollateralTypeStore = signalStore(
  { providedIn: 'root' },
  withState({
    collateralTypes: [] as CollateralTypeDto[],
  }),
  withMethods((store, client = inject(CollateralTypesClient)) => ({
    async loadAll() {
      const data = await firstValueFrom(client.collateralTypes());
      patchState(store, { collateralTypes: data });
    },
  })),
);

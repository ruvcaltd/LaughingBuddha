import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { CounterpartiesClient, CounterpartyDto } from '../../api/client';

export const CounterpartyStore = signalStore(
  { providedIn: 'root' },
  withState({ counterparties: [] as CounterpartyDto[] }),
  withMethods((store, client = inject(CounterpartiesClient)) => ({
    async loadAll() {
      const data = await firstValueFrom(client.counterparties());
      patchState(store, { counterparties: data });
    },
  })),
);

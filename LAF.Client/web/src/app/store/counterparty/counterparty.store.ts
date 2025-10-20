import { CounterpartiesClient } from './../../api/client';
import { inject, Injectable, signal } from '@angular/core';
import { CounterpartyDto } from '../../api/client';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CounterpartyStore {
    private client = inject(CounterpartiesClient)
  private readonly _counterparties = signal<CounterpartyDto[]>([]);
  readonly counterparties = this._counterparties.asReadonly();

  async loadAll() {
    const data = await firstValueFrom(this.client.counterpartiesAll());
    this._counterparties.set(data);
  }
}

import { inject, Injectable, signal } from '@angular/core';
import { CollateralTypeDto, CollateralTypesClient } from '../../api/client';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CollateralTypeStore {
  private client = inject(CollateralTypesClient)
  private readonly _collateralTypes = signal<CollateralTypeDto[]>([]);
  readonly collateralTypes = this._collateralTypes.asReadonly();

  async loadAll() {
    const data = await firstValueFrom(this.client.collateralTypesAll());
    this._collateralTypes.set(data);
  }
}

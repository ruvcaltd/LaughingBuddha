import { inject, Injectable, signal } from '@angular/core';
import { FundDto, FundsClient } from '../../api/client';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FundStore {
  private client = inject(FundsClient)
  private readonly _funds = signal<FundDto[]>([]);
  readonly funds = this._funds.asReadonly();

  async loadAll() {
    const data = await firstValueFrom(this.client.active3());
    this._funds.set(data);
  }
}
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { IRepoRateDto } from '../../api/client';

export interface RepoRatesState {
  rates: IRepoRateDto[];
  loading: boolean;
  error: string | null;
  selectedDate: Date | null;
}

const initialState: RepoRatesState = {
  rates: [],
  loading: false,
  error: null,
  selectedDate: null,
};

export const RepoRatesStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('repo-ates-store'), // enables DevTools connection
  withComputed(({ rates, loading, selectedDate }) => ({
    rateCount: computed(() => rates().length),
    hasRates: computed(() => rates().length > 0),
    isLoading: computed(() => loading()),
    currentDate: computed(() => selectedDate() || new Date()),
  })),
  withMethods((state) => ({
    setRates(rates: IRepoRateDto[]) {
      patchState(state, { rates });
    },
    update(payload: IRepoRateDto) {
      const updatedRates = state.rates().map((rate) => {
        if (
          rate.counterpartyId === payload.counterpartyId &&
          rate.collateralTypeId === payload.collateralTypeId &&
          new Date(rate.repoDate!)?.toDateString() === new Date(payload.repoDate!)?.toDateString()
        ) {
          return { ...rate, ...payload };
        }
        return rate;
      });
      patchState(state, { rates: updatedRates });
    },
    setLoading(loading: boolean) {
      patchState(state, { loading });
    },
    setError(error: string | null) {
      patchState(state, { error });
    },
    setSelectedDate(selectedDate: Date | null) {
      patchState(state, { selectedDate });
    },
    addToCircle(counterpartyId: number, collateralTypeId: number, notionalAmount: number) {
      const updatedRates = state.rates().map((rate) => {
        if (rate.counterpartyId === counterpartyId && rate.collateralTypeId === collateralTypeId) {
          const newBal = (rate.finalCircle || 0) + Math.abs(notionalAmount);
          return { ...rate, finalCircle: newBal };
        }
        return rate;
      });
      patchState(state, { rates: updatedRates });
    },
  })),
);

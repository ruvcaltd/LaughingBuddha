import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { IRepoRateDto, RepoRateDto } from '../../api/client';
import { withDevtools } from '@angular-architects/ngrx-toolkit';



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
  selectedDate: null
};

export const RepoRatesStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('repo-ates-store'), // enables DevTools connection
  withComputed(({ rates, loading, selectedDate }) => ({
    rateCount: computed(() => rates().length),
    hasRates: computed(() => rates().length > 0),
    isLoading: computed(() => loading()),
    currentDate: computed(() => selectedDate() || new Date())
  })),
  withMethods((state) => ({
    setRates(rates: IRepoRateDto[]) {
      patchState(state, { rates });
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
      const updatedRates = state.rates().map(rate => {
        if (rate.counterpartyId === counterpartyId && rate.collateralTypeId === collateralTypeId) {
          const newBal = (rate.finalCircle || 0) * 1000000 + Math.abs(notionalAmount);
          return { ...rate, finalCircle: newBal/1000000 };
        }
        return rate;
      });
      patchState(state, { rates: updatedRates }); 
    }
  }))
);
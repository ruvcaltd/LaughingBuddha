import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { IRepoTradeDto } from '../../api/client';
import { withDevtools } from '@angular-architects/ngrx-toolkit';

export interface TradesState {
  trades: IRepoTradeDto[];
  loading: boolean;
  error: string | null;
  selectedDate: Date | null;
}

const initialState: TradesState = {
  trades: [],
  loading: false,
  error: null,
  selectedDate: null
};

export const TradesStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('trades-store'),
  withComputed(({ trades, loading, selectedDate }) => ({
    tradeCount: computed(() => trades().length),
    hasTrades: computed(() => trades().length > 0),
    isLoading: computed(() => loading()),
    currentDate: computed(() => selectedDate() || new Date())
  })),
  withMethods((state) => ({
    setTrades(trades: IRepoTradeDto[]) {
      patchState(state, { trades });
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
    addTrade(trade: IRepoTradeDto) {
      patchState(state, { trades: [...state.trades(), trade] });
    },
    updateTrade(updatedTrade: IRepoTradeDto) {
      const trades = state.trades().map(trade => 
        trade.id === updatedTrade.id ? updatedTrade : trade
      );
      patchState(state, { trades });
    },
    removeTrade(tradeId: number) {
      const trades = state.trades().filter(trade => trade.id !== tradeId);
      patchState(state, { trades });
    }
  }))
);
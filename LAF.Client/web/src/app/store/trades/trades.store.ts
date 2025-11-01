import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { IRepoTradeDto, RepoTradesClient } from '../../api/client';

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
  selectedDate: null,
};

export const TradesStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('trades-store'),
  withComputed(({ trades, loading, selectedDate }) => ({
    tradeCount: computed(() => trades().length),
    hasTrades: computed(() => trades().length > 0),
    isLoading: computed(() => loading()),
    currentDate: computed(() => selectedDate() || new Date()),
  })),
  withMethods((state, http = inject(RepoTradesClient)) => ({
    loadTrades(fromDate: Date | null, toDate: Date | null, status: string | null) {
      patchState(state, { loading: true });
      http
        .search(
          undefined,
          undefined,
          undefined,
          fromDate ?? undefined,
          toDate ?? undefined,
          undefined,
          status ?? undefined,
          undefined,
        )
        .subscribe({
          next: (trades) => patchState(state, { trades, loading: false }),
          error: (error) => patchState(state, { error: error.message, loading: false }),
        });
    },
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
      const trades = state
        .trades()
        .map((trade) => (trade.id === updatedTrade.id ? updatedTrade : trade));
      patchState(state, { trades });
    },
    removeTrade(tradeId: number) {
      const trades = state.trades().filter((trade) => trade.id !== tradeId);
      patchState(state, { trades });
    },
  })),
);

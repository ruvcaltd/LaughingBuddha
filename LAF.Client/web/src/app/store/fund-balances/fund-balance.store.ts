import { computed } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { IFundBalanceDto } from '../../api/client';

export interface FundBalancesState {
  balances: IFundBalanceDto[];
  loading: boolean;
  error: string | null;
  asOfDate: Date | null;
}

const initialState: FundBalancesState = {
  balances: [],
  loading: false,
  error: null,
  asOfDate: null
};

export const FundBalanceStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('fund-balance-store'),
  withComputed(({ balances, loading, asOfDate }) => ({
    balanceCount: computed(() => balances().length),
    hasBalances: computed(() => balances().length > 0),
    isLoading: computed(() => loading()),
    currentAsOfDate: computed(() => asOfDate() || null),
    // Fast lookup map by fundId for component accessors
    byFundId: computed(() => {
      const map = new Map<number, IFundBalanceDto>();
      for (const b of balances()) {
        if (b.fundId != null) {
          map.set(b.fundId, b);
        }
      }
      return map;
    })
  })),
  withMethods((state) => ({
    setBalances(balances: IFundBalanceDto[]) {
      patchState(state, { balances });
    },
    setLoading(loading: boolean) {
      patchState(state, { loading });
    },
    setError(error: string | null) {
      patchState(state, { error });
    },
    setAsOfDate(asOfDate: Date | null) {
      patchState(state, { asOfDate });
    },
    reduceBalanceBalances(fundId: number, notionalAmount: number) {
      const updatedBalances = state.balances().map(balance => {
        if (balance.fundId === fundId) {
          const newBal = (balance.availableCash || 0) - Math.abs(notionalAmount);
          return { ...balance, availableCash: newBal };
        }
        return balance;
      });
      patchState(state, { balances: updatedBalances });
    },
    upsertBalance(balance: IFundBalanceDto) {
      const curr = state.balances();
      const id = balance.fundId;
      if (id == null) {
        // If no id, append immutably
        patchState(state, { balances: [...curr, balance] });
        return;
      }
      const exists = curr.some(b => b.fundId === id);
      if (!exists) {
        patchState(state, { balances: [...curr, balance] });
      } else {
        patchState(state, {
          balances: curr.map(b => (b.fundId === id ? { ...b, ...balance } : b))
        });
      }
    },
    clear() {
      patchState(state, { balances: [], loading: false, error: null });
    }
  }))
);
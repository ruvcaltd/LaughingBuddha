import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { withDevtools } from '@angular-architects/ngrx-toolkit';

export interface DraftTrade {
  id: string;
  securityId?: number;
  securityIsin?: string;
  securityName?: string;
  collateralTypeId?: number;
  counterpartyId?: number;
  notional: number;
  direction: 'Buy' | 'Sell';
  fundId?: number;
  fundCode?: string;
  fundName?: string;
  rate?: number;
  tradeDate?: Date;
  status: 'Draft' | 'Pending' | 'Submitted';
  createdAt: Date;
  validationWarnings?: string[];
}

export interface OrderBasketState {
  draftTrades: DraftTrade[];
  loading: boolean;
  error: string | null;
  totalValue: number;
}

const initialState: OrderBasketState = {
  draftTrades: [],
  loading: false,
  error: null,
  totalValue: 0
};

export const OrderBasketStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('order-basket-store'),
  withComputed(({ draftTrades, loading, totalValue }) => ({
    draftTradeCount: computed(() => draftTrades().length),
    hasDraftTrades: computed(() => draftTrades().length > 0),
    isLoading: computed(() => loading()),
    basketTotal: computed(() => totalValue()),
    totalBuyNotional: computed(() => 
      draftTrades()
        .filter(trade => trade.direction === 'Buy')
        .reduce((sum, trade) => sum + (trade.notional || 0), 0)
    ),
    totalSellNotional: computed(() => 
      draftTrades()
        .filter(trade => trade.direction === 'Sell')
        .reduce((sum, trade) => sum + (trade.notional || 0), 0)
    )
  })),
  withMethods((state) => ({
    addDraftTrade(draftTrade: DraftTrade): void {
      const updatedTrades = [...state.draftTrades(), draftTrade];
      const totalValue = updatedTrades.reduce((sum, trade) => sum + (trade.notional || 0), 0);
      patchState(state, { 
        draftTrades: updatedTrades,
        totalValue 
      });
    },

    // implement addOrUpdateDraftTrade based on fundcode, counterpartyid and collateraltypeid
    addOrUpdateDraftTrade(draftTrade: DraftTrade): void {
      const existingIndex = state.draftTrades().findIndex(trade => 
        trade.fundCode === draftTrade.fundCode &&
        trade.counterpartyId === draftTrade.counterpartyId &&
        trade.collateralTypeId === draftTrade.collateralTypeId
      );
      let updatedTrades: DraftTrade[];
      if (existingIndex >= 0) {
        updatedTrades = state.draftTrades().map((trade, index) => {
          if (index === existingIndex) {
            return { ...trade, ...draftTrade };
          }
          return trade;
        });
      } else {
        updatedTrades = [...state.draftTrades(), draftTrade];
      }
      const totalValue = updatedTrades.reduce((sum, trade) => sum + (trade.notional || 0), 0);
      patchState(state, { draftTrades: updatedTrades, totalValue });
    },

    removeDraftTrade(id: string): void {
      const filteredTrades = state.draftTrades().filter(trade => trade.id !== id);
      const totalValue = filteredTrades.reduce((sum, trade) => sum + (trade.notional || 0), 0);
      patchState(state, { 
        draftTrades: filteredTrades,
        totalValue 
      });
    },
    updateDraftTrade(id: string, updates: Partial<DraftTrade>): void {
      const updatedTrades = state.draftTrades().map(trade => 
        trade.id === id ? { ...trade, ...updates } : trade
      );
      const totalValue = updatedTrades.reduce((sum, trade) => sum + (trade.notional || 0), 0);
      patchState(state, { 
        draftTrades: updatedTrades,
        totalValue 
      });
    },
    clearBasket(): void {
      patchState(state, { draftTrades: [], totalValue: 0 });
    },
    submitBasket(): void {
      patchState(state, { loading: true });
      // TODO: Implement basket submission logic
      patchState(state, { loading: false, draftTrades: [], totalValue: 0 });
    },
    // Legacy methods for backward compatibility
    addOrder(order: any): void {
      this.addDraftTrade(order as DraftTrade);
    },
    removeOrder(id: string): void {
      this.removeDraftTrade(id);
    },
    updateOrderQuantity(id: string, quantity: number): void {
      this.updateDraftTrade(id, { notional: quantity });
    }
  }))
);
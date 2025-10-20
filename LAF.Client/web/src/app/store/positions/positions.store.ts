import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { IRepoTradeDto } from '../../api/client';
import { withDevtools } from '@angular-architects/ngrx-toolkit';


export interface ITradeBlotterGridItem extends IRepoTradeDto {
  variance?: number;
  fundNotionals?: { [fundId: number]: number };
  fundExposurePercents?: { [fundId: number]: number };
  fundStatuses?: { [fundId: string]: string };
}

export interface PositionsState {
  positions: ITradeBlotterGridItem[];
  loading: boolean;
  error: string | null;
  selectedDate: Date | null;    
}

const initialState: PositionsState = {
  positions: [],
  loading: false,
  error: null,
  selectedDate: null
};

export const PositionsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('positions-store'),
  withComputed(({ positions, loading, selectedDate }) => ({
    positionCount: computed(() => positions().length),
    hasPositions: computed(() => positions().length > 0),
    isLoading: computed(() => loading()),
    currentDate: computed(() => selectedDate() || new Date())
  })),
  withMethods((state) => ({
    setPositions(positions: IRepoTradeDto[]) {
      patchState(state, { positions });
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
    addPosition(trade: IRepoTradeDto) {
      patchState(state, { positions: [...state.positions(), trade] });
    },
    updatePosition(updatedPositionRow: IRepoTradeDto) {
      const positions = state.positions().map(x => 
        x.collateralTypeId === updatedPositionRow.collateralTypeId && x.counterpartyId === updatedPositionRow.counterpartyId  && x.fundCode === updatedPositionRow.fundCode ? updatedPositionRow : x
      );
      patchState(state, { positions });
    },
    addOrUpatePosition(trade: IRepoTradeDto) {
      const index = state.positions().findIndex(x => 
        x.collateralTypeId === trade.collateralTypeId && x.counterpartyId === trade.counterpartyId  && x.fundCode === trade.fundCode
      );
      if (index !== -1) {        
        const positions = state.positions().map((x, i) => i === index ? trade : x);
        patchState(state, { positions });
      } else {        
        patchState(state, { positions: [...state.positions(), trade] });
      }
    },
    removePosition(tradeId: number) {
      const positions = state.positions().filter(trade => trade.id !== tradeId);
      patchState(state, { positions });
    }
  }))
);
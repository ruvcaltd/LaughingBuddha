import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { IPositionDto, IPositionLockDto } from '../../api/client';

export interface ITradeBlotterGridItem {
  variance?: number;
  fundNotionals?: { [fundId: number]: number };
  fundExposurePercents?: { [fundId: number]: number };
  fundStatuses?: { [fundId: string]: string };
  locked?: { [fundId: string]: boolean };
  lockedBy?: { [fundId: string]: string };
  error?: { [fundId: string]: string | undefined };
  counterpartyId?: number;
  securityId?: number;
  securityName?: string | undefined;
  collateralTypeId?: number;
  collateralTypeName?: string | undefined;
  rate?: number;
  startDate?: Date;
  endDate?: Date;
  settlementDate?: Date;
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
  selectedDate: null,
};

export const PositionsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withDevtools('positions-store'),
  withComputed(({ positions, loading, selectedDate }) => ({
    positionCount: computed(() => positions().length),
    hasPositions: computed(() => positions().length > 0),
    isLoading: computed(() => loading()),
    currentDate: computed(() => selectedDate() || new Date()),
  })),
  withMethods((state) => ({
    setPositions(positions: IPositionDto[]) {
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
    addPosition(trade: IPositionDto) {
      patchState(state, { positions: [...state.positions(), trade] });
    },
    updateError(trade: IPositionDto, fundId: string, errorMsg: string | undefined) {
      const positions = state.positions().map((pos) => {
        if (
          pos.collateralTypeId === trade.collateralTypeId &&
          pos.counterpartyId === trade.counterpartyId
        ) {
          pos.error![fundId] = errorMsg;
        }
        return pos;
      });
      patchState(state, { positions });
    },
    lockPosition(lock: IPositionLockDto) {
      const positions = state.positions().map((pos) => {
        if (
          pos.collateralTypeId === lock.collateralTypeId &&
          pos.counterpartyId === lock.counterpartyId
        ) {
          if (pos.locked) {
            pos.locked[lock.fundId!] = lock.locked ?? false;
            if (pos.lockedBy) {
              pos.lockedBy[lock.fundId!] = lock.userDisplay ?? '';
            }
          }
        }
        return pos;
      });
      patchState(state, { positions });
    },
    unlockPosition(lock: IPositionLockDto) {
      const positions = state.positions().map((pos) => {
        if (
          pos.collateralTypeId === lock.collateralTypeId &&
          pos.counterpartyId === lock.counterpartyId
        ) {
          if (pos.locked) pos.locked[lock.fundId!] = false;
          if (pos.lockedBy) {
            pos.lockedBy[lock.fundId!] = '';
          }
        }
        return pos;
      });
      patchState(state, { positions });
    },
  })),
);

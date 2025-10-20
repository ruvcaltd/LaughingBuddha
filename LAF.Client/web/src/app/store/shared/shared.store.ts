import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { computed } from '@angular/core';

export interface SharedState {
  currentUser: any | null;
  sidebarCollapsed: boolean;
  theme: 'light' | 'dark';
  notifications: any[];
}

const initialState: SharedState = {
  currentUser: null,
  sidebarCollapsed: false,
  theme: 'light',
  notifications: []
};

export const SharedStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ currentUser, sidebarCollapsed, theme, notifications }) => ({
    isAuthenticated: computed(() => !!currentUser()),
    isSidebarCollapsed: computed(() => sidebarCollapsed()),
    currentTheme: computed(() => theme()),
    notificationCount: computed(() => notifications().length),
    hasNotifications: computed(() => notifications().length > 0)
  })),
  withMethods((state) => ({
    setCurrentUser(user: any): void {
      patchState(state, { currentUser: user });
    },
    toggleSidebar(): void {
      patchState(state, { sidebarCollapsed: !state.sidebarCollapsed() });
    },
    setTheme(theme: 'light' | 'dark'): void {
      patchState(state, { theme });
    },
    addNotification(notification: any): void {
      patchState(state, { notifications: [...state.notifications(), notification] });
    },
    clearNotifications(): void {
      patchState(state, { notifications: [] });
    }
  }))
);
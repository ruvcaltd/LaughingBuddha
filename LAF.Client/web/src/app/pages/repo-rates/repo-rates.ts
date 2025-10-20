import { Component, OnInit, inject, ChangeDetectorRef, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  DxDataGridModule,
  DxDropDownBoxComponent,
  DxTemplateHost,
  DxTemplateModule,
} from 'devextreme-angular';
import {
  RepoRateDto,
  CreateRepoRateDto,
  UpdateRepoRateDto,
  CounterpartyDto,
  CollateralTypeDto,
  IRepoRateDto,
  AuthClient,
  RepoRatesClient,
} from '../../api/client';
import { RepoRatesStore } from '../../store/repo-rates/repo-rates.store';
import { SharedStore } from '../../store/shared/shared.store';
import { CounterpartyStore } from '../../store/counterparty/counterparty.store';
import { CollateralTypeStore } from '../../store/collateral-type/collateral-type.store';
import { Navbar } from '../../shared/navbar/navbar';
import { ThemeService } from '../../services/theme.service';
import { SelectionChangedEvent } from 'devextreme/ui/data_grid';
import { firstValueFrom } from 'rxjs';

export interface IRepoRateGridItem extends IRepoRateDto {
  previousDayCircle?: number;
  variance?: number;
}

@Component({
  selector: 'app-repo-rates',
  imports: [
    CommonModule,
    DxDataGridModule,
    Navbar,
    DxDropDownBoxComponent,
    DxTemplateModule,
    DxTemplateModule,
  ],
  providers: [DxTemplateHost],
  templateUrl: './repo-rates.html',
})
export class RepoRates implements OnInit {
  private repoRateApiClient = inject(RepoRatesClient);

  private repoRatesStore = inject(RepoRatesStore);
  private sharedStore = inject(SharedStore);
  private themeService = inject(ThemeService);
  private cdr = inject(ChangeDetectorRef);

  public gridOptions = {
    editing: {
      mode: 'batch',
      allowUpdating: true,
      allowAdding: true,
      allowDeleting: false,
      useIcons: true,
    },
    paging: { enabled: true, pageSize: 50 },
    pager: { visible: true, showPageSizeSelector: true, allowedPageSizes: [10, 25, 50, 100] },
    filterRow: { visible: true },
    headerFilter: { visible: true },
    rowAlternationEnabled: true,
    showBorders: true,
    columnAutoWidth: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: 'widget',
  };

  private counterpartyStore = inject(CounterpartyStore);
  private collateralTypeStore = inject(CollateralTypeStore);
  public counterpartyGridBoxOpened = false;
  public collateralTypeGridBoxOpened = false;

  // Signals from store
  rowData = this.repoRatesStore.rates;
  loading = this.repoRatesStore.loading;
  error = this.repoRatesStore.error;
  selectedDate = this.repoRatesStore.selectedDate;
  counterparties = this.counterpartyStore.counterparties;
  collateralTypes = this.collateralTypeStore.collateralTypes;

  gridTheme = computed(() =>
    this.isDark ? 'dx.material.blue.compact.dark' : 'dx.material.blue.compact.light'
  );
  get isDark(): boolean {
    return this.themeService.isDark();
  }

  async ngOnInit(): Promise<void> {
    this.repoRatesStore.setSelectedDate(new Date());
    await Promise.all([this.counterpartyStore.loadAll(), this.collateralTypeStore.loadAll()]);
    await this.loadData();
  }

  public getActiveRatesCount(): number {
    return this.rowData().filter((r) => r.active !== false).length;
  }

  public getAverageTargetCircle(): string {
    if (this.rowData().length === 0) return '$0.0M';
    const sum = this.rowData().reduce((sum, r) => sum + (r.targetCircle || 0), 0);
    const avg = sum / this.rowData().length / 1000000;
    return `$${avg.toFixed(1)}M`;
  }

  async loadData(): Promise<void> {
    this.repoRatesStore.setError(null);
    this.repoRatesStore.setLoading(true);
    try {
      // Load repo rates for selected date if store doesn't have any rates yet
      await this.loadRepoRatesForDate(this.selectedDate() ?? new Date());
    } catch (error) {
      console.error('Error loading data:', error);
      this.repoRatesStore.setError(
        'Failed to load repo rates. Please check your connection and try again.'
      );
    } finally {
      this.repoRatesStore.setLoading(false);
    }
  }

  async loadRepoRatesForDate(date: Date): Promise<void> {
    try {
      if (
        this.repoRatesStore
          .rates()
          .filter(
            (r) => r.repoDate?.toDateString() === (this.selectedDate() ?? new Date()).toDateString()
          ).length === 0
      ) {
        const repoRates = (await firstValueFrom(
          this.repoRateApiClient.date(date)
        )) as IRepoRateGridItem[];
        const repoRatesPreviousDay = await firstValueFrom(this.repoRateApiClient.previousDay(date));
        for (const rate of repoRates) {
          rate.variance = rate.finalCircle! - rate.targetCircle!;
          const prev = repoRatesPreviousDay.find(
            (r) =>
              r.collateralTypeId === rate.collateralTypeId &&
              r.counterpartyId === rate.counterpartyId
          );
          rate.previousDayCircle = prev?.finalCircle;
        }

        this.repoRatesStore.setRates(repoRates);
      }
    } catch (error) {
      console.error('Error loading repo rates:', error);
    }
  }

  async onSaved(): Promise<void> {
    const changes = this.rowData();
    for (const change of changes) {
      await this.saveRow(change, false);
    }
    await this.loadRepoRatesForDate(this.selectedDate() ?? new Date());
  }

  async toggleActive(row: any) {
    try {
      row.active = !row.active;
      if (row.active) await this.repoRateApiClient.setActive(row.id);
      else await this.repoRateApiClient.setInactive(row.id);
    } catch (error) {
      console.error('Error toggling active state:', error);
    }
  }

  async saveRow(data: IRepoRateGridItem, reloadAfter: boolean): Promise<void> {
    try {
      if (data.id && data.id > 0) {
        // Update existing rate
        const updateDto = new UpdateRepoRateDto({
          id: data.id,
          collateralTypeId: data.collateralTypeId,
          counterpartyId: data.counterpartyId,
          repoRate: data.repoRate || 0,
          targetCircle: data.targetCircle || 0,
          finalCircle: data.finalCircle || 0,
          active: data.active,
          modifiedByUserId: this.sharedStore.currentUser()?.id || 1,
        });
        await this.repoRateApiClient.repoRatesPUT(data.id, updateDto);
      } else {
        // Create new rate
        const createDto = new CreateRepoRateDto({
          repoRate: data.repoRate || 0,
          targetCircle: data.targetCircle || 0,
          finalCircle: data.finalCircle || 0,
          active: data.active,
          collateralTypeId: data.collateralTypeId,
          counterpartyId: data.counterpartyId,
          createdByUserId: this.sharedStore.currentUser()?.id || 1,
          repoDate: this.selectedDate() ?? new Date(),
        });
        await this.repoRateApiClient.repoRatesPOST(createDto);
      }
      if (reloadAfter) await this.loadRepoRatesForDate(this.selectedDate() ?? new Date());
    } catch (error) {
      console.error('Error saving repo rate:', error);
    }
  }

  onSelectionChanged($event: SelectionChangedEvent, cellInfo: any, dropDownBoxComponent: any) {
    const selectedRowKeys: number[] = $event.selectedRowKeys;
    cellInfo.setValue(selectedRowKeys[0]);
    if (selectedRowKeys.length > 0) {
      dropDownBoxComponent.close();
    }
  }

  // Date change handler updates store
  public onDateChange(date: Date) {
    this.repoRatesStore.setSelectedDate(date);
    this.loadData();
  }

  public async onNewDay() {
    this.repoRatesStore.setRates([]);
    this.repoRatesStore.setLoading(true);
    this.repoRatesStore.setError(null);

    try {
      const selectedDate = this.selectedDate() ?? new Date();
      let repoRates = (await firstValueFrom(
        this.repoRateApiClient.date(selectedDate)
      )) as IRepoRateGridItem[];
      const repoRatesPreviousDay = await firstValueFrom(
        this.repoRateApiClient.previousDay(selectedDate)
      );
      const newRates: IRepoRateGridItem[] = [];
      for (const rate of repoRatesPreviousDay) {
        const prev = repoRates.find(
          (r) =>
            r.collateralTypeId === rate.collateralTypeId && r.counterpartyId === rate.counterpartyId
        );
        if (prev) continue;
        // clone rate to newRate
        const newRate = { ...rate, id: undefined, repoDate: selectedDate };
        newRates.push(newRate);
      }
      this.repoRatesStore.setRates(newRates);
      await this.onSaved();
      await this.loadRepoRatesForDate(selectedDate);
    } catch (error) {
      console.error('Error loading repo rates:', error);
    } finally {
      this.repoRatesStore.setLoading(false);
    }
  }

  // New row handler (example: add empty row)
  public onNewRow() {
    const newRow = new RepoRateDto({
      id: undefined,
      repoRate: 0,
      targetCircle: 0,
      finalCircle: 0,
      active: true,
      collateralTypeId: undefined,
      counterpartyId: undefined,
    });
    this.repoRatesStore.setRates([...this.rowData(), newRow]);
  }

  getSelectedRowKeys<T>(value: T): T[] {
    return value !== null && value !== undefined ? [value] : [];
  }
}

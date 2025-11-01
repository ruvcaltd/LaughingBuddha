import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, Input, OnInit, computed, inject } from '@angular/core';
import {
  DxDataGridModule,
  DxDropDownBoxComponent,
  DxTemplateHost,
  DxTemplateModule,
} from 'devextreme-angular';
import { SelectionChangedEvent } from 'devextreme/ui/data_grid';
import { firstValueFrom } from 'rxjs';
import {
  CreateRepoRateDto,
  FundsClient,
  IRepoRateDto,
  RepoRateDto,
  RepoRatesClient,
  UpdateRepoRateDto,
} from '../../api/client';
import { ThemeService } from '../../services/theme.service';
import { ToastService } from '../../services/toast.service';
import { ToastComponent } from '../../shared/toast/toast.component';
import { CollateralTypeStore } from '../../store/collateral-type/collateral-type.store';
import { CounterpartyStore } from '../../store/counterparty/counterparty.store';
import { RepoRatesStore } from '../../store/repo-rates/repo-rates.store';
import { SharedStore } from '../../store/shared/shared.store';

export interface IRepoRateGridItem extends IRepoRateDto {
  previousDayCircle?: number;
  variance?: number;
}

@Component({
  selector: 'app-repo-rates',
  imports: [
    CommonModule,
    DxDataGridModule,
    DxDropDownBoxComponent,
    DxTemplateModule,
    DxTemplateModule,
    ToastComponent,
  ],
  providers: [DxTemplateHost],
  templateUrl: './repo-rates.html',
})
export class RepoRates implements OnInit {
  private repoRateApiClient = inject(RepoRatesClient);
  private fundsApiClient = inject(FundsClient);
  private repoRatesStore = inject(RepoRatesStore);

  private sharedStore = inject(SharedStore);
  private themeService = inject(ThemeService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  @Input() embeddedView: boolean = false;

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
    this.isDark ? 'dx.material.blue.compact.dark' : 'dx.material.blue.compact.light',
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
    const avg = sum / this.rowData().length;
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
        'Failed to load repo rates. Please check your connection and try again.',
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
            (r) =>
              r.repoDate?.toDateString() === (this.selectedDate() ?? new Date()).toDateString(),
          ).length === 0
      ) {
        const repoRates = (await firstValueFrom(
          this.repoRateApiClient.date(date),
        )) as IRepoRateGridItem[];
        const repoRatesPreviousDay = await firstValueFrom(this.repoRateApiClient.previousDay(date));
        for (const rate of repoRates) {
          rate.variance = rate.finalCircle! - rate.targetCircle!;
          const prev = repoRatesPreviousDay.find(
            (r) =>
              r.collateralTypeId === rate.collateralTypeId &&
              r.counterpartyId === rate.counterpartyId,
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
    this.cdr.detectChanges();
    console.log('All changes have been saved.');
  }

  async toggleActive(row: any) {
    try {
      row.active = !row.active;
      if (row.active) await firstValueFrom(this.repoRateApiClient.setActive(row.id));
      else await firstValueFrom(this.repoRateApiClient.setInactive(row.id));
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
        const updated = await await firstValueFrom(
          this.repoRateApiClient.repoRatesPUT(data.id, updateDto),
        );
        if (updated) {
          this.toastService.showSuccess(
            `Repo rate ${data.counterpartyName} (${data.collateralTypeName}) has been updated successfully.`,
          );
        }
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
        await firstValueFrom(this.repoRateApiClient.repoRatesPOST(createDto));

        this.toastService.showSuccess(
          `Repo rate ${data.counterpartyName} (${data.collateralTypeName}) has been created successfully.`,
        );
      }
      if (reloadAfter) await this.loadRepoRatesForDate(this.selectedDate() ?? new Date());
    } catch (error) {
      this.toastService.showError(
        `Error saving repo rate ${data.counterpartyId} (${data.collateralTypeId}): ${error}`,
      );
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

      // flatten cash accounts after new day roll
      await firstValueFrom(this.fundsApiClient.flatten(this.selectedDate() ?? new Date()));
      await this.toastService.showInfo('Cash accounts have been flattened for start of day.');

      // roll the new day on the backend
      await firstValueFrom(this.repoRateApiClient.newDay(selectedDate));
      await this.loadRepoRatesForDate(selectedDate);
      this.toastService.showSuccess('Repo rates for new day have been rolled successfully.');
    } catch (error) {
      this.toastService.showError('Failed to load repo rates for new day.');
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

  formatToMillion = {
    formatter: (value: number) => {
      if (value === undefined || value === null) return '$0';
      return `$${(value / 1000000).toFixed(2)}M`;
    },
  };
}

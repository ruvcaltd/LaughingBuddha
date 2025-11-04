import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import {
  DxDataGridComponent,
  DxDataGridModule,
  DxDropDownBoxComponent,
  DxTemplateHost,
  DxTemplateModule,
} from 'devextreme-angular';
import { CellPreparedEvent, RowUpdatedEvent, SelectionChangedEvent } from 'devextreme/ui/data_grid';
import { debounceTime, firstValueFrom, Subject } from 'rxjs';
import {
  CashflowClient,
  CreateCashflowDto,
  FundsClient,
  IPositionChangeDto,
  IPositionLockDto,
  IRepoTradeDto,
  PositionChangeDto,
  PositionsClient,
  RepoRatesClient,
} from '../../api/client';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { ToastComponent } from '../../shared/toast/toast.component';
import { CollateralTypeStore } from '../../store/collateral-type/collateral-type.store';
import { FundBalanceStore } from '../../store/fund-balances/fund-balance.store';
import { ITradeBlotterGridItem, PositionsStore } from '../../store/positions/positions.store';
import { AddCashflowDialogComponent } from '../../shared/add-cashflow-dialog/add-cashflow-dialog.component';
import { OrderBasket } from '../order-basket/order-basket';
import { RepoRates } from '../repo-rates/repo-rates';
import { PositionLockDto } from './../../api/client';
import { SignalRService } from './../../services/signalr.service';
import { CounterpartyStore } from './../../store/counterparty/counterparty.store';
import { RepoRatesStore } from './../../store/repo-rates/repo-rates.store';

@Component({
  selector: 'app-position-view',
  imports: [
    CommonModule,
    DxDataGridModule,
    ToastComponent,
    DxDropDownBoxComponent,
    DxTemplateModule,
    OrderBasket,
    RepoRates,
    AddCashflowDialogComponent,
  ],
  providers: [DxTemplateHost],
  templateUrl: './position-view.html',
})
export class PositionsView implements OnInit, OnDestroy {
  private repoRateApiClient = inject(RepoRatesClient);
  private positionsApiClient = inject(PositionsClient);
  private fundApiClient = inject(FundsClient);
  private cashflowClient = inject(CashflowClient);
  private signalRService = inject(SignalRService);
  private counterpartyStore = inject(CounterpartyStore);
  private collateralTypeStore = inject(CollateralTypeStore);
  private positionsStore = inject(PositionsStore);
  private toastService = inject(ToastService);
  fundBalStore = inject(FundBalanceStore);
  repoRateStore = inject(RepoRatesStore);
  fundIdBeingEdited?: number;
  rowBeingEdited?: ITradeBlotterGridItem;
  private cd = inject(ChangeDetectorRef);
  private authService = inject(AuthService);

  pageReloadPending = signal(false);

  // Add signal for panel visibility
  showOrderBasketPanel = signal(false);

  // Add signal for repo rates visibility
  showRepoRates = signal(this.getShowRepoRatesFromStorage());

  // Signals for cashflow dialog
  showAddCashflowPopup = signal(false);
  newCashflow = signal<CreateCashflowDto>(
    new CreateCashflowDto({
      cashAccountId: undefined,
      fundId: undefined,
      amount: undefined,
      currencyCode: undefined,
      cashflowDate: new Date(),
      description: undefined,
      source: undefined,
      createdByUserId: this.authService.getUserId() ?? undefined,
    }),
  );

  // Funds with accounts
  funds = signal<any[]>([]);

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

  public repoRateGridBoxOpened = false;
  public fundBalances = this.fundBalStore.balances();
  public selectedDate = computed(() => new Date());
  public counterparties = this.counterpartyStore.counterparties;
  public collateralTypes = this.collateralTypeStore.collateralTypes;
  public positions = this.positionsStore.positions;
  public loading = this.positionsStore.isLoading;
  public error = computed(() => this.positionsStore.error());

  private _editThrottle = new Subject<void>();

  @ViewChild('grid', { static: false }) grid?: DxDataGridComponent;

  async ngOnInit(): Promise<void> {
    await this.fundBalStore.loadAll(this.selectedDate());
    await this.counterpartyStore.loadAll();
    await this.collateralTypeStore.loadAll();
    await this.loadFunds();
    await this.loadData();
    this.subscribeToSignalR();

    this._editThrottle.pipe(debounceTime(60000)).subscribe(() => {
      this.onEditingCancelled(null);
    });
  }

  async ngOnDestroy(): Promise<void> {
    await this.onEditingCancelled(null);
    this.signalRService.off('PositionChanged');
    this.signalRService.off('NewTrade');
    this.signalRService.off('PositionCellEditing');
  }

  // Add methods to control the panel
  toggleOrderBasketPanel() {
    this.showOrderBasketPanel.set(!this.showOrderBasketPanel());
  }

  closeOrderBasketPanel() {
    this.showOrderBasketPanel.set(false);
  }

  // Methods for repo rates toggle
  private getShowRepoRatesFromStorage(): boolean {
    const stored = localStorage.getItem('showRepoRates');
    return stored ? JSON.parse(stored) : true; // default to true
  }

  toggleRepoRates() {
    this.showRepoRates.set(!this.showRepoRates());
    localStorage.setItem('showRepoRates', JSON.stringify(this.showRepoRates()));
  }

  private subscribeToSignalR(): void {
    this.signalRService.on(
      'PositionChanged',
      (change: { sender: number; payload: IPositionChangeDto }) => {
        if (this.authService.getUserId() !== change.sender) {
          this.pageReloadPending.set(true);
        }
      },
    );

    this.signalRService.on('NewTrade', (change: { sender: number; payload: IRepoTradeDto }) => {
      if (this.authService.getUserId() !== change.sender) {
        this.pageReloadPending.set(true);
      }
    });

    this.signalRService.on(
      'PositionCellEditing',
      (change: { sender: number; payload: IPositionLockDto }) => {
        if (this.authService.getUserId() === change.sender) {
          return;
        }
        this.positionsStore.lockPosition(change.payload);
        this.cd.detectChanges();
      },
    );
  }

  async loadData(): Promise<void> {
    this.positionsStore.setLoading(true);
    this.positionsStore.setError(null);
    this.pageReloadPending.set(false);

    try {
      const selectedDate = new Date();
      await Promise.all([
        this.loadRepoRates(selectedDate),
        this.loadFundBalances(selectedDate),
        this.loadPositions(selectedDate, false),
      ]);
    } catch (error) {
      console.error('Error loading data:', error);
      this.positionsStore.setError(
        'Failed to load trade data. Please check your connection and try again.',
      );
    } finally {
      this.positionsStore.setLoading(false);
    }
  }

  async loadRepoRates(date: Date): Promise<void> {
    try {
      if (this.repoRateStore.rates().length > 0) {
        return;
      }
      const repoRates = await firstValueFrom(this.repoRateApiClient.date(date));
      this.repoRateStore.setRates(repoRates);
    } catch (error) {
      console.error('Error loading repo rates:', error);
      throw error;
    }
  }

  async loadFundBalances(date: Date): Promise<void> {
    try {
      this.fundBalances = await firstValueFrom(this.fundApiClient.balances(date));
      this.fundBalStore.setBalances(this.fundBalances);
    } catch (error) {
      console.error('Error loading fund balances:', error);
      throw error;
    }
  }

  async loadFunds(): Promise<void> {
    try {
      const fundsData = await firstValueFrom(this.cashflowClient.daily(new Date()));
      this.funds.set(fundsData);
    } catch (error) {
      console.error('Error loading funds:', error);
      throw error;
    }
  }

  async loadPositions(date: Date, force: boolean): Promise<void> {
    if (!force && this.positionsStore.positions().length > 0) {
      return;
    }

    try {
      const positions = await firstValueFrom(this.positionsApiClient.day(date));
      const tradesWithExtensions: ITradeBlotterGridItem[] = positions.map((pos) => ({
        ...pos,
        variance: pos.variance,
        fundNotionals: pos.fundNotionals,
        fundExposurePercents: pos.exposurePercentages,
        fundStatuses: pos.statuses,
        rate: pos.rate,
        locked: {},
        lockedBy: {},
        error: {},
        commitStatus: {},
      }));
      this.positionsStore.setPositions(tradesWithExtensions);
    } catch (error) {
      console.error('Error loading trades:', error);
      throw error;
    }
  }

  onRowInserted(e: any): void {
    this.onRowUpdated(e);
  }

  async onRowUpdated(e: RowUpdatedEvent) {
    await firstValueFrom(
      this.positionsApiClient.broadcastLock(
        new PositionLockDto({
          fundId: this.fundIdBeingEdited!,
          counterpartyId: this.rowBeingEdited!.counterpartyId,
          collateralTypeId: this.rowBeingEdited!.collateralTypeId,
          locked: false,
        }),
      ),
    );

    const pos = e.data as ITradeBlotterGridItem;
    if (pos.fundNotionals) {
      Object.entries(pos.fundNotionals).forEach(async ([fundId, newNotional]) => {
        await this.onNotionalChanged(pos, parseInt(fundId), newNotional);
      });
    }
  }

  onSelectionChanged($event: SelectionChangedEvent, cellInfo: any, dropDownBoxComponent: any) {
    const selectedRowKeys: number[] = $event.selectedRowKeys;
    cellInfo.setValue(selectedRowKeys[0]);
    if (selectedRowKeys.length > 0) {
      dropDownBoxComponent.close();
    }
  }

  onRepoRateSelectionChanged(
    $event: SelectionChangedEvent,
    cellInfo: any,
    dropDownBoxComponent: any,
  ) {
    const selectedRowKeys: any[] = $event.selectedRowKeys;
    if (selectedRowKeys.length > 0) {
      const selectedRepoRate = this.repoRateStore
        .rates()
        .find(
          (rate) =>
            rate.collateralTypeId === selectedRowKeys[0].collateralTypeId &&
            rate.counterpartyId === selectedRowKeys[0].counterpartyId,
        );

      if (selectedRepoRate) {
        const existingPosition = this.positionsStore
          .positions()
          .find(
            (x) =>
              x.collateralTypeId == selectedRepoRate.collateralTypeId &&
              x.counterpartyId == selectedRepoRate.counterpartyId,
          );

        if (existingPosition) {
          dropDownBoxComponent.close();
          this.toastService.showWarning('This security is already assigned to a position.');
          this.cd.detectChanges();
          return;
        }

        this.positionsStore.addPosition({
          collateralTypeId: selectedRepoRate.collateralTypeId,
          counterpartyId: selectedRepoRate.counterpartyId,
          collateralTypeName: selectedRepoRate.collateralTypeName,
          counterpartyName: selectedRepoRate.counterpartyName,
          variance: selectedRepoRate.finalCircle! - selectedRepoRate.targetCircle!,
          securityName: `${selectedRepoRate.counterpartyName}${selectedRepoRate.collateralTypeName}`,
          locked: {},
          lockedBy: {},
          commitStatus: {},
        } as ITradeBlotterGridItem);
      }
      dropDownBoxComponent.close();
    }
    this.cancelEditing();
    this.cd.detectChanges();
  }

  cancelEditing() {
    if (this.grid && this.grid.instance) {
      this.grid.instance.cancelEditData();
    }
  }

  getSelectedRowKeys<T>(value: T): T[] {
    return value !== null && value !== undefined ? [value] : [];
  }

  async onNotionalChanged(
    trade: ITradeBlotterGridItem,
    fundId: number,
    newNotional: number,
  ): Promise<void> {
    try {
      console.log(
        `Notional changed for Security ${trade.securityName}, Fund ID ${fundId}: New Notional = ${newNotional}`,
      );

      const change: IPositionChangeDto = {
        fundId: fundId,
        collateralTypeId: trade.collateralTypeId,
        counterpartyId: trade.counterpartyId,
        newNotionalAmount: newNotional,
        securityMaturityDate: new Date(new Date().setDate(this.selectedDate().getDate() + 1)),
      };

      const pos = await firstValueFrom(
        this.positionsApiClient.update(new PositionChangeDto(change)),
      );

      console.log(pos);

      if (pos.status === 'NoChange') {
        this.toastService.showInfo(`No changes detected for ${this.getFundName(fundId)}`);
      }

      if (pos.status === 'Success') {
        this.toastService.showSuccess(`Draft trades created for ${this.getFundName(fundId)}`);
        await this.loadPositions(this.selectedDate(), true);
        this.fundBalStore.loadAll(this.selectedDate());
      } else {
        this.toastService.showError(`Failed to create draft trades: ${pos.errorMessage}`);
        this.positionsStore.updateError(trade, fundId.toString(), pos.errorMessage);
      }

      this.cd.detectChanges();
    } catch (error) {
      console.error('Error handling notional change:', error);
      this.toastService.showError('Failed to process notional change');
    }
  }

  async onEditingStart(e: any) {
    this._editThrottle.next();
    if (
      e.column.caption === 'Notional' ||
      e.column.caption === 'Exposure %' ||
      e.column.caption === 'Status'
    ) {
      const fundId = parseInt(e.column.dataField.split('.')[1]);

      if (
        e.data.locked?.[fundId] &&
        e.data.lockedBy?.[fundId] !== this.authService.getDisplayName()
      ) {
        e.cancel = true;
        this.toastService.showWarning(
          'This cell is locked by ' + e.data.lockedBy?.[fundId] + ' and cannot be edited.',
          8000,
        );
        return;
      }

      this.rowBeingEdited = e.data;
      this.fundIdBeingEdited = fundId;

      await firstValueFrom(
        this.positionsApiClient.broadcastLock(
          new PositionLockDto({
            fundId: fundId,
            counterpartyId: e.data.counterpartyId,
            collateralTypeId: e.data.collateralTypeId,
            locked: true,
          }),
        ),
      );
    }
  }

  onEditingEnd(e: any): void {}

  async onFocusedCellChanged(_e: any) {
    await this.finishEditing();
  }

  async onEditingCancelled(_e: any) {
    await this.finishEditing();
  }

  async finishEditing() {
    if (this.rowBeingEdited) {
      await firstValueFrom(
        this.positionsApiClient.broadcastLock(
          new PositionLockDto({
            fundId: this.fundIdBeingEdited,
            counterpartyId: this.rowBeingEdited!.counterpartyId,
            collateralTypeId: this.rowBeingEdited!.collateralTypeId,
            locked: false,
          }),
        ),
      );

      this.rowBeingEdited = undefined;
      this.fundIdBeingEdited = undefined;
    }
  }

  onCellPrepared(e: CellPreparedEvent): void {
    try {
      if (!e || e.rowType !== 'data' || !e.column || !e.cellElement) {
        return;
      }

      const dataField: string | undefined = e.column.dataField;
      if (!dataField || typeof dataField !== 'string') {
        return;
      }

      const isFundColumn =
        dataField.startsWith('fundNotionals.') ||
        dataField.startsWith('fundExposurePercents.') ||
        dataField.startsWith('fundStatuses.');
      if (!isFundColumn) {
        return;
      }

      const parts = dataField.split('.');
      const fundId = Number(parts[1]);
      const isLocked = !!e.data?.locked?.[fundId];
      const hasError = e.data?.error?.[fundId];
      const columnAllowEditing = e.column.allowEditing;
      const isAllowedByColumn = typeof columnAllowEditing === 'boolean' ? columnAllowEditing : true;

      if (!isAllowedByColumn || isLocked) {
        e.cellElement.classList.add('cell-locked');
        e.cellElement.setAttribute(
          'title',
          `This cell is locked by ${e.data?.lockedBy?.[fundId]} and cannot be edited.`,
        );
      } else if (isAllowedByColumn && !isLocked) {
        e.cellElement.classList.remove('cell-locked');
        e.cellElement.removeAttribute('title');
      }

      if (hasError) {
        e.cellElement.classList.add('cell-error');
        e.cellElement.setAttribute('title', e.data?.error?.[fundId]);
      } else {
        e.cellElement.classList.remove('cell-error');
        e.cellElement.removeAttribute('title');
      }
    } catch (err) {
      console.warn('onCellPrepared styling failed:', err);
    }
  }

  calculateExposurePercent(fundId: number, notional: number): number {
    const fundBalance = this.fundBalances.find((fb) => fb.fundId === fundId);
    if (!fundBalance || !fundBalance.availableCash) return 0;
    return (notional / fundBalance.availableCash) * 100;
  }

  getStatus(fundId: number, notional: number): string {
    const exposurePercent = this.calculateExposurePercent(fundId, notional);
    if (exposurePercent > 90) return 'High Risk';
    if (exposurePercent > 70) return 'Medium Risk';
    return 'Normal';
  }

  openAddCashflowPopup(balance: any): void {
    const fund = this.funds().find((f) => f.fundId === balance.fundId);
    if (!fund || !fund.accounts || fund.accounts.length === 0) {
      this.toastService.showError('No cash accounts available for this fund.');
      return;
    }
    const firstAccount = fund.accounts[0];
    this.newCashflow.set(
      new CreateCashflowDto({
        ...this.newCashflow(),
        fundId: balance.fundId,
        cashAccountId: firstAccount.cashAccountId,
        currencyCode: balance.currencyCode,
        createdByUserId: this.authService.getUserId() ?? undefined,
      }),
    );
    this.showAddCashflowPopup.set(true);
  }

  async saveNewCashflow(): Promise<void> {
    try {
      const newCashflow = this.newCashflow();
      await firstValueFrom(this.cashflowClient.cashflowPOST(newCashflow));
      this.toastService.showSuccess('Cashflow added successfully.');
      this.showAddCashflowPopup.set(false);
      // Optionally reload funds or balances
      await this.loadFunds();
      await this.fundBalStore.loadAll(this.selectedDate());
    } catch (err) {
      this.showAddCashflowPopup.set(false);
      this.toastService.showError('Failed to add cashflow.');
    }
  }

  onSaveCashflow(dto: CreateCashflowDto): void {
    this.newCashflow.set(dto);
    this.saveNewCashflow();
  }

  onCancelCashflow(): void {
    this.showAddCashflowPopup.set(false);
  }

  getFundName(fundId: number): string {
    const fund = this.fundBalances.find((f) => f.fundId === fundId);
    return fund?.fundCode + ' | ' + fund?.fundName || `Fund ${fundId}`;
  }

  getCounterpartyName(counterpartyId: number): string {
    const counterparty = this.counterparties().find((c) => c.id === counterpartyId);
    return counterparty?.name || `Counterparty ${counterpartyId}`;
  }

  getCollateralTypeName(collateralTypeId: number): string {
    const collateralType = this.collateralTypes().find((ct) => ct.id === collateralTypeId);
    return collateralType?.name || `Collateral ${collateralTypeId}`;
  }
}

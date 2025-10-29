import { PositionLockDto } from './../../api/client';
import { SignalRService } from './../../services/signalr.service';
import { RepoRatesStore } from './../../store/repo-rates/repo-rates.store';
import { Component, OnInit, inject, ChangeDetectorRef, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  DxDataGridModule,
  DxDropDownBoxComponent,
  DxTemplateHost,
  DxTemplateModule,
} from 'devextreme-angular';
import {
  RepoTradeDto,
  CreateRepoTradeDto,
  UpdateRepoTradeDto,
  RepoRateDto,
  FundDto,
  FundBalanceDto,
  CounterpartyDto,
  CollateralTypeDto,
  IRepoTradeDto,
  IRepoRateDto,
  RepoRatesClient,
  FundsClient,
  RepoTradesClient,
  PositionsClient,
  IPositionChangeDto,
  PositionChangeDto,
  IPositionLockDto,
} from '../../api/client';
import { CounterpartyStore } from '../../store/counterparty/counterparty.store';
import { CollateralTypeStore } from '../../store/collateral-type/collateral-type.store';
import { FundStore } from '../../store/fund/fund.store';
import { TradesStore } from '../../store/trades/trades.store';
import { Navbar } from '../../shared/navbar/navbar';
import { ToastComponent } from '../../shared/toast/toast.component';
import { ThemeService } from '../../services/theme.service';
import { ToastService } from '../../services/toast.service';
import { OrderBasketStore, DraftTrade } from '../../store/order-basket/order-basket.store';
import { SelectionChangedEvent } from 'devextreme/ui/data_grid';
import DataGrid from 'devextreme/ui/data_grid';
import { ITradeBlotterGridItem, PositionsStore } from '../../store/positions/positions.store';
import { firstValueFrom } from 'rxjs';
import { FundBalanceStore } from '../../store/fund-balances/fund-balance.store';


@Component({
  selector: 'app-position-view',
  imports: [
    CommonModule,
    DxDataGridModule,
    Navbar,
    ToastComponent,
    DxDropDownBoxComponent,
    DxTemplateModule,
  ],
  providers: [DxTemplateHost],
  templateUrl: './position-view.html',
})
export class PositionsView implements OnInit {
  private repoRateApiClient = inject(RepoRatesClient);
  private positionsApiClient = inject(PositionsClient);
  private repoTradesApiClient = inject(RepoTradesClient);
  private fundApiClient = inject(FundsClient);
  private signalRService = inject(SignalRService);
    
  private themeService = inject(ThemeService);
  private counterpartyStore = inject(CounterpartyStore);
  private collateralTypeStore = inject(CollateralTypeStore);
  private fundStore = inject(FundStore);
  private fundBalStore = inject(FundBalanceStore);
  private positionsStore = inject(PositionsStore);
  private orderBasketStore = inject(OrderBasketStore);
  private tradeStore = inject(TradesStore);
  private toastService = inject(ToastService);
  repoRateStore = inject(RepoRatesStore);

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
  
  // Data arrays  
  public fundBalances = this.fundBalStore.balances();
  
  // Signals from stores
  public selectedDate = computed(() => new Date()); // Default to today
  public counterparties = this.counterpartyStore.counterparties;
  public collateralTypes = this.collateralTypeStore.collateralTypes;
  public funds = this.fundStore.funds;
  public positions = this.positionsStore.positions;
  public loading = this.positionsStore.isLoading;
  public error = computed(() => this.positionsStore.error());
    


  async ngOnInit(): Promise<void> {
    await Promise.all([
      this.counterpartyStore.loadAll(), 
      this.collateralTypeStore.loadAll(),
      this.fundStore.loadAll()
    ]);
    await this.loadData();

   this.subscribeToSignalR();
  }

  ngOnDestroy(): void {
    // Unsubscribe SignalR handlers
    this.signalRService.off('PositionChanged');
    this.signalRService.off('NewTrade');
  }

  private subscribeToSignalR(): void {
    // PositionChanged -> update the specific position line
    this.signalRService.on<IPositionChangeDto>('PositionChanged', (change: IPositionChangeDto) => {
      console.log('PositionChanged received:', change);
      // TODO: reflect change into positionsStore when server contract is finalized
    });

    // NewTrade -> refresh positions from server
    this.signalRService.on('NewTrade', () => {
      console.log('NewTrade received, consider reloading positions...');
      // Optional: trigger a refresh if needed
      // void this.loadPositions(new Date());
    });

    this.signalRService.on<IPositionLockDto>('PositionCellEditing', (pos) => {
      this.positionsStore.lockPosition(pos);
      console.log('PositionCellEditing received:', pos);
    });
  }

  async loadData(): Promise<void> {
    this.positionsStore.setLoading(true);
    this.positionsStore.setError(null);
    
    try {
      const selectedDate = new Date();
      await Promise.all([
        this.loadRepoRates(selectedDate),
        this.loadFundBalances(selectedDate),
        this.loadPositions(selectedDate)
      ]);
    } catch (error) {
      console.error('Error loading data:', error);
      this.positionsStore.setError('Failed to load trade data. Please check your connection and try again.');
    } finally {
      this.positionsStore.setLoading(false);
    }
  }

  async loadRepoRates(date: Date): Promise<void> {
    try {      
      // check if repoRates for the date are already loaded
      if(this.repoRateStore.rates().length > 0){
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

  async loadPositions(date: Date): Promise<void> {
    if(this.positionsStore.positions().length > 0){
      return;
    }

    try {
      // Get active trades for the date - need to use search method with appropriate parameters
      const positions = await firstValueFrom(this.positionsApiClient.day(
        date
      ));
      
      const tradesWithExtensions: ITradeBlotterGridItem[] = positions.map(pos => ({
        ...pos,
        variance: 0, // Will be calculated
        fundNotionals: pos.fundNotionals,
        fundExposurePercents: pos.exposurePercentages,
        fundStatuses: pos.statuses,
        // Ensure locked map exists so locks and UI styling can work reliably
        locked: {}
      }));
      this.positionsStore.setPositions(tradesWithExtensions);
    } catch (error) {
      console.error('Error loading trades:', error);
      throw error;
    }
  }

  
  
  async onSaved(): Promise<void> {
    // Implement save logic for batch updates
    const currentPositions = this.positionsStore.positions();    

  }

   onRowInserted(e: any): void {
    this.onRowUpdated(e);
   }
  
   
    // Additional event handlers for debugging and ensuring we catch all changes
  async onRowUpdated(e: any) {
    const pos = e.data as ITradeBlotterGridItem;
   if(pos.fundNotionals){
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

  onRepoRateSelectionChanged($event: SelectionChangedEvent, cellInfo: any, dropDownBoxComponent: any) {
    const selectedRowKeys: number[] = $event.selectedRowKeys;
    if (selectedRowKeys.length > 0) {
      const selectedRepoRate = this.repoRateStore.rates().find(rate => rate.id === selectedRowKeys[0]);
      if (selectedRepoRate) {

        // if this already exists in the grid, return
        const existingPosition = this.tradeStore.trades().find(x => x.collateralTypeId === selectedRepoRate.collateralTypeId && x.counterpartyId === selectedRepoRate.counterpartyId);
        if(existingPosition){
          dropDownBoxComponent.close();
          return;
        }

        // Find the trade in the store using the trade ID from the cell data
        const currentPositions = this.tradeStore.trades();
        const tradeIndex = currentPositions.findIndex(trade => trade.id === cellInfo.data.id);

        if (tradeIndex !== -1) {
          // Create a copy of the trades array with the updated trade
          const updatedTrades = [...currentPositions];
          const tradeToUpdate = { ...updatedTrades[tradeIndex] } as ITradeBlotterGridItem;
          
          // Update the trade with repo rate data
          tradeToUpdate.counterpartyId = selectedRepoRate.counterpartyId;
          tradeToUpdate.collateralTypeId = selectedRepoRate.collateralTypeId;
          tradeToUpdate.rate = selectedRepoRate.repoRate;
          tradeToUpdate.variance = selectedRepoRate.finalCircle! - selectedRepoRate.targetCircle!;
          
          // Populate security info
          this.populateSecurityInfo(tradeToUpdate, selectedRepoRate);
          
          // Replace the trade in the array
          updatedTrades[tradeIndex] = tradeToUpdate;
          
          // Update the store
          this.positionsStore.setPositions(updatedTrades);
          
          // Also update the cell data to reflect changes immediately
          cellInfo.data.counterpartyId = selectedRepoRate.counterpartyId;
          cellInfo.data.collateralTypeId = selectedRepoRate.collateralTypeId;
          cellInfo.data.rate = selectedRepoRate.repoRate;
          cellInfo.data.variance = selectedRepoRate.finalCircle! - selectedRepoRate.targetCircle!;
          cellInfo.data.securityId = tradeToUpdate.securityId;          
          cellInfo.data.securityName = tradeToUpdate.securityName;
        } else {
          // For new trades that don't have an ID yet, update cell data directly
          cellInfo.data.counterpartyId = selectedRepoRate.counterpartyId;
          cellInfo.data.collateralTypeId = selectedRepoRate.collateralTypeId;
          cellInfo.data.rate = selectedRepoRate.repoRate;
          cellInfo.data.variance = selectedRepoRate.finalCircle! - selectedRepoRate.targetCircle!;
          
          // Populate security info for the cell data
          this.populateSecurityInfo(cellInfo.data, selectedRepoRate);
        }
        
        // Set the display value for the security description column
        cellInfo.setValue(selectedRepoRate.counterpartyName + ' - ' + selectedRepoRate.collateralTypeName);
      }
      dropDownBoxComponent.close();
    }
  }

  private async populateSecurityInfo(trade: ITradeBlotterGridItem, repoRate: IRepoRateDto): Promise<void> {
    try {
      // Check for existing trades with same counterparty and collateral type
      const existingTrades = await firstValueFrom(this.repoTradesApiClient.search2(      
        undefined,
        trade.counterpartyId,
        trade.collateralTypeId,
        undefined, // startDateFrom
        undefined, // startDateTo
        undefined, // settlementDate
        undefined, // status
        undefined // direction        
      ));
      
      if (existingTrades.length > 0) {
        // Use existing security
        const existingTrade = existingTrades[0];
        trade.securityId = existingTrade.securityId;        
        trade.securityName = existingTrade.securityName;
      } else {
        // Create mock security
        const counterparty = this.counterparties().find(c => c.id === repoRate.counterpartyId);
        const collateralType = this.collateralTypes().find(ct => ct.id === repoRate.collateralTypeId);
        
        // Generate a mock security ID (using a negative number to indicate mock)
        trade.securityId = -Date.now();        
        trade.securityName = `${counterparty?.name} ${collateralType?.name} Repo`;
      }
    } catch (error) {
      console.error('Error populating security info:', error);
      // Fallback to mock security
      const counterparty = this.counterparties().find(c => c.id === repoRate.counterpartyId);
      const collateralType = this.collateralTypes().find(ct => ct.id === repoRate.collateralTypeId);
      
      // Generate a mock security ID (using a negative number to indicate mock)
      trade.securityId = -Date.now();      
      trade.securityName = `${counterparty?.name} ${collateralType?.name} Repo`;
    }
  }

  getSelectedRowKeys<T>(value: T): T[] {
    return value !== null && value !== undefined ? [value] : [];
  }

  // Handle notional value changes for validation and draft trade creation
  async onNotionalChanged(trade: ITradeBlotterGridItem, fundId: number, newNotional: number): Promise<void> {
    try {

      console.log(`Notional changed for Security ${trade.securityName}, Fund ID ${fundId}: New Notional = ${newNotional}`);

      const change: IPositionChangeDto = {        
        fundId: fundId,
        collateralTypeId: trade.collateralTypeId,
        counterpartyId: trade.counterpartyId,
        newNotionalAmount: newNotional,
        status: 'Draft',
        // T+1
        securityMaturityDate: new Date(new Date().setDate(this.selectedDate().getDate() + 1)) // TODO: use business day calculation
      };

      await firstValueFrom(this.positionsApiClient.update(new PositionChangeDto(change)));

      // // Step 1: Validate against Target Circle limit
      // const validationResult = await this.validateTargetCircleLimit(trade, fundId, newNotional);
      
      // if (!validationResult.isValid) {
      //   this.toastService.showWarning(validationResult.warningMessage!, 8000);
      //   return; // Don't proceed with trade creation if validation fails
      // }

      // // if trade.counterpartyId or trade.collateralTypeId is not set, we cannot proceed
      // if(!trade.counterpartyId || !trade.collateralTypeId){
      //   this.toastService.showError('Please select a valid security before entering notional.');
      //   return;
      // }

      // // Step 2: Create draft trade
      // this.addOrderForNewNotional(trade, newNotional);

      // // Step 3: Update available cash display
      // this.updateFundBalance(fundId, newNotional);

      // // update repo rate final circle balance
      // this.updateRepoRateBalance(trade.counterpartyId, trade.collateralTypeId, newNotional);

      // Show success message
  this.toastService.showSuccess(`Draft trade created for ${this.getFundName(fundId)}`);

    } catch (error) {
      console.error('Error handling notional change:', error);
      this.toastService.showError('Failed to process notional change');
    }
  }

  async onEditingStart(e: any) {
    console.log('Editing started:', e);
    if(e.column.caption === 'Notional' || e.column.caption === 'Exposure %' || e.column.caption === 'Status'){
      const fundId = parseInt(e.column.dataField.split('.')[1]);
      await firstValueFrom(this.positionsApiClient.broadcastLock(new PositionLockDto({        
        fundId: fundId,
        counterpartyId: e.data.counterpartyId,
        collateralTypeId: e.data.collateralTypeId
      })));
    }
  }

  onEditingEnd(e: any): void {
    console.log('Editing ended:', e);
    // Sometimes the cell value change happens at the end of editing
    if (e.dataField && e.dataField.startsWith('fundNotionals.')) {
      console.log('Processing from editing end...');
      const fundId = parseInt(e.dataField.split('.')[1]);
      const newNotional = e.value;
      const trade = e.data;
      
      if (newNotional !== undefined && newNotional !== null && trade && newNotional !== 0) {
        this.onNotionalChanged(trade, fundId, newNotional);
      }
    }
  }

  // Visually indicate non-editable (locked) cells with a red border
  onCellPrepared(e: any): void {
    try {
      if (!e || e.rowType !== 'data' || !e.column || !e.cellElement) {
        return;
      }

      const dataField: string | undefined = e.column.dataField;
      if (!dataField || typeof dataField !== 'string') {
        return;
      }

      // Only consider the dynamic fund columns
      const isFundColumn =
        dataField.startsWith('fundNotionals.') ||
        dataField.startsWith('fundExposurePercents.') ||
        dataField.startsWith('fundStatuses.');
      if (!isFundColumn) {
        return;
      }

      // Extract fundId from dataField like 'fundNotionals.12' or 'fundExposurePercents.12.exposure'
      const parts = dataField.split('.');
      const fundId = Number(parts[1]);

      // Determine if the cell should be editable based on row-level locks
      const isLocked = !!e.data?.locked?.[fundId];

      // If editing is disallowed either by column config or row lock, add the CSS class
      const columnAllowEditing = e.column.allowEditing;
      const isAllowedByColumn = typeof columnAllowEditing === 'boolean' ? columnAllowEditing : true;

      if (!isAllowedByColumn || isLocked) {
        e.cellElement.classList.add('cell-locked');
      }
    } catch (err) {
      console.warn('onCellPrepared styling failed:', err);
    }
  }

  // Calculate exposure percentage for a fund
  calculateExposurePercent(fundId: number, notional: number): number {
    const fundBalance = this.fundBalances.find(fb => fb.fundId === fundId);
    if (!fundBalance || !fundBalance.availableCash) return 0;
    return (notional / fundBalance.availableCash) * 100;
  }

  // Get status based on exposure and limits
  getStatus(fundId: number, notional: number): string {
    const exposurePercent = this.calculateExposurePercent(fundId, notional);
    if (exposurePercent > 90) return 'High Risk';
    if (exposurePercent > 70) return 'Medium Risk';
    return 'Normal';
  }

  // Helper method to get fund name
  getFundName(fundId: number): string {
    const fund = this.funds().find(f => f.id === fundId);
    return fund?.fundName || `Fund ${fundId}`;
  }

  // Helper method to get counterparty name
  getCounterpartyName(counterpartyId: number): string {
    const counterparty = this.counterparties().find(c => c.id === counterpartyId);
    return counterparty?.name || `Counterparty ${counterpartyId}`;
  }

  // Helper method to get collateral type name
  getCollateralTypeName(collateralTypeId: number): string {
    const collateralType = this.collateralTypes().find(ct => ct.id === collateralTypeId);
    return collateralType?.name || `Collateral ${collateralTypeId}`;
  }
}

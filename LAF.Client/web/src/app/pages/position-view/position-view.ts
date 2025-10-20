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
  private repoTradesApiClient = inject(RepoTradesClient);
  private fundApiClient = inject(FundsClient);
    
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

  gridTheme = computed(() => (this.isDark ? 'dx.material.blue.compact.dark' : 'dx.material.blue.compact.light'));
  get isDark(): boolean {
    return this.themeService.isDark();
  }

  async ngOnInit(): Promise<void> {
    await Promise.all([
      this.counterpartyStore.loadAll(), 
      this.collateralTypeStore.loadAll(),
      this.fundStore.loadAll()
    ]);
    await this.loadData();    
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
      const positions = await firstValueFrom(this.repoTradesApiClient.search2(
        undefined, // fundId
        undefined, // counterpartyId  
        date, // startDateFrom
        date, // startDateTo
        undefined, // settlementDate
        'Active', // status
        undefined // direction
      ));
      const tradesWithExtensions: ITradeBlotterGridItem[] = positions.map(trade => ({
        ...trade,
        variance: 0, // Will be calculated
        fundNotionals: {},
        fundExposurePercents: {},
        fundStatuses: {}
      }));
      this.positionsStore.setPositions(tradesWithExtensions);
    } catch (error) {
      console.error('Error loading trades:', error);
      throw error;
    }
  }

  
  // Handle cell value changes in the DataGrid
  async onCellValueChanged(e: any): Promise<void> {
    console.log('Cell value changed:', e); // Debug logging
    
    // Check if this is a notional field change
    const dataField = e.dataField;
    console.log('Data field:', dataField);
    
    // Check if this is a fund notional field (format: 'fundNotionals.{fundId}')
    if (dataField && dataField.startsWith('fundNotionals.')) {
      const fundId = parseInt(dataField.split('.')[1]);
      const newNotional = e.value;
      const trade = e.data;
      
      console.log(`Fund notional changed - Fund ID: ${fundId}, New Notional: ${newNotional}, Trade:`, trade);
      
      if (newNotional !== undefined && newNotional !== null && trade && newNotional !== 0) {
        console.log('Processing notional change...');
        await this.onNotionalChanged(trade, fundId, newNotional);
      }
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
  onRowUpdated(e: any): void {
    const pos = e.data as ITradeBlotterGridItem;
    this.positionsStore.addOrUpatePosition(pos);
    
   if(pos.fundNotionals){
        Object.entries(pos.fundNotionals).forEach(([fundId, newNotional]) => {
          this.onNotionalChanged(pos, parseInt(fundId), newNotional);
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
        const existingPosition = this.positionsStore.positions().find(x => x.collateralTypeId === selectedRepoRate.collateralTypeId && x.counterpartyId === selectedRepoRate.counterpartyId);
        if(existingPosition){
          dropDownBoxComponent.close();
          return;
        }

        // Find the trade in the store using the trade ID from the cell data
        const currentPositions = this.positionsStore.positions();
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
          cellInfo.data.securityIsin = tradeToUpdate.securityIsin;
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
        trade.fundId,
        trade.counterpartyId,
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
        trade.securityIsin = existingTrade.securityIsin;
        trade.securityName = existingTrade.securityName;
      } else {
        // Create mock security
        const counterparty = this.counterparties().find(c => c.id === repoRate.counterpartyId);
        const collateralType = this.collateralTypes().find(ct => ct.id === repoRate.collateralTypeId);
        
        // Generate a mock security ID (using a negative number to indicate mock)
        trade.securityId = -Date.now();
        trade.securityIsin = `MOCK_${counterparty?.name}_${collateralType?.name}_${Date.now()}`;
        trade.securityName = `${counterparty?.name} ${collateralType?.name} Repo`;
      }
    } catch (error) {
      console.error('Error populating security info:', error);
      // Fallback to mock security
      const counterparty = this.counterparties().find(c => c.id === repoRate.counterpartyId);
      const collateralType = this.collateralTypes().find(ct => ct.id === repoRate.collateralTypeId);
      
      // Generate a mock security ID (using a negative number to indicate mock)
      trade.securityId = -Date.now();
      trade.securityIsin = `MOCK_${counterparty?.name}_${collateralType?.name}_${Date.now()}`;
      trade.securityName = `${counterparty?.name} ${collateralType?.name} Repo`;
    }
  }

  getSelectedRowKeys<T>(value: T): T[] {
    return value !== null && value !== undefined ? [value] : [];
  }

  // Handle notional value changes for validation and draft trade creation
  async onNotionalChanged(trade: ITradeBlotterGridItem, fundId: number, newNotional: number): Promise<void> {
    try {
      // Step 1: Validate against Target Circle limit
      const validationResult = await this.validateTargetCircleLimit(trade, fundId, newNotional);
      
      if (!validationResult.isValid) {
        this.toastService.showWarning(validationResult.warningMessage!, 8000);
        return; // Don't proceed with trade creation if validation fails
      }

      // if trade.counterpartyId or trade.collateralTypeId is not set, we cannot proceed
      if(!trade.counterpartyId || !trade.collateralTypeId){
        this.toastService.showError('Please select a valid security before entering notional.');
        return;
      }

      // Step 2: Create draft trade
      this.addOrderForNewNotional(trade, newNotional);

      // Step 3: Update available cash display
      this.updateFundBalance(fundId, newNotional);

      // update repo rate final circle balance
      this.updateRepoRateBalance(trade.counterpartyId, trade.collateralTypeId, newNotional);

      // Show success message
      this.toastService.showSuccess(`Draft trade created for ${this.getFundName(fundId)}: $${newNotional.toLocaleString()}`);

    } catch (error) {
      console.error('Error handling notional change:', error);
      this.toastService.showError('Failed to process notional change');
    }
  }

  private addOrderForNewNotional(trade: ITradeBlotterGridItem, newNotional: number){
     // get submitted trades from tradeStore that has the same fund code, counterpartyId, collateralTypeId
     const trades =  this.tradeStore.trades().filter(t =>
       t.fundCode === trade.fundCode &&
       t.counterpartyId === trade.counterpartyId &&
       t.collateralTypeId === trade.collateralTypeId
     );

      // calculate total position
     let totalPosition = 0;
     trades.forEach(t => {
       const directionMultiplier = t.direction === 'Buy' ? 1 : -1;
       totalPosition += (t.notional || 0) * directionMultiplier;
     });

     // prepare a new order with a notional equal to diff of newNotional and totalPosition
      const direction: 'Buy' | 'Sell' = newNotional >= totalPosition ? 'Buy' : 'Sell';
      const absNotional = Math.abs(newNotional - totalPosition);
      
      const newOrder: DraftTrade = {        
        id: `draft_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
        securityId: trade.securityId,
        securityIsin: trade.securityIsin,
        securityName: trade.securityName,
        collateralTypeId: trade.collateralTypeId,
        counterpartyId: trade.counterpartyId,
        notional: absNotional,
        direction: direction,
        fundId: trade.fundId,
        fundCode: trade.fundCode,
        fundName: trade.fundName,
        rate: trade.rate,        
        status: 'Draft',        
        createdAt: new Date()
      };

      this.orderBasketStore.addOrUpdateDraftTrade(newOrder);    

     return totalPosition;
  }

  private async validateTargetCircleLimit(trade: ITradeBlotterGridItem, fundId: number, newNotional: number): Promise<{isValid: boolean, warningMessage?: string}> {
    try {
      // Get the corresponding repo rate for this counterparty and collateral type
      const repoRate = this.repoRateStore.rates().find(rate =>
        rate.counterpartyId === trade.counterpartyId && 
        rate.collateralTypeId === trade.collateralTypeId
      );

      if (!repoRate) {
        return { isValid: true }; // No repo rate found, can't validate
      }
        
      // Check against Target Circle limit
      const targetCircleAmount = (repoRate.targetCircle || 0) * 1000000;
      
      if (newNotional > targetCircleAmount) {
        const excessAmount = newNotional - targetCircleAmount;
        const message = `Target Circle limit exceeded! Total notional ($${newNotional.toLocaleString()}) exceeds Target Circle ($${targetCircleAmount.toLocaleString()}) by $${excessAmount.toLocaleString()}`;
        return { isValid: false, warningMessage: message };
      }

      return { isValid: true };
    } catch (error) {
      console.error('Error validating Target Circle limit:', error);
      return { isValid: true }; // Allow trade on validation errors
    }
  }


  private updateFundBalance(fundId: number, notionalAmount: number): void {    
    this.fundBalStore.reduceBalanceBalances(fundId, notionalAmount);   
  }

  private updateRepoRateBalance(counterpartyId: number, collateralTypeId: number, notionalAmount: number): void {        
    this.repoRateStore.addToCircle(counterpartyId, collateralTypeId, notionalAmount);
  }

  onEditingStart(e: any): void {
    console.log('Editing started:', e);
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

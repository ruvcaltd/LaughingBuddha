import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { DxDataGridModule, DxTemplateHost, DxTemplateModule } from 'devextreme-angular';
import { firstValueFrom } from 'rxjs';
import { RepoTradesClient } from '../../api/client';
import { TradesStore } from '../../store/trades/trades.store';

@Component({
  selector: 'app-order-basket',
  imports: [CommonModule, DxDataGridModule, DxTemplateModule],
  providers: [DxTemplateHost],
  templateUrl: './order-basket.html',
})
export class OrderBasket implements OnInit {
  private tradesStore = inject(TradesStore);
  private tradeApiClient = inject(RepoTradesClient);

  // Filter trades where status is 'Draft'
  draftTrades = computed(() =>
    this.tradesStore.trades().filter((trade) => trade.status === 'Draft'),
  );

  loading = this.tradesStore.loading;
  error = this.tradesStore.error;

  public gridOptions = {
    editing: {
      mode: 'batch' as const,
      allowUpdating: true,
      allowAdding: false,
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
    columnResizingMode: 'widget' as const,
  };

  async ngOnInit(): Promise<void> {
    this.tradesStore.loadTrades(new Date(), new Date(), 'Draft');
  }

  formatCurrency(value: number | undefined): string {
    if (value === undefined || value === null) return '$0';
    return `$${value.toFixed(2)}M`;
  }

  formatPercent(value: number | undefined): string {
    if (value === undefined || value === null) return '0%';
    return `${value.toFixed(4)}%`;
  }

  async submitTrades(): Promise<void> {
    const tradeIds = this.draftTrades()
      .map((trade) => trade.id)
      .filter((id) => id !== undefined) as number[];
    try {
      await firstValueFrom(this.tradeApiClient.submit(tradeIds));
      // Refresh the trades after submission
      this.tradesStore.loadTrades(new Date(), new Date(), 'Draft');
    } catch (error) {
      console.error('Error submitting trades:', error);
      // Handle error appropriately, maybe show a toast or alert
    }
  }
}

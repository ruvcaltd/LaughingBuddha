import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { DxDataGridModule, DxTemplateHost, DxTemplateModule } from 'devextreme-angular';
import { TradesStore } from '../../store/trades/trades.store';

@Component({
  selector: 'app-submitted-trades',
  imports: [CommonModule, DxDataGridModule, DxTemplateModule],
  providers: [DxTemplateHost],
  templateUrl: './submitted-trades.html',
})
export class SubmittedTrades implements OnInit {
  private tradesStore = inject(TradesStore);

  // Filter trades where status is not 'Draft'
  submittedTrades = computed(() =>
    this.tradesStore.trades().filter((trade) => trade.status !== 'Draft'),
  );

  loading = this.tradesStore.loading;
  error = this.tradesStore.error;

  public gridOptions = {
    editing: {
      mode: 'batch' as const,
      allowUpdating: false,
      allowAdding: false,
      allowDeleting: false,
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
    this.tradesStore.loadTrades(new Date(), new Date(), null);
  }

  formatCurrency(value: number | undefined): string {
    if (value === undefined || value === null) return '$0';
    return `$${value.toFixed(2)}M`;
  }

  formatPercent(value: number | undefined): string {
    if (value === undefined || value === null) return '0%';
    return `${value.toFixed(4)}%`;
  }
}

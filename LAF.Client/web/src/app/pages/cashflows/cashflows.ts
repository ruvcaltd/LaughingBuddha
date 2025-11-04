import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { firstValueFrom } from 'rxjs';
import {
  CashflowClient,
  CashflowDto,
  CreateCashflowDto,
  FundsClient,
  ICashflowDto,
  IFundAccountCashflowsDto,
} from '../../api/client';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';
import { ToastService } from '../../services/toast.service';
import { ToastComponent } from '../../shared/toast/toast.component';
import { SharedStore } from '../../store/shared/shared.store'; // Assuming this exists for currentUser
import { AddCashflowDialogComponent } from '../../shared/add-cashflow-dialog/add-cashflow-dialog.component';

@Component({
  selector: 'app-cashflows',
  imports: [CommonModule, DxDataGridModule, FormsModule, ToastComponent, DxTemplateModule, AddCashflowDialogComponent],
  templateUrl: './cashflows.html',
})
export class Cashflows implements OnInit, OnDestroy {
  ngOnDestroy(): void {
    this.signalRService.off('NewCashflow');
    this.signalRService.off('CashflowDeleted');
  }
  private cashflowClient = inject(CashflowClient);
  private fundsClient = inject(FundsClient);
  private toastService = inject(ToastService);
  private sharedStore = inject(SharedStore);
  private cd = inject(ChangeDetectorRef);
  private signalRService = inject(SignalRService);
  private authService = inject(AuthService);

  // Signals for state
  funds = signal<IFundAccountCashflowsDto[]>([]);
  selectedFund = signal<IFundAccountCashflowsDto | null>(null);
  selectedCashAccount = signal<number | null>(null);
  selectedDate = signal<Date>(new Date());

  cashflows = signal<any[]>([]); // Array of CashflowDto

  loading = signal(false);
  error = signal<string | null>(null);

  // Popup state
  showAddPopup = signal(false);
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

  async ngOnInit(): Promise<void> {
    await this.loadFunds();

    this.signalRService.on(
      'CashflowCreated',
      async (change: { sender: number; payload: ICashflowDto }) => {
        if (change.sender === this.authService.getUserId()) return;
        const selectedFund = this.selectedFund();
        const selectedCashAccount = this.selectedCashAccount();
        await this.loadFunds();
        await this.reloadGrid(selectedFund?.fundId!, selectedCashAccount!);
      },
    );

    this.signalRService.on(
      'CashflowDeleted',
      async (change: {
        sender: number;
        payload: { id: number; fundId: number; cashAccountId: number };
      }) => {
        if (change.sender === this.authService.getUserId()) return;
        await this.loadFunds();
        await this.reloadGrid(change.payload.fundId, change.payload.cashAccountId);
      },
    );

    this.cd.detectChanges();
  }

  async loadFunds(): Promise<void> {
    try {
      this.loading.set(true);
      const fundsData = await firstValueFrom(this.cashflowClient.daily(new Date())); // Assuming getFunds() method exists; replace with actual endpoint if different
      this.funds.set(fundsData);
      console.log(this.funds());
      this.cd.detectChanges();
    } catch (err) {
      this.error.set('Failed to load funds.');
      this.toastService.showError('Failed to load funds.');
    } finally {
      this.loading.set(false);
    }
  }

  async onCashAccountChange($event: any): Promise<void> {
    const account: number | undefined = $event.target?.value ? +$event.target.value : undefined;
    if (account) {
      this.selectedCashAccount.set(account);
      await this.loadCashflows();
    }
  }

  async onDateChange(date: Date): Promise<void> {
    this.selectedDate.set(date);
    await this.loadCashflows();
  }

  async loadCashflows(): Promise<void> {
    if (!this.selectedFund() || !this.selectedCashAccount() || !this.selectedDate()) return;
    try {
      this.loading.set(true);
      var cfs = this.selectedFund()?.accounts?.find(
        (a) => a.cashAccountId == this.selectedCashAccount(),
      )?.cashflows;
      this.cashflows.set(cfs || []);
    } catch (err) {
      this.error.set('Failed to load cashflows.');
      this.toastService.showError('Failed to load cashflows.');
    } finally {
      this.loading.set(false);
    }
  }

  openAddPopup(): void {
    if (!this.selectedFund() || !this.selectedCashAccount()) {
      this.toastService.showError('Select a Fund and Cash Account first.');
      return;
    }
    this.newCashflow.set(
      new CreateCashflowDto({
        ...this.newCashflow(),
        fundId: this.selectedFund()!.fundId,
        cashAccountId: this.selectedCashAccount() ?? undefined,
        createdByUserId: this.sharedStore.currentUser()?.id,
      }),
    );
    this.showAddPopup.set(true);
  }

  async saveNewCashflow(): Promise<void> {
    try {
      const newCashflow = this.newCashflow();
      await firstValueFrom(this.cashflowClient.cashflowPOST(newCashflow));
      this.toastService.showSuccess('Cashflow added successfully.');
      this.showAddPopup.set(false);

      this.selectedFund()
        ?.accounts?.find((a) => a.cashAccountId == this.selectedCashAccount())
        ?.cashflows?.push(
          new CashflowDto({
            cashAccountId: newCashflow.cashAccountId!,
            fundId: newCashflow.fundId!,
            amount: newCashflow.amount!,
            currencyCode: newCashflow.currencyCode!,
            description: newCashflow.description!,
            source: newCashflow.source!,
          }),
        );

      await this.reloadGrid(newCashflow.fundId!, newCashflow.cashAccountId!);
    } catch (err) {
      this.showAddPopup.set(false);
      this.toastService.showError('Failed to add cashflow.');
    }

    this.cd.detectChanges();
  }

  async onRowRemoving(e: any) {
    await this.deleteCashflow(e.data.id);
  }

  async deleteCashflow(id: number): Promise<void> {
    try {
      this.loading.set(true);
      const selectedFund = this.selectedFund();
      const selectedCashAccount = this.selectedCashAccount();
      await firstValueFrom(this.cashflowClient.cashflowDELETE(id)); // Assuming this method exists
      this.toastService.showSuccess('Cashflow deleted successfully.');
      await this.reloadGrid(selectedFund!.fundId!, selectedCashAccount!);
    } catch (err) {
      this.toastService.showError('Failed to delete cashflow.');
    } finally {
      this.loading.set(false);
    }
  }

  async reloadGrid(fundId: number, cashAccountId: number) {
    await this.loadFunds(); // Refresh grid
    const selectedFund = this.funds().find((f) => f.fundId === fundId);
    this.selectedFund.set(selectedFund ?? null);
    this.selectedCashAccount.set(cashAccountId ?? null);
    this.loadCashflows();
  }

  closeAddPopup(): void {
    this.showAddPopup.set(false);
  }

  onFundChange(): void {
    this.selectedCashAccount.set(null);
  }

  async reloadCurrentSelection(): Promise<void> {
    if (!this.selectedFund() || !this.selectedCashAccount()) {
      this.toastService.showError('Select a Fund and Cash Account first.');
      return;
    }
    await this.reloadGrid(this.selectedFund()!.fundId!, this.selectedCashAccount()!);
  }

  onSave(dto: CreateCashflowDto): void {
    this.newCashflow.set(dto);
    this.saveNewCashflow();
  }

  onCancel(): void {
    this.closeAddPopup();
  }
}

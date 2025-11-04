import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CreateCashflowDto } from '../../api/client';


@Component({
  selector: 'app-add-cashflow-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-cashflow-dialog.component.html',
})
export class AddCashflowDialogComponent {
  @Input() show: boolean = false;
  @Input() cashflow: CreateCashflowDto = new CreateCashflowDto();
  @Output() save = new EventEmitter<CreateCashflowDto>();
  @Output() cancel = new EventEmitter();

  onSave() {
    this.save.emit(this.cashflow);
  }

  onCancel() {
    this.cancel.emit();
  }
}
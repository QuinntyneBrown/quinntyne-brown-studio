import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'qbs-date-time-field',
  imports: [FormsModule],
  templateUrl: './date-time-field.html',
  styleUrl: './date-time-field.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DateTimeField {
  readonly fieldId = input.required<string>();
  readonly label = input.required<string>();
  readonly zone = input('');
  readonly value = input('');
  readonly offset = input('');
  readonly offsets = input<string[]>([]);
  readonly error = input('');
  readonly disabled = input(false);
  readonly step = input(60);
  readonly valueChanged = output<string>();
  readonly offsetChanged = output<string>();
}

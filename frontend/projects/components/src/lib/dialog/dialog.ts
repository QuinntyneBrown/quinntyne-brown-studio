import {
  Component,
  ElementRef,
  viewChild,
  input,
  output,
  AfterViewInit,
  OnDestroy,
} from '@angular/core';
@Component({ selector: 'qbs-dialog', templateUrl: './dialog.html', styleUrl: './dialog.css' })
export class Dialog implements AfterViewInit, OnDestroy {
  title = input('Photo');
  closed = output<void>();
  element = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private previous: HTMLElement | null = null;
  ngAfterViewInit() {
    this.previous = document.activeElement as HTMLElement;
    this.element().nativeElement.showModal();
  }
  ngOnDestroy() {
    this.element().nativeElement.close();
    queueMicrotask(() => this.previous?.focus());
  }
}

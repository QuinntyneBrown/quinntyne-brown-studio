/*
 * Dialog behaviour shared by the catalog and the isolated preview page:
 * a trigger opens the modal, Escape or a close control dismisses it, and
 * focus returns to the control that opened it.
 */
export function wireDialogs(container) {
  for (const trigger of container.querySelectorAll('[data-dialog-open]')) {
    trigger.addEventListener('click', () => {
      const dialog = container.querySelector(`#${trigger.dataset.dialogOpen}`);
      if (!dialog) return;
      dialog.returnFocusTo = trigger;
      dialog.showModal();
    });
  }
  for (const dialog of container.querySelectorAll('dialog')) {
    for (const close of dialog.querySelectorAll('[data-dialog-close]')) {
      close.addEventListener('click', () => dialog.close());
    }
    dialog.addEventListener('close', () => dialog.returnFocusTo?.focus());
  }
}

import { componentMarkup, dialogMarkup, patternMarkup } from './catalog-content.js';
import { wireDialogs } from './dialogs.js';

const manifest = await fetch('/component-manifest.json').then((response) => response.json());
const parameters = new URL(window.location.href).searchParams;
const type = parameters.get('type') ?? 'component';
const id = parameters.get('id');
const scenario = parameters.get('example') ?? parameters.get('scenario');
const target = document.querySelector('#preview');

const component = manifest.components.find((entry) => entry.id === id);
const markup =
  type === 'component' && component
    ? componentMarkup(component, scenario)
    : type === 'pattern'
      ? patternMarkup(id, scenario)
      : type === 'dialog'
        ? dialogMarkup(id, scenario)
        : '';

target.innerHTML =
  markup ||
  '<div class="notice notice--error" role="status">That example is not in the component manifest.</div>';
wireDialogs(target);

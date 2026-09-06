import { componentMarkup, dialogMarkup, patternMarkup } from './catalog-content.js';
import { wireDialogs } from './dialogs.js';

const manifest = await fetch('/component-manifest.json').then((response) => response.json());
const app = document.querySelector('#catalog');
const escapeHtml = (value) =>
  value.replace(/[&<>"]/g, (character) => `&${{ '&': 'amp', '<': 'lt', '>': 'gt', '"': 'quot' }[character]};`);
const tidy = (markup) => markup.replace(/\s*\n\s*/g, ' ').trim();

const componentById = (id) => manifest.components.find((component) => component.id === id);
const familyById = (collection, id) => manifest[collection].find((family) => family.id === id);

const tokenGroups = [
  {
    name: 'Colour',
    tokens: ['--ink', '--muted', '--line', '--soft', '--paper', '--accent', '--danger'],
  },
  { name: 'Type', tokens: ['--serif', '--sans'] },
  {
    name: 'Space and shape',
    tokens: ['--space-1', '--space-2', '--space-3', '--space-4', '--space-5', '--space-6', '--radius', '--tap-target'],
  },
];

function example(markup, label) {
  return `<div class="example"><div class="example__frame" data-example-frame${
    label ? ` aria-label="${label}"` : ''
  }>${markup}</div><details class="example__code"><summary>Markup</summary><pre><code>${escapeHtml(
    tidy(markup),
  )}</code></pre></details></div>`;
}

export function renderFoundations() {
  const styles = getComputedStyle(document.documentElement);
  const rows = tokenGroups
    .map(
      (group) => `<section class="section"><div class="section__header"><h2>${group.name}</h2></div>
        <div class="records">${group.tokens
          .map(
            (token) =>
              `<article class="records__row" data-token="${token}"><div><h3>${token}</h3><p class="records__detail">${escapeHtml(
                styles.getPropertyValue(token).trim(),
              )}</p></div><div class="records__actions"><span class="swatch" style="background:${
                token.includes('serif') || token.includes('sans') || token.includes('space') || token.includes('radius') || token.includes('tap')
                  ? 'transparent'
                  : `var(${token})`
              }"></span></div></article>`,
          )
          .join('')}</div></section>`,
    )
    .join('');
  return `<header class="page__header"><div><p class="page__eyebrow">Foundations</p><h1>Design tokens</h1>
    <p class="page__description">Tokens are declared once in <code>assets/tokens.css</code> and are authoritative for this product.</p></div></header>${rows}`;
}

export function renderIndex() {
  const cards = (items, kind) =>
    items
      .map(
        (item) =>
          `<a class="card" data-${kind}-card="${item.id}" href="/${kind}s/${item.id}"><h3>${item.name}</h3><p class="records__detail">${item.description}</p></a>`,
      )
      .join('');
  return `<header class="page__header"><div><p class="page__eyebrow">${manifest.product.name}</p>
    <h1>A quiet, considered foundation.</h1><p class="page__description">${manifest.product.description}</p></div></header>
    <dl class="coverage" aria-label="Catalog coverage">
      <div data-coverage="components"><dt>Components</dt><dd>${manifest.components.length}</dd></div>
      <div data-coverage="patterns"><dt>Screen patterns</dt><dd>${manifest.patterns.reduce((total, family) => total + family.scenarios.length, 0)}</dd></div>
      <div data-coverage="dialogs"><dt>Dialog scenarios</dt><dd>${manifest.dialogs.reduce((total, family) => total + family.scenarios.length, 0)}</dd></div>
    </dl>
    ${manifest.categories
      .map((category) => {
        const components = manifest.components.filter((component) => component.category === category.id);
        return components.length
          ? `<section class="section"><div class="section__header"><h2>${category.name}</h2></div>
             <div class="layout__grid">${cards(components, 'component')}</div></section>`
          : '';
      })
      .join('')}
    <section class="section"><div class="section__header"><h2>Screen patterns</h2></div>
      <div class="layout__grid">${cards(manifest.patterns, 'pattern')}</div></section>
    <section class="section"><div class="section__header"><h2>Dialogs</h2></div>
      <div class="layout__grid">${cards(manifest.dialogs, 'dialog')}</div></section>`;
}

export function renderComponent(id, exampleId) {
  const component = componentById(id);
  if (!component) return renderMissing();
  const active = component.examples.find((entry) => entry.id === exampleId) ?? component.examples[0];
  return `<header class="page__header"><div><p class="page__eyebrow">${
    manifest.categories.find((category) => category.id === component.category).name
  }</p><h1>${component.name}</h1><p class="page__description">${component.description}</p></div></header>
    <nav class="switcher" aria-label="Examples">${component.examples
      .map(
        (entry) =>
          `<a href="/components/${component.id}?example=${entry.id}"${
            entry.id === active.id ? ' aria-current="true"' : ''
          }>${entry.title}</a>`,
      )
      .join('')}</nav>
    <section class="panel" data-example-panel><h2>${active.title}</h2><p class="text--muted">${active.description}</p>
      ${example(componentMarkup(component, active.id), `${component.name} example`)}</section>
    <section class="section"><div class="section__header"><h2>Contract</h2></div>
      <div class="records">
        <article class="records__row"><div><h3>Classes</h3><p class="records__detail">${component.classes
          .map((name) => `<code>.${name}</code>`)
          .join(' ')}</p></div></article>
        <article class="records__row"><div><h3>States</h3><p class="records__detail">${component.states.join(
          ', ',
        )}</p></div></article>
        <article class="records__row"><div><h3>Isolated preview</h3><p class="records__detail">
          <a href="/preview.html?type=component&amp;id=${component.id}&amp;example=${active.id}">Open this example on its own</a></p></div></article>
      </div></section>`;
}

function renderFamily(collection, id, scenarioId, markup) {
  const family = familyById(collection, id);
  if (!family) return renderMissing();
  const active = family.scenarios.find((scenario) => scenario.id === scenarioId) ?? family.scenarios[0];
  const label = collection === 'patterns' ? 'Screen pattern' : 'Dialog';
  return `<header class="page__header"><div><p class="page__eyebrow">${label}</p><h1>${family.name}</h1>
    <p class="page__description">${family.description}</p></div></header>
    <nav class="switcher" aria-label="Scenarios">${family.scenarios
      .map(
        (scenario) =>
          `<a href="/${collection === 'patterns' ? 'pattern' : 'dialog'}s/${family.id}/${scenario.id}"${
            scenario.id === active.id ? ' aria-current="true"' : ''
          }>${scenario.name}</a>`,
      )
      .join('')}</nav>
    <section class="panel" data-example-panel><h2>${active.name}</h2>
      ${example(markup(family.id, active.id), `${family.name} ${active.name}`)}</section>
    <section class="section"><div class="section__header"><h2>Review</h2></div>
      <div class="records">
        <article class="records__row"><div><h3>Scenarios</h3><p class="records__detail">${family.scenarios
          .map((scenario) => scenario.name)
          .join(', ')}</p></div></article>
        <article class="records__row"><div><h3>Isolated preview</h3><p class="records__detail">
          <a href="/preview.html?type=${collection === 'patterns' ? 'pattern' : 'dialog'}&amp;id=${family.id}&amp;scenario=${active.id}">Open this scenario on its own</a></p></div></article>
      </div></section>`;
}

export function renderPattern(id, scenarioId) {
  return renderFamily('patterns', id, scenarioId, patternMarkup);
}

export function renderDialog(id, scenarioId) {
  return renderFamily('dialogs', id, scenarioId, dialogMarkup);
}

function renderMissing() {
  return `<header class="page__header"><div><p class="page__eyebrow">404</p><h1>That catalog entry does not exist.</h1>
    <p class="page__description">Every entry is listed in the component manifest.</p></div></header>
    <div class="form__actions"><a class="button" href="/">Return to the catalog</a></div>`;
}

function renderNavigation(path) {
  const link = (href, label) =>
    `<a href="${href}"${path === href ? ' aria-current="page"' : ''}>${label}</a>`;
  return `${link('/', 'Overview')}${link('/foundations', 'Foundations')}
    <p class="nav__group">Components</p>${manifest.components
      .map((component) => link(`/components/${component.id}`, component.name))
      .join('')}
    <p class="nav__group">Screen patterns</p>${manifest.patterns
      .map((family) => link(`/patterns/${family.id}`, family.name))
      .join('')}
    <p class="nav__group">Dialogs</p>${manifest.dialogs
      .map((family) => link(`/dialogs/${family.id}`, family.name))
      .join('')}`;
}

function route() {
  const { pathname, searchParams } = new URL(window.location.href);
  const [, first, second, third] = pathname.split('/');
  if (!first) return renderIndex();
  if (first === 'foundations') return renderFoundations();
  if (first === 'components') return renderComponent(second, searchParams.get('example'));
  if (first === 'patterns') return renderPattern(second, third);
  if (first === 'dialogs') return renderDialog(second, third);
  return renderMissing();
}

export function render() {
  const { pathname } = new URL(window.location.href);
  const disclosure = document.querySelector('#catalog-navigation');
  // A phone should meet the content, not forty links: the list opens by default
  // only where the sidebar sits beside the content.
  disclosure.open = window.matchMedia('(min-width: 851px)').matches;
  document.querySelector('#navigation').innerHTML = renderNavigation(pathname);
  app.innerHTML = route();
  wireDialogs(app);
  document.title = `${document.querySelector('#catalog h1').textContent} · ${manifest.product.name}`;
}

document.addEventListener('click', (event) => {
  const link = event.target.closest('a[href^="/"]');
  if (!link || link.href.includes('/preview.html') || event.metaKey || event.ctrlKey) return;
  event.preventDefault();
  window.history.pushState({}, '', link.getAttribute('href'));
  render();
  window.scrollTo({ top: 0 });
});
window.addEventListener('popstate', render);
render();

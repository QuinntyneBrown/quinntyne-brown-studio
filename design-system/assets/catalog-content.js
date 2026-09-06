/*
 * Standalone fixtures for screen patterns and dialog scenarios.
 * Every string below is illustrative catalog content: no studio data, no
 * credentials, and no request ever leaves the page.
 */

const photoTile = (name, label = 'Ready', disabled = false) =>
  `<figure class="photo-grid__tile"><button class="photo-grid__open" type="button"${
    disabled ? ' disabled' : ''
  } aria-label="View ${name}"><span>${label}</span></button><figcaption class="photo-grid__caption">${name}</figcaption></figure>`;

const photoGrid = (tiles) => `<div class="photo-grid">${tiles.join('')}</div>`;

const header = (eyebrow, title, description, action = '') =>
  `<header class="page__header"><div><p class="page__eyebrow">${eyebrow}</p><h1>${title}</h1><p class="page__description">${description}</p></div>${action}</header>`;

const notice = (message, error = false) =>
  `<div class="notice${error ? ' notice--error' : ''}" role="status" aria-live="polite">${message}</div>`;

const patterns = {
  'marketing-home': {
    published: () =>
      `${header('Quinntyne Brown Studio', 'Photography with feeling.', 'Weddings, events, headshots, and family portraits across the Greater Toronto Area.', '<a class="button" href="#">Calculate your quote</a>')}
       <section class="section"><div class="section__header"><h2>Recent work</h2><a href="#">View the portfolio →</a></div>
       ${photoGrid([photoTile('Riverside ceremony'), photoTile('Harbour portraits'), photoTile('Family in the park')])}</section>`,
    unavailable: () =>
      `${header('Quinntyne Brown Studio', 'Photography with feeling.', 'Weddings, events, headshots, and family portraits across the Greater Toronto Area.')}
       ${notice('Published content is temporarily unavailable. Please try again shortly.', true)}
       <div class="empty-state"><span aria-hidden="true">◇</span><p>No galleries available yet.</p></div>`,
  },
  'quote-calculator': {
    calculated: () =>
      `${header('Quotation', 'Your quote', 'Totals recalculate as you change the session.')}
       <div class="layout__split"><form class="form"><h2>Session</h2><div class="form__grid">
       <label class="field"><span>Service</span><select name="service"><option>Wedding</option><option>Event</option></select></label>
       <label class="field"><span>Hours</span><input name="hours" type="number" value="8" /></label>
       <label class="field"><span>Equipment rental units</span><input name="equipment" type="number" value="2" /></label>
       <label class="field"><span>Assistants</span><input name="assistants" type="number" value="1" /></label>
       </div><div class="form__actions"><button class="button" type="button">Save this quote</button></div></form>
       <aside class="panel panel--soft"><h2>Estimate</h2>
       <p class="price__line"><span>Wedding coverage · 8 hours</span><span>$2,200.00</span></p>
       <p class="price__line"><span>Equipment rental · 2 units</span><span>$240.00</span></p>
       <p class="price__line"><span>Advance booking discount</span><span>−$120.00</span></p>
       <p class="price__total">$2,320.00 CAD</p></aside></div>`,
    validation: () =>
      `${header('Quotation', 'Your quote', 'Totals recalculate as you change the session.')}
       <div class="layout__split"><form class="form"><h2>Session</h2><div class="form__grid">
       <label class="field"><span>Hours</span><input name="hours" type="number" value="0" aria-describedby="hours-error" /><small class="field__error" id="hours-error">Enter one hour or more.</small></label>
       <label class="field"><span>Address</span><input name="address" value="" aria-describedby="address-error" /><small class="field__error" id="address-error">Choose a resolved address.</small></label>
       </div><div class="form__actions"><button class="button" type="button">Save this quote</button></div></form>
       <aside class="panel panel--soft"><h2>Estimate</h2>${notice('Correct the highlighted fields to see a total.', true)}</aside></div>`,
    unconfigured: () =>
      `${header('Quotation', 'Your quote', 'Totals recalculate as you change the session.')}
       <div class="layout__split"><form class="form"><h2>Session</h2><div class="form__grid">
       <label class="field"><span>Service</span><select name="service"><option>Wedding</option></select></label>
       <label class="field"><span>Hours</span><input name="hours" type="number" value="8" /></label>
       </div></form>
       <aside class="panel panel--soft"><h2>Estimate</h2>${notice('Quoting is unavailable until studio rates are configured.', true)}<p class="text--muted">No total is shown, and no zero amount is implied.</p></aside></div>`,
  },
  'admin-records': {
    populated: () =>
      `${header('Studio administration', 'Sessions', 'Every booked session, its photographer, and its schedule.', '<button class="button" type="button">Add session</button>')}
       <div class="records">
       <article class="records__row"><div><h3>Riverside ceremony</h3><p class="records__detail">Saturday 14 June · Wedding · Amara Bell</p></div><div class="records__actions"><a href="#">Open session →</a><button class="button button--secondary" type="button">Edit</button></div></article>
       <article class="records__row"><div><h3>Harbour headshots</h3><p class="records__detail">Tuesday 17 June · Headshots · Unassigned</p></div><div class="records__actions"><a href="#">Open session →</a><button class="button button--secondary" type="button">Edit</button></div></article>
       </div>`,
    empty: () =>
      `${header('Studio administration', 'Sessions', 'Every booked session, its photographer, and its schedule.', '<button class="button" type="button">Add session</button>')}
       <div class="empty-state"><span aria-hidden="true">◇</span><p>No sessions yet.</p></div>`,
    loading: () =>
      `${header('Studio administration', 'Sessions', 'Every booked session, its photographer, and its schedule.')}
       <p role="status">Loading…</p>`,
  },
  'session-review': {
    ready: () =>
      `${header('Session', 'Riverside ceremony', 'Review the session and choose the photographs worth keeping.', '<button class="button" type="button">Publish selection</button>')}
       ${notice('Suggestions are advisory. A photographer decides what is delivered.')}
       ${photoGrid([
         photoTile('Ceremony 014'),
         photoTile('Ceremony 015'),
         photoTile('Ceremony 016'),
         photoTile('Ceremony 017'),
       ])}`,
    processing: () =>
      `${header('Session', 'Riverside ceremony', 'Review the session and choose the photographs worth keeping.')}
       ${photoGrid([
         photoTile('Ceremony 018', 'Processing', true),
         photoTile('Ceremony 019', 'Processing', true),
         photoTile('Ceremony 020', 'Ready'),
       ])}
       <p class="text--muted">Processing continues in the background; the page never fabricates a preview.</p>`,
  },
  'client-gallery': {
    assigned: () =>
      `${header('Your gallery', 'Riverside ceremony', 'Available until 14 December 2026.', '<a class="button button--secondary" href="#">Create an album</a>')}
       ${photoGrid([photoTile('Ceremony 014'), photoTile('Ceremony 015'), photoTile('Ceremony 016')])}`,
    revoked: () =>
      `${header('Your gallery', 'Riverside ceremony', 'Access to this gallery has ended.')}
       ${notice('This gallery is no longer available. Contact the studio if you need access.', true)}
       <div class="empty-state"><span aria-hidden="true">◇</span><p>No photographs available.</p></div>`,
  },
  'client-prints': {
    estimate: () =>
      `${header('Prints', 'Request prints', 'Sizes and prices are confirmed by the studio before any order is placed.')}
       <div class="layout__split"><section class="panel"><h2>Selected photographs</h2>
       <div class="records">
       <article class="records__row"><div><h3>Ceremony 014</h3><p class="records__detail">8 × 10 · Lustre</p></div><div class="records__actions"><button class="button button--secondary" type="button">Remove</button></div></article>
       <article class="records__row"><div><h3>Portrait 002</h3><p class="records__detail">5 × 7 · Matte</p></div><div class="records__actions"><button class="button button--secondary" type="button">Remove</button></div></article>
       </div></section>
       <aside class="panel panel--soft"><h2>Estimate</h2><p class="price__line"><span>8 × 10 · Lustre</span><span>$48.00</span></p><p class="price__line"><span>5 × 7 · Matte</span><span>$32.00</span></p><p class="price__total">$80.00 CAD</p><div class="form__actions"><button class="button" type="button">Submit request</button></div></aside></div>`,
    submitted: () =>
      `${header('Prints', 'Request submitted', 'The studio reviews every request before confirming it.')}
       ${notice('Your request was received. Prices are held at the moment of submission.')}
       <section class="panel"><p class="price__line"><span>Request reference</span><span>PR-2026-0184</span></p><p class="price__line"><span>Submitted</span><span>6 September 2026</span></p><p class="price__total">$80.00 CAD</p></section>`,
  },
  authentication: {
    'sign-in': () =>
      `<div class="layout__split"><form class="form"><h2>Sign in</h2><div class="form__grid">
       <label class="field"><span>Email</span><input name="email" type="email" autocomplete="username" /></label>
       <label class="field"><span>Password</span><input name="password" type="password" autocomplete="current-password" /></label>
       </div><div class="form__actions"><button class="button" type="button">Sign in</button><a href="#">Forgot your password?</a></div></form>
       <aside class="panel panel--soft"><h2>Private galleries</h2><p class="text--muted">Clients receive an invitation by email. Sessions appear once the studio assigns them.</p></aside></div>`,
    invalid: () =>
      `<div class="layout__split"><form class="form"><h2>Sign in</h2>${notice('Those credentials were not accepted.', true)}<div class="form__grid">
       <label class="field"><span>Email</span><input name="email" type="email" value="client@example.com" /></label>
       <label class="field"><span>Password</span><input name="password" type="password" /></label>
       </div><div class="form__actions"><button class="button" type="button">Sign in</button></div></form>
       <aside class="panel panel--soft"><h2>Private galleries</h2><p class="text--muted">The same message is shown whether or not an account exists.</p></aside></div>`,
  },
  'print-review': {
    inbox: () =>
      `${header('Studio administration', 'Print requests', 'Requests wait for review before the studio confirms them.')}
       <div class="records">
       <article class="records__row"><div><h3>PR-2026-0184 · Amara Bell</h3><p class="records__detail">Submitted 6 September · 2 photographs · $80.00 CAD</p></div><div class="records__actions"><span class="badge">Submitted</span><button class="button" type="button">Review</button></div></article>
       <article class="records__row"><div><h3>PR-2026-0181 · Daniel Osei</h3><p class="records__detail">Submitted 2 September · 4 photographs · $164.00 CAD</p></div><div class="records__actions"><span class="badge">Reviewed</span><a href="#">Open →</a></div></article>
       </div>`,
    reviewed: () =>
      `${header('Studio administration', 'PR-2026-0184', 'The submitted lines and their prices never change after review.')}
       ${notice('Reviewed by the studio on 6 September 2026.')}
       <div class="layout__split"><section class="panel"><h2>Requested photographs</h2>
       <p class="price__line"><span>Ceremony 014 · 8 × 10 · Lustre</span><span>$48.00</span></p>
       <p class="price__line"><span>Portrait 002 · 5 × 7 · Matte</span><span>$32.00</span></p>
       <p class="price__total">$80.00 CAD</p></section>
       <aside class="panel panel--soft"><h2>Client</h2><p class="records__detail">Amara Bell</p><p class="text--muted">Prices were held at submission.</p></aside></div>`,
  },
  'admin-settings': {
    rates: () =>
      `${header('Studio administration', 'Quote rates', 'Rates drive every quote; a saved change applies to later calculations.')}
       <form class="form"><h2>Service rates</h2><div class="form__grid">
       <label class="field"><span>Wedding · per hour</span><input type="number" name="wedding" value="275" /></label>
       <label class="field"><span>Event · per hour</span><input type="number" name="event" value="210" /></label>
       <label class="field"><span>Travel · per kilometre</span><input type="number" name="travel" value="0.72" step="0.01" /></label>
       <label class="field"><span>Assistant · per person</span><input type="number" name="assistant" value="180" /></label>
       </div><div class="form__actions"><button class="button" type="button">Save rates</button></div></form>`,
    discounts: () =>
      `${header('Studio administration', 'Discount rules', 'One eligible discount applies; the largest wins without stacking.')}
       <form class="form"><h2>Advance booking</h2><div class="form__grid">
       <label class="field"><span>Days in advance</span><input type="number" name="days" value="90" /></label>
       <label class="field"><span>Percentage</span><input type="number" name="percentage" value="10" /></label>
       <label class="field"><span>Code</span><input name="code" value="HELLO12" /></label>
       <label class="field"><span>Enabled</span><input type="checkbox" checked /></label>
       </div>${notice('A rule starts disabled at zero percent until a studio administrator sets it.')}<div class="form__actions"><button class="button" type="button">Save rules</button></div></form>`,
  },
  'system-states': {
    'not-found': () =>
      `<section class="panel"><p class="page__eyebrow">404</p><h1>That page is not here.</h1><p class="page__description">The link may be old, or the gallery may have been unpublished.</p><div class="form__actions"><a class="button" href="#">Return home</a></div></section>`,
    'access-denied': () =>
      `<section class="panel"><p class="page__eyebrow">403</p><h1>This area is not available to your account.</h1><p class="page__description">Client accounts see their own galleries; administration requires a studio account.</p><div class="form__actions"><a class="button" href="#">Go to your galleries</a></div></section>`,
    'service-error': () =>
      `<section class="panel"><p class="page__eyebrow">503</p><h1>The studio service is unavailable.</h1><p class="page__description">Nothing was saved. Try again in a few minutes.</p><div class="form__actions"><button class="button" type="button">Try again</button></div></section>`,
  },
};

const dialogs = {
  confirm: {
    destructive: () =>
      `<div><button class="button button--danger" type="button" data-dialog-open="dialog-destructive">Delete photographs</button>
       <dialog class="dialog" id="dialog-destructive" aria-labelledby="dialog-destructive-title">
       <header class="dialog__header"><h2 id="dialog-destructive-title">Delete 24 photographs?</h2><button type="button" aria-label="Close dialog" data-dialog-close>×</button></header>
       <div class="dialog__body"><p>Deleted photographs cannot be recovered, and client access ends immediately.</p>
       <div class="form__actions"><button class="button button--danger" type="button" data-dialog-close>Delete</button><button class="button button--secondary" type="button" data-dialog-close>Keep them</button></div></div></dialog></div>`,
    publish: () =>
      `<div><button class="button" type="button" data-dialog-open="dialog-publish">Publish gallery</button>
       <dialog class="dialog" id="dialog-publish" aria-labelledby="dialog-publish-title">
       <header class="dialog__header"><h2 id="dialog-publish-title">Publish this gallery?</h2><button type="button" aria-label="Close dialog" data-dialog-close>×</button></header>
       <div class="dialog__body"><p>Published galleries are visible to anyone with the link. Private client assignments are unchanged.</p>
       <div class="form__actions"><button class="button" type="button" data-dialog-close>Publish</button><button class="button button--secondary" type="button" data-dialog-close>Cancel</button></div></div></dialog></div>`,
  },
  lightbox: {
    photo: () =>
      `<div><button class="button button--secondary" type="button" data-dialog-open="dialog-lightbox">Open photograph</button>
       <dialog class="dialog" id="dialog-lightbox" aria-labelledby="dialog-lightbox-title">
       <header class="dialog__header"><h2 id="dialog-lightbox-title">Ceremony 014</h2><button type="button" aria-label="Close dialog" data-dialog-close>×</button></header>
       <div class="dialog__body"><div class="image__preview" style="aspect-ratio:3/2;display:flex;align-items:center;justify-content:center;background:#e9eae2;color:#74766e">Photograph 3 of 48</div></div></dialog></div>`,
    unavailable: () =>
      `<div><button class="button button--secondary" type="button" data-dialog-open="dialog-lightbox-unavailable">Open photograph</button>
       <dialog class="dialog" id="dialog-lightbox-unavailable" aria-labelledby="dialog-lightbox-unavailable-title">
       <header class="dialog__header"><h2 id="dialog-lightbox-unavailable-title">Album 004</h2><button type="button" aria-label="Close dialog" data-dialog-close>×</button></header>
       <div class="dialog__body">${notice('This photograph is no longer available.', true)}</div></dialog></div>`,
  },
};

export function patternMarkup(familyId, scenarioId) {
  return patterns[familyId]?.[scenarioId]?.() ?? '';
}

export function dialogMarkup(familyId, scenarioId) {
  return dialogs[familyId]?.[scenarioId]?.() ?? '';
}

export function componentMarkup(component, exampleId) {
  const example = component.examples.find((entry) => entry.id === exampleId) ?? component.examples[0];
  return example?.markup ?? '';
}

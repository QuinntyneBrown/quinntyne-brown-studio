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

const blogPost = (isoDate, date, title) =>
  `<article class="blog-post"><a href="#"><span class="blog-post__image" aria-hidden="true"></span><time datetime="${isoDate}">${date}</time><h2>${title}</h2></a></article>`;

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
  'blog-listing': {
    published: () =>
      `<section class="blog-page"><header class="blog-head"><p class="page__eyebrow">From the studio</p><h1>Blog</h1><p>Notes on making photographs feel easy, personal, and true to the people in them.</p></header><div class="blog-grid">${[
        ['2026-08-28', 'August 28, 2026', 'How to make room for the unscripted moments'],
        ['2026-08-14', 'August 14, 2026', 'What a calm wedding morning looks like in photographs'],
        ['2026-07-30', 'July 30, 2026', 'A guide to choosing a place for family portraits'],
        ['2026-07-11', 'July 11, 2026', 'Why the best event photographs happen between the big moments'],
        ['2026-06-19', 'June 19, 2026', 'Headshots that still feel like you'],
        ['2026-05-29', 'May 29, 2026', 'From ceremony to last dance: building a wedding timeline'],
      ].map(([isoDate, date, title]) => blogPost(isoDate, date, title)).join('')}</div></section>`,
    empty: () =>
      `<section class="blog-page"><header class="blog-head"><p class="page__eyebrow">From the studio</p><h1>Blog</h1><p>Notes on making photographs feel easy, personal, and true to the people in them.</p></header><div class="blog-empty"><h2>No articles yet</h2><p>Articles will appear here once they are published. Check back soon.</p></div></section>`,
  },
  'about-page': {
    team: () =>
      `<section class="hero"><div class="hero__copy"><p class="page__eyebrow">About the studio</p><h1>Photographs that feel like you.</h1><p>A small Toronto studio photographing weddings, events, headshots, and family portraits the way they actually feel: unhurried, honest, and a little bit wild.</p><div class="form__actions"><a class="button" href="#">Say hello ↗</a><a href="#">See the work</a></div><div class="hero__meta"><span>Since 2018</span><span>Toronto &amp; beyond</span><span>3 photographers</span></div></div><div class="hero__image"><div class="hero__placeholder"><span>Q B</span><p>Ordinary magic / Weddings</p></div></div></section>
       <section class="section story"><div><p class="page__eyebrow">How it started</p><h2>A borrowed camera, a friend’s wedding, and a lot of listening.</h2></div><div class="story__body"><p>Quinntyne Brown Studio began in 2018 with one camera and one favour: photographing a friend’s small backyard wedding. The photographs that mattered most weren’t the posed ones.</p><p>That afternoon became the way we work. We plan carefully so that the day itself can be unscripted.</p></div></section>
       <section class="section"><div class="section__header"><div><p class="page__eyebrow">What we believe</p><h2>Three things we won’t compromise on.</h2></div></div><div class="steps">${[
         ['Less posing. More being.', 'We give gentle direction and then get out of the way.'],
         ['Planning that feels like a conversation.', 'Timelines, locations, and the small details are settled together.'],
         ['Honest pricing, from the first estimate.', 'Photography, travel, and extras are itemized before you commit.'],
       ]
         .map(([title, body], index) => `<div class="step"><div class="step__number">0${index + 1}</div><h3>${title}</h3><p>${body}</p></div>`)
         .join('')}</div></section>
       <section class="section"><div class="section__header"><div><p class="page__eyebrow">The people</p><h2>Behind the camera.</h2></div><a href="#">Check availability ↗</a></div><div class="team-grid">${[
         ['QB', 'Quinntyne Brown', 'Founder &amp; lead photographer'],
         ['MA', 'Mara Adeyemi', 'Photographer'],
         ['JL', 'Jonah Lindqvist', 'Photographer'],
       ]
         .map(([mark, name, role]) => `<article class="team-card"><div class="team-card__mark" aria-hidden="true">${mark}</div><div><h3>${name}</h3><p class="team-card__role">${role}</p></div></article>`)
         .join('')}</div><p class="team-note">Alongside the studio is a trusted circle of second shooters, makeup artists, and assistants who join us when a day calls for more hands.</p></section>
       <section class="section quote-strip"><div><p class="page__eyebrow">Thoughtfully planned. Honestly priced.</p><h2>Let’s make something together.</h2></div><div class="form__actions"><a class="button" href="#">Get in touch ↗</a><a href="#">Find your quote</a></div></section>`,
    empty: () =>
      `<section class="hero"><div class="hero__copy"><p class="page__eyebrow">About the studio</p><h1>Photographs that feel like you.</h1><p>A small Toronto studio photographing weddings, events, headshots, and family portraits the way they actually feel: unhurried, honest, and a little bit wild.</p><div class="form__actions"><a class="button" href="#">Say hello ↗</a><a href="#">See the work</a></div><div class="hero__meta"><span>Since 2018</span><span>Toronto &amp; beyond</span></div></div><div class="hero__image"><div class="hero__placeholder"><span>Q B</span><p>A photograph is on its way</p></div></div></section>
       <section class="section"><div class="section__header"><div><p class="page__eyebrow">The people</p><h2>Behind the camera.</h2></div></div><div class="empty-state"><span aria-hidden="true">◇</span><p>Introductions coming soon</p><p class="text--muted">Our photographers will appear here shortly.</p></div></section>`,
  },
  'contact-page': {
    configured: () =>
      `${header('Get in touch', 'Something beautiful starts with hello.', 'Tell us what you have in mind. We’ll find the right way to capture it.')}
       <div class="layout__split"><form class="form">${notice('We usually reply within two working days.')}<div class="form__grid">
       <label class="field"><span>Your name</span><input name="name" maxlength="200" required /></label>
       <label class="field"><span>Email address</span><input name="email" type="email" maxlength="254" required /></label>
       <label class="field"><span>Phone (optional)</span><input name="phone" type="tel" maxlength="50" /><small class="text--muted">Only if you’d rather talk it through.</small></label>
       <label class="field"><span>I’m interested in</span><select name="interest"><option>Wedding</option><option>Event</option><option>Headshots</option><option>Family portraits</option></select></label>
       </div><label class="field"><span>Tell us a little about your plans</span><textarea name="message" maxlength="4000" required></textarea></label>
       <div class="field__choices"><label><input type="checkbox" name="consent" required /> I’m happy for the studio to contact me about this request.</label></div>
       <div class="form__actions"><button class="button" type="button">Send your message ↗</button><span class="text--muted">No payment or booking is made.</span></div></form>
       <aside class="panel panel--soft contact-card"><p class="page__eyebrow">Toronto, Ontario</p><h2>Come say hello.</h2><dl class="detail-list"><div class="detail-list__row"><dt>Email</dt><dd><a href="#">hello@example.test</a></dd></div><div class="detail-list__row"><dt>Phone</dt><dd><a href="#">416-555-0100</a></dd></div><div class="detail-list__row"><dt>Studio</dt><dd>Daylight Studio<br />120 Sample Street, Toronto</dd></div><div class="detail-list__row"><dt>Hours</dt><dd>Monday – Saturday · 09:00 – 18:00</dd></div><div class="detail-list__row"><dt>Replies</dt><dd>Within two working days</dd></div></dl></aside></div>
       <section class="section"><div class="section__header"><div><p class="page__eyebrow">Where to find us</p><h2>Two studio spaces. Any location you love.</h2></div><a href="#">Plan a location ↗</a></div><div class="places"><div class="map" role="img" aria-label="Illustrative map showing the Daylight Studio location in Toronto"><svg viewBox="0 0 600 450" xmlns="http://www.w3.org/2000/svg" aria-hidden="true" focusable="false"><rect width="600" height="450" fill="#f5f5f0"/><rect y="362" width="600" height="88" fill="#e2e6e1"/><g stroke="#ffffff" stroke-width="14" stroke-linecap="round"><line x1="0" y1="150" x2="600" y2="140"/><line x1="0" y1="262" x2="600" y2="252"/></g><g transform="translate(305 206)"><circle cy="-14" r="18" fill="#242620"/><path d="M-15 -4 L0 30 L15 -4Z" fill="#242620"/><circle cy="-14" r="7" fill="#f5f5f0"/></g></svg><span class="map__label">Daylight Studio · Toronto</span></div><div>
       <div class="place"><div><h3>Daylight Studio</h3><p class="text--muted">120 Sample Street, Toronto</p></div><div class="place__meta"><span class="badge">Toronto</span><span>By appointment</span></div></div>
       <div class="place"><div><h3>The White Room</h3><p class="text--muted">48 Example Avenue, Hamilton</p></div><div class="place__meta"><span class="badge">Hamilton</span><span>By appointment</span></div></div>
       <div class="place"><div><h3>On location</h3><p class="text--muted">Parks, venues, kitchens, and the end of your own street. Travel is itemized in your quote.</p></div><div class="place__meta"><span class="badge">Anywhere</span></div></div></div></div></section>
       <section class="section"><p class="page__eyebrow">Before you write</p><h2>A few things people ask first.</h2><div class="steps">${[
         ['How far ahead should we book?', 'Weddings are usually reserved six to twelve months out. Booking at least 90 days ahead also earns 10% off.'],
         ['Do you travel?', 'Yes. Toronto is home, and round-trip distance is itemized in your estimate.'],
         ['How do we receive our photographs?', 'Through a private online gallery, edited with care, with fine art prints available to order.'],
       ]
         .map(([question, answer]) => `<div class="faq"><h3>${question}</h3><p>${answer}</p></div>`)
         .join('')}</div></section>
       <section class="section quote-strip"><div><p class="page__eyebrow">Prefer to start with numbers?</p><h2>Build an estimate in about a minute.</h2></div><a class="button" href="#">Find your quote ↗</a></section>`,
    empty: () =>
      `${header('Get in touch', 'Something beautiful starts with hello.', 'Tell us what you have in mind. We’ll find the right way to capture it.')}
       <div class="layout__split"><form class="form">${notice('We usually reply within two working days.')}<div class="form__grid">
       <label class="field"><span>Your name</span><input name="name" maxlength="200" required /></label>
       <label class="field"><span>Email address</span><input name="email" type="email" maxlength="254" required /></label>
       </div><div class="form__actions"><button class="button" type="button">Send your message ↗</button></div></form>
       <aside class="panel panel--soft contact-card"><p class="page__eyebrow">Toronto, Ontario</p><h2>Come say hello.</h2><dl class="detail-list"><div class="detail-list__row"><dt>Studio</dt><dd>On location, by appointment</dd></div></dl></aside></div>
       <section class="section"><div class="section__header"><div><p class="page__eyebrow">Where to find us</p><h2>On location, anywhere you love.</h2></div></div><div class="places"><div class="map" role="img" aria-label="Illustrative map showing the studio location in Toronto"><span class="map__label">Studio · Toronto</span></div><div><div class="empty-state"><span aria-hidden="true">◇</span><p>Studio spaces coming soon</p><p class="text--muted">We photograph on location while our studio spaces are being prepared.</p></div>
       <div class="place"><div><h3>On location</h3><p class="text--muted">Parks, venues, kitchens, and the end of your own street.</p></div><div class="place__meta"><span class="badge">Anywhere</span></div></div></div></div></section>`,
    sent: () =>
      `${header('Get in touch', 'Something beautiful starts with hello.', 'Tell us what you have in mind. We’ll find the right way to capture it.')}
       <div class="layout__split"><form class="form">${notice('Thank you. Your message was sent with reference QB-IN-1042. The studio will follow up within two working days.')}<div class="form__grid">
       <label class="field"><span>Your name</span><input name="name" maxlength="200" required /></label>
       <label class="field"><span>Email address</span><input name="email" type="email" maxlength="254" required /></label>
       </div><div class="form__actions"><button class="button" type="button">Send your message ↗</button></div></form>
       <aside class="panel panel--soft contact-card"><p class="page__eyebrow">Toronto, Ontario</p><h2>Come say hello.</h2><dl class="detail-list"><div class="detail-list__row"><dt>Replies</dt><dd>Within two working days</dd></div></dl></aside></div>`,
    validation: () =>
      `${header('Get in touch', 'Something beautiful starts with hello.', 'Tell us what you have in mind. We’ll find the right way to capture it.')}
       <div class="layout__split"><form class="form"><div class="form__grid">
       <label class="field"><span>Your name</span><input name="name" value="Priya Raman" maxlength="200" required /></label>
       <label class="field"><span>Email address</span><input name="email" type="email" value="priya@" aria-invalid="true" aria-describedby="error-email" required /><small class="field__error" id="error-email">Enter a valid email address.</small></label>
       </div><label class="field"><span>Tell us a little about your plans</span><textarea name="message" aria-invalid="true" aria-describedby="error-message" required></textarea><small class="field__error" id="error-message">This field is required.</small></label>
       <div class="form__actions"><button class="button" type="button">Send your message ↗</button></div></form>
       <aside class="panel panel--soft contact-card"><p class="page__eyebrow">Toronto, Ontario</p><h2>Come say hello.</h2><p class="text--muted">Nothing was sent. Correct the highlighted fields and try again.</p></aside></div>`,
  },
  'quote-calculator': {
    calculated: () => `${header('Your estimate', 'Your session, thoughtfully priced.', 'Explore a live estimate, shaped around your plans.')}
      <div class="layout__split"><section class="panel"><h2>The occasion</h2><div class="form__grid">
      <label class="field"><span>Photography service</span><select><option>Wedding</option><option>Event</option><option>Headshots</option><option>Family portraits</option></select></label>
      <label class="field"><span>Session date</span><input type="date" value="2027-06-01"></label>
      <label class="field"><span>Start time</span><input type="time" value="10:00" step="900"></label>
      <label class="field"><span>End time</span><input type="time" value="12:00" step="900"></label></div></section>
      <aside class="panel panel--soft"><h2>Your estimate</h2><div class="price__line"><span>Photography</span><span>200.00 CAD</span></div>
      <div class="price__line"><span>Travel</span><span>24.00 CAD</span></div><div class="price__line"><span>Advance booking · 10%</span><span>−22.40 CAD</span></div>
      <div class="price__line price__total"><span>Estimate</span><span>201.60 CAD</span></div>
      ${notice('A photographer is currently available.')}
      <p class="text--muted">CAD before tax. Tax and final consultation adjustments are excluded. Availability is indicative and does not reserve a session.</p></aside></div>`,
    validation: () => `${header('Quotation', 'Your quote', 'Correct the session details to continue.')}${notice('End time must be after start time.', true)}
      <label class="field"><span>End time</span><input type="time" value="09:00" aria-invalid="true" aria-describedby="time-error"><small id="time-error">End time must be after start time.</small></label>`,
    unavailable: () => `${header('Quotation', 'Your quote', 'Your session details are preserved.')}${notice('Quoting is unavailable until rates and a studio base are configured.', true)}` ,
    failure: () => `${header('Quotation', 'Your quote', 'Your session details are preserved.')}${notice('Driving distance could not be calculated.', true)}<button class="button" type="button">Retry estimate</button>`,
    incomplete: () => `${header('Quotation', 'Your quote', 'Begin with your session details.')}${notice('Add a resolved location to see an estimate.')}`,
    loading: () => `${header('Quotation', 'Your quote', 'Your previous estimate is no longer current.')}${notice('Updating your quote…')}`,
    lookup: () => `${header('Quotation', 'The places', 'Choose the address that matches your plans.')}
      <div class="stack"><button class="button button--secondary" type="button">Venue A · Select</button><button class="button button--secondary" type="button">Venue B · Select</button></div>`,
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
  'inquiry-review': {
    inbox: () =>
      `${header('Studio / Marketing', 'Inquiries', 'Messages from the contact page, ready for a reply.', '<a href="#">View contact page ↗</a>')}
       <div class="records">
       <article class="records__row"><div><h3>Priya Raman</h3><p class="records__detail">Wedding · Received 2 September 2026 · QB-IN-1041</p></div><div class="records__actions"><span class="badge">Submitted</span><button class="button button--secondary" type="button">Open inquiry</button></div></article>
       <article class="records__row"><div><h3>Daniel Okafor</h3><p class="records__detail">Headshots · Received 26 August 2026 · QB-IN-1040</p></div><div class="records__actions"><span class="badge">Reviewed</span><button class="button button--secondary" type="button">Open inquiry</button></div></article>
       </div>`,
    reviewed: () =>
      `${header('Marketing / Inquiries', 'Daniel Okafor', 'Headshots · Received 26 August 2026 · QB-IN-1040', '<a href="#">← All inquiries</a>')}
       <div class="layout__split"><section class="panel"><dl class="detail-list"><div class="detail-list__row"><dt>Reference</dt><dd>QB-IN-1040</dd></div><div class="detail-list__row"><dt>Name</dt><dd>Daniel Okafor</dd></div><div class="detail-list__row"><dt>Email</dt><dd><a href="#">daniel@example.test</a></dd></div><div class="detail-list__row"><dt>Phone</dt><dd class="text--muted">Not provided</dd></div><div class="detail-list__row"><dt>Interest</dt><dd>Headshots</dd></div><div class="detail-list__row"><dt>Received</dt><dd>26 August 2026</dd></div><div class="detail-list__row"><dt>Message</dt><dd>Looking for two headshot looks for a new role. &lt;b&gt;Markup stays text.&lt;/b&gt;</dd></div></dl></section>
       <aside class="panel panel--soft"><p class="page__eyebrow">Review status</p><span class="badge">Reviewed</span><h2>Looked at, and noted.</h2><p class="text--muted">Reviewed by the studio on 27 August 2026.</p><div class="form__actions"><button class="button" type="button" disabled>Mark reviewed</button></div></aside></div>`,
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
    'studio-details': () =>
      `${header('Studio administration', 'Studio details', 'These details appear on the public contact page. Leave a field empty to keep it off the page.')}
       <div class="layout__split"><form class="form"><div class="form__grid">
       <label class="field"><span>Studio email address</span><input name="email" type="email" value="hello@example.test" maxlength="254" /><small class="text--muted">Inquiry notifications go to this address.</small></label>
       <label class="field"><span>Phone number</span><input name="phone" type="tel" value="416-555-0100" maxlength="50" /></label>
       <label class="field"><span>Opening hours</span><input name="hours" value="Monday – Saturday · 09:00 – 18:00" maxlength="200" /><small class="text--muted">For example: Monday – Saturday · 09:00 – 18:00</small></label>
       <label class="field"><span>Typical reply time</span><input name="replyNote" value="Within two working days" maxlength="200" /><small class="text--muted">For example: Within two working days</small></label>
       </div><div class="form__actions"><button class="button" type="button">Save studio details</button><a href="#">View contact page ↗</a></div></form>
       <aside class="panel panel--soft"><p class="page__eyebrow">Good to know</p><h2>Only what you fill in is shown.</h2><p class="text--muted">An empty email or phone field simply leaves that row off the contact page. Nothing is invented for visitors.</p></aside></div>`,
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

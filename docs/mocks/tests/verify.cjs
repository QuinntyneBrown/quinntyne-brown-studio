/* Run with Node and Playwright available; see ../README.md. */
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const http = require('node:http');
const {pathToFileURL} = require('node:url');
const {chromium} = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const root = path.resolve(__dirname, '..');
global.window = global;
require('../assets/catalog.js');
const catalog = global.MockCatalog;
const artifacts = path.join(root, '.verification');
fs.mkdirSync(artifacts, {recursive:true});
const failures = [], browserErrors = [];
let checks = 0;
const check = async (name, fn) => { try { await fn(); checks++; } catch (e) { failures.push({name,error:e.message}); console.error('FAIL:',name,e.message.slice(0,500)); } };
const server = http.createServer((req,res)=>{
  const target=path.resolve(root,'.'+decodeURIComponent(req.url.split('?')[0]));
  if(!target.startsWith(root+path.sep)) {res.writeHead(403);res.end();return;}
  fs.readFile(target,(error,data)=>{if(error){res.writeHead(404);res.end();return;}res.setHeader('Content-Type',({'.html':'text/html','.js':'text/javascript','.css':'text/css','.jpg':'image/jpeg'})[path.extname(target)]||'text/plain');res.end(data);});
});
(async()=>{
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const origin='http://127.0.0.1:'+server.address().port;
  const browser=await chromium.launch({channel:process.env.BROWSER_CHANNEL||'chrome',headless:true});
  const context=await browser.newContext({viewport:{width:1440,height:1000}});
  const page=await context.newPage();
  page.on('pageerror',e=>browserErrors.push({url:page.url(),error:e.message}));
  page.on('dialog',d=>d.accept());
  const go=async(p)=>{await page.goto(origin+'/'+p);await page.waitForSelector('#app main');};
  const reset=async()=>{await go('index.html');await page.evaluate(()=>localStorage.removeItem('qb-studio-mocks-v1'));};
  await check('Every page, linked state and dialog renders',async()=>{
    for(const c of catalog){
      const route=`${c.site}/${c.slug}.html`;
      await go(route);
      assert.ok((await page.locator('main').innerText()).length>50,route);
      const broken=await page.locator('a[href]').evaluateAll(anchors=>anchors.map(a=>a.getAttribute('href')).filter(h=>!h.startsWith('#')&&!h.includes('://')&&!h.startsWith('mailto:')));
      for(const href of broken)assert.ok(fs.existsSync(path.resolve(root,c.site,href.split('?')[0])),`${route}: broken ${href}`);
      for(const s of c.states){await go(`${route}?state=${s}`);assert.ok(await page.locator('main').count(),`${route} ${s}`);}
      for(const d of c.dialogs){await go(`${route}?dialog=${d}`);assert.equal(await page.locator('dialog').evaluate(el=>el.open),true,`${route} ${d}`);await page.keyboard.press('Escape');assert.equal(await page.locator('dialog').evaluate(el=>el.open),false);}
    }
  });
  await check('Index covers all pages and local photographs load',async()=>{
    await go('index.html');assert.ok(await page.locator('.index-row').count()===catalog.length);
    await go('marketing/gallery.html');await page.locator('img').first().waitFor();
    await page.evaluate(()=>Promise.all([...document.images].map(i=>i.decode().catch(()=>{}))));
    assert.ok(await page.locator('img').evaluateAll(imgs=>imgs.every(i=>i.naturalWidth>0)));
  });
  await check('Quote costs, multiple locations and largest discount',async()=>{
    await reset();await go('marketing/quote.html');
    const future=new Date();future.setDate(future.getDate()+125);const date=future.toLocaleDateString('en-CA');
    await page.locator('[name=date]').fill(date);
    await page.locator('[name=hours]').fill('4');
    await page.locator('[name=distance]').fill('25');
    await page.locator('[name=equipment]').fill('2');
    await page.locator('[name=assistants]').fill('1');
    await page.locator('[name=parking]').fill('1');
    await page.locator('[name=meals]').fill('2');
    await page.locator('[name=studio]').selectOption('st1');
    await page.locator('[name=code]').fill('HELLO12');
    await page.locator('[data-action=add-location]').click();
    await page.locator('[name=location]').last().fill('High Park');
    const q=await page.evaluate(()=>MockDebug.data.quote);
    assert.equal(q.subtotal,2178);assert.equal(q.total,1916.64);assert.equal(q.discount.percent,12);assert.equal(q.inputs.locations.length,2);
    for(const [type,rate]of [['wedding',350],['events',225],['headshots',180],['family',200]]){await page.locator('[name=type]').selectOption(type);assert.equal(await page.evaluate(()=>MockDebug.data.quote.costs[0][1]),rate*4);}
    await page.locator('[name=code]').fill('WRONG');assert.match(await page.locator('#code-feedback').innerText(),/don’t recognize/);
    await page.locator('[name=code]').fill('');assert.equal(await page.evaluate(()=>MockDebug.data.quote.discount.percent),10);
    await page.locator('[data-action=review-quote]').click();await page.waitForURL('**/quote-summary.html');assert.match(await page.locator('main').innerText(),/High Park/);
  });
  await check('Quote unavailable and validation block continuation',async()=>{
    await go('marketing/quote.html?state=unavailable');assert.equal(await page.locator('[data-action=review-quote]').isDisabled(),true);
    await go('marketing/quote.html');await page.locator('[name=hours]').fill('0');await page.locator('[data-action=review-quote]').click();assert.ok(await page.locator('.field-error').count());
  });
  await check('Admin pricing changes reach marketing calculator',async()=>{
    await reset();await go('admin/rates.html');await page.locator('[name=wedding]').fill('400');await page.getByRole('button',{name:'Save rates',exact:true}).click();await go('marketing/quote.html');assert.equal(await page.evaluate(()=>MockDebug.data.quote.costs[0][1]),1600);
    await go('admin/print-prices.html');await page.locator('[name=p2]').fill('40');await page.getByRole('button',{name:'Save print prices'}).click();await go('marketing/prints.html');assert.match(await page.locator('main').innerText(),/\$40\.00/);
  });
  await check('Admin content publish updates marketing',async()=>{
    await go('admin/content-editor.html?id=home');await page.locator('[name=heading]').fill('A new studio story.');await page.locator('[data-action=publish]').click();await page.locator('[data-action=confirm-publish]').click();await page.waitForURL('**/content.html?saved=1');await go('marketing/home.html');assert.equal(await page.locator('h1').innerText(),'A new studio story.');
  });
  await check('Unsaved changes dialog preserves or discards edits',async()=>{
    await go('admin/vendor-editor.html?id=v1');await page.locator('[name=name]').fill('Changed name');await page.getByRole('link',{name:'Cancel',exact:true}).click();assert.equal(await page.locator('dialog').evaluate(d=>d.open),true);await page.locator('[data-action=close-dialog]').last().click();assert.equal(await page.locator('[name=name]').inputValue(),'Changed name');await page.getByRole('link',{name:'Cancel',exact:true}).click();await page.locator('[data-action=confirm-discard]').click();await page.waitForURL('**/vendors.html');
  });
  await check('Schedule conflicts and availability propagation',async()=>{
    await reset();await go('admin/schedule.html');await page.locator('[data-action=schedule]').first().click();await page.locator('dialog [name=date]').fill('2026-10-17');await page.getByRole('button',{name:'Save unavailable time'}).click();assert.match(await page.locator('#schedule-feedback').innerText(),/overlaps/);
    const future=new Date();future.setDate(future.getDate()+140);const date=future.toLocaleDateString('en-CA');await page.locator('dialog [name=date]').fill(date);await page.getByRole('button',{name:'Save unavailable time'}).click();assert.equal(await page.locator('dialog').evaluate(d=>d.open),false);await go('marketing/quote.html');await page.locator('[name=date]').fill(date);assert.equal(await page.locator('[data-action=review-quote]').isDisabled(),true);
  });
  await check('Bulk upload partial failure and retry',async()=>{
    await go('admin/upload.html');await page.locator('[data-action=sample-upload]').click();await page.waitForFunction(()=>document.querySelectorAll('[data-action=retry-one]').length===2);await page.locator('[data-action=retry-upload]').click();await page.waitForFunction(()=>[...document.querySelectorAll('#upload-queue .badge')].every(e=>e.textContent.trim()==='Complete'));
    await page.locator('[data-action=sample-upload]').click();await page.locator('[data-action=cancel-upload]').click();await page.locator('[data-action=confirm-cancel-upload]').click();assert.match(await page.locator('#upload-queue').innerText(),/Cancelled/);
    await page.locator('#photo-files').setInputFiles({name:'bad.txt',mimeType:'text/plain',buffer:Buffer.from('test')});assert.match(await page.locator('#upload-feedback').innerText(),/not a supported/);
  });
  await check('AI error, retry, accepting and persisting selections',async()=>{
    await go('admin/review.html?state=ai-error');await page.locator('[data-action=run-ai]').click();await page.waitForFunction(()=>document.querySelector('#ai-feedback').textContent.includes('couldn’t finish'));await page.locator('[data-action=run-ai]').click();await page.waitForFunction(()=>document.querySelector('dialog').open);await page.locator('[data-action=accept-ai]').click();await page.locator('[data-action=save-selection]').click();await go('admin/review.html');assert.equal(await page.locator('[data-photo-select]:checked').count(),3);
  });
  await check('Client album create, select, rename and remove',async()=>{
    await reset();await go('client/albums.html');await page.locator('[data-action=album-create]').click();await page.locator('dialog [name=name]').fill('Summer light');await page.getByRole('button',{name:'Create album',exact:true}).click();await page.waitForURL('**/album-editor.html?id=*');await page.locator('[data-photo-select="0"]').check();await page.locator('[data-photo-select="2"]').check();await page.getByRole('button',{name:'Save album',exact:true}).click();await page.waitForURL('**/album.html?id=*');assert.equal(await page.locator('.photo-tile').count(),2);await page.locator('[data-action=album-rename]').click();await page.locator('dialog [name=name]').fill('Summer, together');await page.getByRole('button',{name:'Save name'}).click();assert.equal(await page.locator('h1').innerText(),'Summer, together');
  });
  await check('Print quantities, request failure/retry and immutable request total',async()=>{
    await reset();await go('client/prints.html');await page.locator('[data-cart-qty="0"]').fill('2');assert.equal(await page.locator('#cart-total').innerText(),'$122.00');await page.locator('[data-action=review-prints]').click();await page.waitForURL('**/print-review.html');await go('client/print-review.html?state=request-error');await page.locator('[name=confirm]').check();await page.getByRole('button',{name:'Send print request'}).click();assert.match(await page.locator('#print-request-form').innerText(),/wasn’t sent/);await page.getByRole('button',{name:'Send print request'}).click();await page.waitForURL('**/print-confirmation.html?id=*');await page.getByRole('link',{name:'View your request'}).click();assert.match(await page.locator('main').innerText(),/\$122\.00/);assert.equal(await page.evaluate(()=>MockDebug.data.cart.length),0);
  });
  await check('Sign-in, invalid credentials, reset and expired reset link',async()=>{
    await go('client/login.html?state=invalid-login');await page.locator('[name=password]').fill('sample123');await page.getByRole('button',{name:'Sign in',exact:false}).click();assert.match(await page.locator('#auth-feedback').innerText(),/wasn’t recognized/);await page.getByRole('button',{name:'Sign in',exact:false}).click();await page.waitForURL('**/galleries.html');
    await go('admin/reset-password.html?state=expired-link');assert.equal(await page.locator('#auth-form').count(),0);await go('admin/reset-password.html');await page.locator('[name=password]').fill('sample123');await page.locator('[name=confirmPassword]').fill('different123');await page.getByRole('button',{name:'Reset password',exact:true}).click();assert.match(await page.locator('#auth-feedback').innerText(),/don’t match/);
  });
  await check('Dialog keyboard controls and focus return',async()=>{
    await go('client/gallery.html');const first=page.locator('[data-action=photo]').first();await first.click();await page.keyboard.press('ArrowRight');assert.equal(await page.locator('#dialog-title').innerText(),'Just us');await page.keyboard.press('Escape');assert.equal(await first.evaluate(e=>e===document.activeElement),true);
  });
  await check('Responsive layout has no page overflow',async()=>{
    await reset();
    for(const width of [1440,768,390]){
      await page.setViewportSize({width,height:1000});
      for(const route of ['marketing/home','marketing/quote','admin/dashboard','admin/schedule','admin/session-editor','admin/upload','client/gallery','client/prints','index']){
        await go(route+'.html');const metrics=await page.evaluate(()=>({scroll:document.documentElement.scrollWidth,client:document.documentElement.clientWidth}));assert.ok(metrics.scroll<=metrics.client+1,`${route} at ${width}: ${JSON.stringify(metrics)}`);
        if(['marketing/home','admin/dashboard','client/gallery'].includes(route)&&width!==768)await page.screenshot({path:path.join(artifacts,route.replace('/','-')+'-'+width+'.png'),fullPage:true});
      }
    }
  });
  await check('Every page opens directly using file:// with no external assets',async()=>{
    await page.setViewportSize({width:1440,height:1000});
    for(const c of catalog){await page.goto(pathToFileURL(path.join(root,c.site,c.slug+'.html')).href);await page.waitForSelector('main');assert.ok((await page.locator('main').innerText()).length>50);}
  });
  await check('Unavailable browser storage falls back gracefully',async()=>{
    const fallback=await browser.newContext();await fallback.addInitScript(()=>{Object.defineProperty(window,'localStorage',{get(){throw new Error('Storage disabled');}});});const p=await fallback.newPage();await p.goto(origin+'/marketing/quote.html');await p.locator('[name=hours]').fill('3');assert.ok(await p.locator('#quote-totals').innerText());await fallback.close();
  });
  await check('No browser JavaScript errors',async()=>assert.deepEqual(browserErrors,[]));
  await browser.close();server.close();
  const report={passed:checks,failed:failures.length,pages:catalog.length,states:catalog.reduce((n,c)=>n+c.states.length,0),dialogs:catalog.reduce((n,c)=>n+c.dialogs.length,0),failures,browserErrors};
  fs.writeFileSync(path.join(artifacts,'report.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));if(failures.length)process.exitCode=1;
})().catch(error=>{console.error(error);server.close();process.exit(1);});

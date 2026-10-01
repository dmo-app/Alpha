// Run with the proof host listening: node verify-shell.cjs [baseUrl]
// Differential behavior uses the accepted fixture owned by Alpha only.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { JSDOM, VirtualConsole } = require('jsdom');
const root = path.resolve(__dirname, '../../src/DMO.Alpha.Web/wwwroot');
const base = process.argv[2] || 'http://127.0.0.1:5190';
const baseline = path.join(__dirname, 'fixtures/accepted-boquilhas.html');
const route = '/31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html';

async function load(html, user = '1003', query = '?view=registo&tool_id=unchanged') {
  const errors = [];
  const console = new VirtualConsole();
  console.on('jsdomError', error => {
    if (!error.message.includes('navigation')) errors.push(error.message);
  });
  // CSS geometry is verified in the browser, not jsdom's partial CSS parser.
  const behavioralHtml = html.replace(/<style>[\s\S]*?<\/style>/g, '')
    .replace(/<link\b[^>]*rel="stylesheet"[^>]*>/g, '');
  const dom = new JSDOM(behavioralHtml, {
    url: base + route + query, runScripts: 'dangerously', resources: 'usable',
    pretendToBeVisual: true, virtualConsole: console,
    beforeParse(window) {
      if (user) window.sessionStorage.setItem('betaDemoUser', user);
      window.HTMLElement.prototype.scrollIntoView = function () {};
      const NativeDate = window.Date;
      window.Date = class extends NativeDate {
        constructor(...args) { super(...(args.length ? args : ['2026-10-01T10:00:00Z'])); }
        static now() { return new NativeDate('2026-10-01T10:00:00Z').getTime(); }
      };
    }
  });
  await new Promise(resolve => dom.window.addEventListener('load', resolve, { once: true }));
  assert.deepEqual(errors, [], 'module scripts must run without errors');
  return dom;
}
const compact = text => text.replace(/>\s+</g, '><').trim();
const content = document => ['.beta-bq-layout', '#movementModal', '#toast'].map(
  selector => compact(document.querySelector(selector).outerHTML));
const click = (dom, selector) => dom.window.document.querySelector(selector).click();
const storage = dom => {
  const store = dom.window.sessionStorage;
  return Object.fromEntries(Object.keys(store).sort().map(key => [key, store.getItem(key)]));
};

(async () => {
  const response = await fetch(base + route + '?view=registo&tool_id=unchanged');
  assert.equal(response.status, 200);
  const html = await response.text();
  const parsed = new JSDOM(html).window.document;
  if (baseline) {
    const source = new JSDOM(fs.readFileSync(baseline, 'utf8')).window.document;
    assert.deepEqual(content(parsed), content(source), 'original module markup copied exactly');
    assert.deepEqual([...parsed.querySelectorAll('style')].map(x => x.textContent),
      [...source.querySelectorAll('style')].map(x => x.textContent), 'module inline CSS unchanged');
    assert.deepEqual([...parsed.querySelectorAll('script:not([src])')].map(x => x.textContent),
      [...source.querySelectorAll('script:not([src])')].map(x => x.textContent), 'module inline JavaScript unchanged');
    assert.deepEqual([...parsed.querySelectorAll('script[src]')].map(x => x.getAttribute('src')),
      [...source.querySelectorAll('script[src]')].map(x => x.getAttribute('src')).filter(x => x !== 'beta-shell.js'),
      'only legacy header renderer omitted; module script order/query versions unchanged');
  }
  assert.equal(parsed.querySelectorAll('.dmo-shell-header').length, 1);
  assert.equal(parsed.querySelectorAll('.dmo-shell-navigation nav').length, 2);
  assert.equal(parsed.querySelector('.dmo-shell-module-title').textContent.trim(), 'Boquilhas');
  assert.equal(parsed.querySelector('[data-module-key]').dataset.moduleKey, 'boquilhas');
  assert.deepEqual([...parsed.querySelectorAll('.dmo-secondary-nav button')].map(x => x.textContent),
    ['Registo', 'Boquilhas', 'Histórico', 'Definições']);
  assert.deepEqual([...parsed.querySelectorAll('.dmo-primary-nav a')].map(x => x.textContent), ['Planeamento']);
  assert(!html.includes('src="beta-shell.js"'));
  assert(parsed.querySelector('.dmo-shell-logo'));
  assert.equal(parsed.querySelector('.dmo-shell-identity strong').textContent, 'Portal DMO');
  const manifest = JSON.parse(fs.readFileSync(path.join(__dirname, 'baseline-assets.json')));
  for (const name of Object.keys(manifest).filter(name => !name.startsWith('_'))) {
    const actual = crypto.createHash('sha256').update(fs.readFileSync(path.join(root, name))).digest('hex');
    assert.equal(actual, manifest[name],
      `Unrelated asset changed: ${name}`);
  }
  assert(require.resolve('jsdom').startsWith(path.join(__dirname, 'node_modules')), 'test dependency must resolve inside Alpha');
  for (const name of Object.keys(manifest)) {
    assert.equal((await fetch(`${base}/${name}`)).status, 200, `Alpha-owned asset: ${name}`);
  }
  assert.equal((await fetch(base + '/login')).status, 200, 'native Alpha login remains available');
  // Link identities are preserved; unconverted reference destinations are not hosted.
  const fixture = new JSDOM(fs.readFileSync(baseline, 'utf8')).window.document;
  assert.equal(parsed.querySelector('.dmo-primary-nav a').getAttribute('href'),
    fixture.querySelector('a[href="20_JOB_ON_01_VISUAL_AUTHORITY_job-on.html"]').getAttribute('href'));
  assert.equal((await fetch(base + '/20_JOB_ON_01_VISUAL_AUTHORITY_job-on.html')).status, 404,
    'no external reference fallback may serve unconverted pages');
  assert.equal((await fetch(base + '/Program.cs')).status, 404);

  for (const user of ['1003', '1001']) {
    const proof = await load(html, user);
    const doc = proof.window.document;
    assert.equal(doc.querySelector('[data-user-profile-name]').textContent,
      user === '1003' ? 'Rui Costa' : 'João Silva');
    assert.equal(doc.querySelectorAll('.dmo-shell-logout').length, 1);
    let original;
    if (baseline) {
      original = await load(fs.readFileSync(baseline, 'utf8'), user);
      assert.deepEqual(content(doc), content(original.window.document), 'initial module content unchanged');
    }
    for (const key of ['boquilhas', 'historico', 'definicoes', 'registo']) {
      click(proof, `.tab[data-view="${key}"]`);
      assert.equal(doc.querySelector('.view.active').id, key);
      assert.equal(proof.window.location.search, '?view=registo&tool_id=unchanged');
      if (original) {
        click(original, `.tab[data-view="${key}"]`);
        assert.deepEqual(content(doc), content(original.window.document), `${key} behavior unchanged`);
      }
    }
    // Existing module actions: select a production rail line, open a trace,
    // save a movement, inspect history, then save a line's repairer setting.
    for (const selector of ['#bqCurrentLines [data-line="B3"]', '#bqTraceList button', '[data-movement="Saída"]']) {
      click(proof, selector);
      if (original) click(original, selector);
    }
    assert(doc.querySelector('#movementModal').classList.contains('open'));
    for (const dom of [proof, original].filter(Boolean)) {
      const form = dom.window.document.querySelector('#movementForm');
      form.querySelector('input[type="number"]').value = '2';
      form.dispatchEvent(new dom.window.Event('submit', { bubbles: true, cancelable: true }));
      click(dom, '.tab[data-view="historico"]');
      click(dom, '.tab[data-view="definicoes"]');
      click(dom, '#repairerLines button');
      const repairerForm = dom.window.document.querySelector('#repairerForm');
      repairerForm.querySelector('select').value = 'Externo B';
      repairerForm.dispatchEvent(new dom.window.Event('submit', { bubbles: true, cancelable: true }));
    }
    if (original) {
      assert.deepEqual(content(doc), content(original.window.document), 'movement/history/settings content unchanged');
      assert.deepEqual(storage(proof), storage(original), 'movement/settings/session persistence unchanged');
    }
    click(proof, '.dmo-shell-logout');
    assert.equal(proof.window.sessionStorage.getItem('betaDemoUser'), null);
    proof.window.close();
    original?.window.close();
  }
  // Existing query semantics: only registo is recognized by the prototype;
  // other view parameters do not gain new behavior during shell extraction.
  for (const query of ['?view=historico', '?view=definicoes', '?view=unknown']) {
    const proof = await load(html, '1003', query);
    assert.equal(proof.window.document.querySelector('.view.active').id, 'registo');
    if (baseline) {
      const original = await load(fs.readFileSync(baseline, 'utf8'), '1003', query);
      assert.deepEqual(content(proof.window.document), content(original.window.document));
      original.window.close();
    }
    proof.window.close();
  }
  console.log('PASS: Razor composition, module isolation, Alpha-owned assets and unchanged Boquilhas route/query semantics, operator/chief session, all four functions, rail, movement, history, settings, query semantics and logout.');
  console.log(baseline ? 'PASS: differential DOM and sessionStorage checks against the accepted proof.' : 'Original baseline not supplied: differential checks skipped.');
})().catch(error => { console.error(error); process.exitCode = 1; });

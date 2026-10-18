import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
import { gzipSync } from 'node:zlib';
import { fileURLToPath } from 'node:url';
import { createGameServer, resolveRequest, validateBuild } from '../Tools/Serve-Web.mjs';

// These are isolated HTTP fixtures, not a Unity build or a playable game.
const results = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../TestResults');
fs.mkdirSync(results, { recursive: true });
const fixture = fs.mkdtempSync(path.join(results, 'local-server-'));
const artifacts = path.join(fixture, 'Build');

test('local game server and build prerequisite checks', async t => {
  t.after(() => {
    const target = fs.realpathSync(fixture);
    assert.equal(path.dirname(target).toLowerCase(), fs.realpathSync(results).toLowerCase());
    fs.rmSync(target, { recursive: true });
  });
  await t.test('missing build is rejected', () => assert.throws(() => validateBuild(fixture), /No Unity Web build/));
  fs.writeFileSync(path.join(fixture, 'index.html'), '<!doctype html><title>HTTP test fixture only</title>');
  await t.test('an HTML page alone is not accepted as a Unity build', () => assert.throws(() => validateBuild(fixture), /loader.*missing/));
  fs.mkdirSync(artifacts);
  fs.writeFileSync(path.join(artifacts, 'test.loader.js'), '// HTTP fixture');
  fs.writeFileSync(path.join(artifacts, 'test.framework.js'), '// HTTP fixture');
  fs.writeFileSync(path.join(artifacts, 'test.wasm'), Buffer.from([0, 97, 115, 109, 1, 0, 0, 0]));
  await t.test('missing game data is rejected', () => assert.throws(() => validateBuild(fixture), /game data.*missing/));
  fs.writeFileSync(path.join(artifacts, 'test.data'), 'fixture');
  fs.writeFileSync(path.join(artifacts, 'compressed.wasm.gz'), gzipSync('fixture'));
  await t.test('expected Unity file types pass the file presence check', () => assert.doesNotThrow(() => validateBuild(fixture)));
  await t.test('Windows traversal and malformed URL encoding are rejected', () => {
    assert.throws(() => resolveRequest('/..%5c..%5cprivate.txt', fixture));
    assert.throws(() => resolveRequest('/%', fixture));
  });

  const server = createGameServer(fixture);
  await new Promise((resolve, reject) => { server.once('error', reject); server.listen(0, '127.0.0.1', resolve); });
  t.after(() => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }));
  const port = server.address().port;
  function request(url, method = 'GET') {
    return new Promise((resolve, reject) => {
      const req = http.request({ host: '127.0.0.1', port, path: url, method }, response => {
        const chunks = []; response.on('data', chunk => chunks.push(chunk));
        response.on('end', () => resolve({ status: response.statusCode, headers: response.headers, body: Buffer.concat(chunks) }));
      });
      req.on('error', reject); req.end();
    });
  }
  await t.test('index is served through HTTP', async () => {
    const r = await request('/'); assert.equal(r.status, 200); assert.match(r.headers['content-type'], /text\/html/); assert.match(r.body.toString(), /HTTP test fixture only/);
  });
  await t.test('WebAssembly has the browser-required MIME type', async () => {
    const r = await request('/Build/test.wasm'); assert.equal(r.status, 200); assert.equal(r.headers['content-type'], 'application/wasm'); assert.equal(r.headers['content-encoding'], undefined);
  });
  await t.test('compressed WebAssembly preserves MIME and encoding', async () => {
    const r = await request('/Build/compressed.wasm.gz'); assert.equal(r.headers['content-type'], 'application/wasm'); assert.equal(r.headers['content-encoding'], 'gzip');
  });
  await t.test('HEAD returns metadata without a body', async () => {
    const r = await request('/Build/test.wasm', 'HEAD'); assert.equal(r.status, 200); assert.equal(r.headers['content-length'], '8'); assert.equal(r.body.length, 0);
  });
  await t.test('missing files and unsupported methods return explicit status codes', async () => {
    assert.equal((await request('/missing')).status, 404); assert.equal((await request('/', 'POST')).status, 405); assert.equal((await request('/%')).status, 400);
  });
  await t.test('health response identifies the project for safe server reuse', async () => {
    const r = await request('/__frontier_health'); const health = JSON.parse(r.body.toString()); assert.equal(health.game, 'CubeFrontierEclipse'); assert.equal(health.root, fixture);
  });
});

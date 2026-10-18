import http from 'node:http';
import path from 'node:path';
import fs from 'node:fs';
import { execFile } from 'node:child_process';
import { fileURLToPath } from 'node:url';

export const buildRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../CubeFrontier/Builds/Web');
const mime = { '.html':'text/html; charset=utf-8', '.js':'application/javascript', '.wasm':'application/wasm', '.data':'application/octet-stream', '.json':'application/json', '.png':'image/png', '.ico':'image/x-icon', '.css':'text/css; charset=utf-8' };

export function validateBuild(root = buildRoot) {
  if (!fs.existsSync(path.join(root, 'index.html'))) throw new Error('No Unity Web build exists. Run Start-Game.bat with Unity Editor installed to build it.');
  const directory = path.join(root, 'Build');
  const files = fs.existsSync(directory) ? fs.readdirSync(directory, { withFileTypes: true }).filter(entry => entry.isFile()).map(entry => entry.name) : [];
  for (const [name, pattern] of [['loader', /\.loader\.js$/], ['framework', /\.framework\.js(?:\.br|\.gz)?$/], ['WebAssembly', /\.wasm(?:\.br|\.gz)?$/], ['game data', /\.data(?:\.br|\.gz)?$/]]) {
    if (!files.some(file => pattern.test(file))) throw new Error(`Incomplete Unity Web build: ${name} file is missing from ${directory}. Rebuild with Start-Game.bat -Rebuild.`);
  }
}

export function resolveRequest(url, root = buildRoot) {
  const pathname = decodeURIComponent(new URL(url, 'http://localhost').pathname);
  const file = path.resolve(root, '.' + (pathname === '/' ? '/index.html' : pathname));
  if (!file.startsWith(path.resolve(root) + path.sep)) throw new Error('Outside build directory');
  return file;
}

export function createGameServer(root = buildRoot) {
  root = path.resolve(root);
  return http.createServer((req, res) => {
    if (req.method !== 'GET' && req.method !== 'HEAD') { res.writeHead(405, { Allow: 'GET, HEAD' }); res.end('Method not allowed'); return; }
    if (req.url === '/__frontier_health') {
      res.writeHead(200, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' });
      res.end(req.method === 'HEAD' ? undefined : JSON.stringify({ game: 'CubeFrontierEclipse', root })); return;
    }
    let file;
    try { file = resolveRequest(req.url, root); } catch { res.writeHead(400); res.end('Invalid path'); return; }
    fs.stat(file, (err, stat) => {
      if (err || !stat.isFile()) { res.writeHead(404); res.end('Not found'); return; }
      let extension = path.extname(file), encoding;
      if (extension === '.br' || extension === '.gz') { encoding = extension === '.br' ? 'br' : 'gzip'; extension = path.extname(file.slice(0, -extension.length)); }
      const headers = { 'Content-Type': mime[extension] || 'application/octet-stream', 'X-Content-Type-Options': 'nosniff', 'Cache-Control': 'no-cache', 'Content-Length': stat.size };
      if (encoding) headers['Content-Encoding'] = encoding;
      res.writeHead(200, headers);
      if (req.method === 'HEAD') { res.end(); return; }
      fs.createReadStream(file).on('error', () => res.destroy()).pipe(res);
    });
  });
}

function openBrowser(url) {
  const command = process.platform === 'win32' ? 'rundll32.exe' : process.platform === 'darwin' ? 'open' : 'xdg-open';
  const args = process.platform === 'win32' ? ['url.dll,FileProtocolHandler', url] : [url];
  execFile(command, args, { windowsHide: true }, error => { if (error) console.error(`Could not open the browser automatically. Open ${url} yourself.`); });
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    validateBuild();
    if (process.argv.includes('--check')) console.log('Unity Web build files are present.');
    else {
      const port = Number(process.env.PORT || 8080);
      if (!Number.isInteger(port) || port < 1 || port > 65535) throw new Error('PORT must be between 1 and 65535.');
      const server = createGameServer();
      server.on('error', error => {
        console.error(error.code === 'EADDRINUSE' ? `Port ${port} is in use. Close the previous local server or select another PORT. Changing ports changes the browser save location.` : error.message);
        process.exitCode = 1;
      });
      server.listen(port, '127.0.0.1', () => {
        const url = `http://127.0.0.1:${port}`;
        console.log(`Cube Frontier: Eclipse is served at ${url}`);
        console.log('Keep this window open while playing. Ctrl+C stops the server.');
        if (process.argv.includes('--open-browser')) openBrowser(url);
      });
      process.on('SIGINT', () => { server.close(); server.closeAllConnections(); });
      process.on('SIGTERM', () => { server.close(); server.closeAllConnections(); });
    }
  } catch (error) {
    if (process.argv.includes('--check')) console.log(error.message); else console.error(error.message);
    process.exitCode = 1;
  }
}

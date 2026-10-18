# Web build and deployment preparation

There is no generated Web build or hosted game yet. These instructions describe the next build/deployment steps; they are not evidence of a successful deployment.

## Build

Install Unity 6000.3.25f1 + Web Build Support and ensure license activation. Import `CubeFrontier`, resolve packages and clear Console errors. Use **Cube Frontier > Build Web**, or:

```powershell
.\Tools\Build-Web.ps1 -UnityPath 'E:\actual-editor-path\Editor\Unity.exe'
```

The script writes `TestResults/Unity-Web-Build.log`, checks process exit status, and requires generated `CubeFrontier/Builds/Web/index.html`. Build output is intentionally ignored by Git.

## Local serving

For ordinary play, double-click `Start-Game.bat` in the workspace root. It locates the Editor if a build is needed, then serves the generated game and opens the browser. A custom Editor path can be supplied as the first argument, or via `UNITY_EDITOR_PATH`. Use `-Rebuild` after source changes. The launcher does not install Unity or activate a license.

```powershell
node .\Tools\Serve-Web.mjs
```

Uses loopback port 8080 by default; override with `$env:PORT = '8081'`. It only serves generated build files and sets WASM/JavaScript MIME types. It is a development preview server, not a production application server.

## Static hosting

Upload the entire `CubeFrontier/Builds/Web` directory to a static host after passing browser tests. Unity compilation requires an Editor/license in a compatible build environment; a generic Node static-host build command cannot compile this project. Do not deploy source `Assets/` as if it were the game.

Current build configuration deliberately uses **uncompressed** Unity output. This simplifies initial delivery and avoids mismatched encoding headers, at the cost of a larger download. Configure:

| File | Content-Type | Content-Encoding |
|---|---|---|
| `*.wasm` | `application/wasm` | absent |
| `*.js` | `application/javascript` | absent |
| `*.data` | `application/octet-stream` | absent |
| `*.html` | `text/html; charset=utf-8` | absent |

If enabling Brotli or gzip later, compressed resources also need `Content-Encoding: br` or `gzip` while retaining the underlying file's content type. Test these headers on the actual HTTPS URL. Do not attach encoding headers to uncompressed files.

Use `X-Content-Type-Options: nosniff`. Avoid long caching for `index.html`/loader until versioned asset deployment is configured. The generated loader may require a CSP allowing WebAssembly compilation; verify policy in target browsers instead of copying an untested blanket policy.

## Environment variables

The local single-player game has no credentials or service secrets. Optional local launcher variables are `PORT` (default 8080), `UNITY_EDITOR_PATH` (full path to Unity.exe) and `FRONTIER_NO_PAUSE=1` (suppress the batch error pause for automation). Do not invent API URLs, Unity project IDs or administrator passwords in this client.

When implementing the backend, introduce and document server-only database connection credentials, invitation-signing keys, session secrets and one-time administrator setup credentials through the host secret manager. Public client configuration may include the API base URL and UGS project/environment identifiers. It must never include a database service key or administrator password.

## Backend and migrations

**Not implemented.** There is no backend deployment command or migration to run. Phase C/D must add an actual service and tested schema before deploying an administrative panel. Planned tables cover users/roles, administrator setup/rotation state, rooms, invitations, room memberships, UGS session metadata, worlds/seeds, player state/inventory, modified chunk revisions, versioned snapshots and audit events. Define constraints and transaction behavior alongside API implementation rather than presenting an unused schema as completed persistence.

## Unity Multiplayer Services

**Not configured.** Before integration, create/link the intended UGS project and environment, install a compatible package set, enable its required services, and verify WebSockets using Unity's [distributed-authority Web guide](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/distributed-authority-webgl.html). Resolve authentication and trusted authorization before exposing room invitations. No live service or billing configuration was changed during this task.

## Troubleshooting

- No Unity.exe: locate the actual Editor executable; Unity Hub alone is not the compiler. The workspace audit did not find either on E:.
- Pink blocks: inspect shader errors and URP assets, then run Prepare Project. Do not treat asset generation as verification of shader compilation.
- No mouse look: ensure legacy Input Manager is selected by project setup, click the game, and check pointer-lock permission.
- Blank Web page: inspect browser Console/Network, verify all generated files were uploaded, and check MIME/encoding/CSP headers.
- Lost saves: keep origin and browser profile stable; check blocked/quota-limited storage and explicit save status. The game does not have cloud backups.
- Slow first loading/generation: profile the actual build and lower render distance. Texture/mesh/creature optimizations remain to be measured.

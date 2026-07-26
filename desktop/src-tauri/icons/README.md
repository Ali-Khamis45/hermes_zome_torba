# Icons

Placeholder — this directory needs the actual icon set before `tauri build` will succeed.

Generate the full set (`32x32.png`, `128x128.png`, `128x128@2x.png`, `icon.icns`, `icon.ico`,
`tray-icon.png`) from a single source image once the HZT brand mark exists:

```bash
npm run tauri icon path/to/source-icon.png --prefix ../..
```

See [tauri.conf.json](../tauri.conf.json) for how these are referenced.

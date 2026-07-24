# Chroma Website

Responsive, dependency-free landing page for Chroma, published at
[acchtt.github.io/Chroma](https://acchtt.github.io/Chroma/).

## Run locally

From this directory:

```bash
python -m http.server 8080
```

Then open `http://localhost:8080`.

## Files

- `index.html` — page structure and content
- `styles.css` — responsive styling
- `script.js` — mobile navigation and small header behaviors
- `assets/chroma-logo.png` — official Chroma logo
- `assets/favicon-32.png` — browser tab icon
- `assets/apple-touch-icon.png` — iOS home-screen icon
- `assets/chroma-app.png` — application screenshot

Pushes that change this folder are deployed by
`.github/workflows/deploy-pages.yml`.

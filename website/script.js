const header = document.querySelector('.site-header');
const menu = document.querySelector('.menu-button');
const links = document.querySelector('.nav-links');
const releaseVersion = 'v1.10.3';

const updateHeader = () => header?.classList.toggle('scrolled', window.scrollY > 12);

const closeMenu = () => {
  menu?.classList.remove('open');
  links?.classList.remove('open');
  menu?.setAttribute('aria-expanded', 'false');
};

const addAntiCheatSection = () => {
  const downloadSection = document.querySelector('.download-section');
  if (!downloadSection || document.querySelector('#anti-cheat')) return;

  const section = document.createElement('section');
  section.className = 'section';
  section.id = 'anti-cheat';
  section.setAttribute('aria-labelledby', 'anti-cheat-title');
  section.innerHTML = `
    <div class="shell">
      <div class="section-heading">
        <p class="eyebrow">Transparent by design</p>
        <h2 id="anti-cheat-title">Anti-cheat safety</h2>
        <p>
          Chroma operates outside game processes and changes display saturation through
          GPU-vendor control APIs. Its current architecture avoids the techniques commonly
          associated with cheats.
        </p>
      </div>

      <div class="feature-grid">
        <article class="feature-card">
          <div class="feature-icon cyan" aria-hidden="true">
            <svg viewBox="0 0 24 24"><path d="M12 3 5 6v5c0 4.6 2.9 8.2 7 10 4.1-1.8 7-5.4 7-10V6l-7-3Zm-3 9 2 2 4-5" /></svg>
          </div>
          <h3>External operation</h3>
          <p>No DLL injection, internal overlay, game-code hooks, game-file changes, or kernel driver.</p>
        </article>

        <article class="feature-card">
          <div class="feature-icon violet" aria-hidden="true">
            <svg viewBox="0 0 24 24"><path d="M4 7h16v10H4zM8 11h8M8 14h5" /></svg>
          </div>
          <h3>No game-memory access</h3>
          <p>Chroma only queries the foreground executable name with limited Windows process permissions.</p>
        </article>

        <article class="feature-card">
          <div class="feature-icon magenta" aria-hidden="true">
            <svg viewBox="0 0 24 24"><path d="M4 17V7m5 10V4m5 13V9m5 8V6" /></svg>
          </div>
          <h3>Official GPU interfaces</h3>
          <p>Saturation is applied through Intel IGCL, NVIDIA NVAPI, or AMD ADLX display controls.</p>
        </article>
      </div>

      <div class="download-card" style="margin-top: 24px; display: block;">
        <div>
          <p class="eyebrow">Assessment</p>
          <h3>Low risk, not a certification</h3>
          <p>
            Based on the current source code, Chroma is considered low risk for VAC,
            BattlEye, Easy Anti-Cheat, and Riot Vanguard. Chroma is not officially certified,
            endorsed, or allowlisted by those providers. Game-publisher policies and detection
            rules can change, so no third-party utility can guarantee approval for every game.
          </p>
        </div>
      </div>
    </div>`;

  downloadSection.before(section);

  const downloadLink = links?.querySelector('.nav-download');
  if (links && downloadLink && !links.querySelector('a[href="#anti-cheat"]')) {
    const safetyLink = document.createElement('a');
    safetyLink.href = '#anti-cheat';
    safetyLink.textContent = 'Anti-cheat safety';
    links.insertBefore(safetyLink, downloadLink);
  }
};

addAntiCheatSection();

menu?.addEventListener('click', () => {
  const open = menu.getAttribute('aria-expanded') !== 'true';
  menu.classList.toggle('open', open);
  links?.classList.toggle('open', open);
  menu.setAttribute('aria-expanded', String(open));
});

links?.querySelectorAll('a').forEach(link => link.addEventListener('click', closeMenu));
window.addEventListener('scroll', updateHeader, { passive: true });
window.addEventListener('resize', () => {
  if (window.innerWidth > 700) closeMenu();
});

const heroDownload = document.querySelector('.hero-actions .button-primary');
const heroDownloadText = heroDownload
  ? [...heroDownload.childNodes].find(node => node.nodeType === Node.TEXT_NODE && node.textContent.trim())
  : null;
if (heroDownloadText) {
  heroDownloadText.textContent = ` Download ${releaseVersion}`;
}

const softwareSchema = document.querySelector('script[type="application/ld+json"]');
if (softwareSchema?.textContent) {
  try {
    const schema = JSON.parse(softwareSchema.textContent);
    schema.softwareVersion = releaseVersion.replace(/^v/i, '');
    softwareSchema.textContent = JSON.stringify(schema);
  } catch {
    // Keep the static metadata if the embedded schema cannot be parsed.
  }
}

updateHeader();
document.querySelector('#year').textContent = new Date().getFullYear();

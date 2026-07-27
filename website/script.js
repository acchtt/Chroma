const header = document.querySelector('.site-header');
const menu = document.querySelector('.menu-button');
const links = document.querySelector('.nav-links');
const releaseVersion = 'v1.10.4';

const updateHeader = () => header?.classList.toggle('scrolled', window.scrollY > 12);

const closeMenu = () => {
  menu?.classList.remove('open');
  links?.classList.remove('open');
  menu?.setAttribute('aria-expanded', 'false');
};

const addAntiCheatStyles = () => {
  if (document.querySelector('#anti-cheat-design')) return;

  const style = document.createElement('style');
  style.id = 'anti-cheat-design';
  style.textContent = `
    #anti-cheat {
      border-block: 1px solid rgba(128, 150, 198, 0.12);
      background:
        radial-gradient(circle at 12% 20%, rgba(88, 236, 245, 0.07), transparent 24rem),
        radial-gradient(circle at 88% 70%, rgba(165, 123, 255, 0.08), transparent 26rem),
        rgba(3, 7, 18, 0.36);
    }

    .safety-header-card {
      display: grid;
      grid-template-columns: auto minmax(0, 1fr) 190px;
      align-items: center;
      gap: 16px;
      padding: 18px;
      border: 1px solid var(--line-strong);
      border-radius: 18px;
      background: linear-gradient(135deg, rgba(13, 29, 53, 0.94), rgba(5, 11, 25, 0.97));
      box-shadow:
        inset 0 1px 0 rgba(255, 255, 255, 0.04),
        0 22px 58px rgba(0, 0, 0, 0.24);
    }

    .safety-shield-tile {
      display: grid;
      width: 52px;
      height: 52px;
      place-items: center;
      border: 1px solid rgba(88, 236, 245, 0.28);
      border-radius: 14px;
      color: var(--cyan);
      background: linear-gradient(145deg, rgba(20, 42, 75, 0.98), rgba(8, 14, 34, 0.98));
      box-shadow: 0 0 28px rgba(88, 236, 245, 0.12);
    }

    .safety-shield-tile svg {
      width: 27px;
      height: 27px;
    }

    .safety-header-copy h2 {
      margin: 0 0 6px;
      font-size: clamp(28px, 3vw, 40px);
    }

    .safety-header-copy p {
      margin: 0;
      color: var(--muted-2);
      font-size: 14px;
    }

    .safety-state {
      padding: 11px 13px;
      border: 1px solid rgba(140, 245, 165, 0.2);
      border-radius: 12px;
      background: rgba(7, 16, 32, 0.72);
    }

    .safety-state-main {
      display: flex;
      align-items: center;
      gap: 8px;
      color: var(--lime);
      font-size: 12px;
      font-weight: 800;
      letter-spacing: 0.1em;
    }

    .safety-state-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: var(--lime);
      box-shadow: 0 0 14px rgba(140, 245, 165, 0.75);
    }

    .safety-state small {
      display: block;
      margin-top: 3px;
      color: var(--muted-2);
      font-size: 12px;
    }

    .safety-summary {
      max-width: 900px;
      margin: 24px 0 0;
      color: var(--muted);
      font-size: 17px;
      line-height: 1.72;
    }

    .safety-feature-grid {
      margin-top: 24px;
    }

    .safety-feature-grid .feature-card {
      min-height: 238px;
    }

    .safety-assessment {
      display: grid;
      grid-template-columns: auto minmax(0, 1fr);
      gap: 18px;
      margin-top: 24px;
      padding: 25px;
      border: 1px solid var(--line-strong);
      border-radius: 18px;
      background: linear-gradient(135deg, rgba(14, 31, 59, 0.9), rgba(5, 11, 25, 0.96));
      box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.035);
    }

    .safety-assessment-icon {
      display: grid;
      width: 44px;
      height: 44px;
      place-items: center;
      border: 1px solid rgba(140, 245, 165, 0.24);
      border-radius: 12px;
      color: var(--lime);
      background: rgba(140, 245, 165, 0.07);
    }

    .safety-assessment-icon svg {
      width: 22px;
      height: 22px;
    }

    .safety-assessment .eyebrow {
      margin-bottom: 7px;
      color: var(--lime);
    }

    .safety-assessment h3 {
      margin-bottom: 8px;
    }

    .safety-assessment p:last-child {
      margin: 0;
      color: var(--muted);
    }

    @media (max-width: 780px) {
      .safety-header-card {
        grid-template-columns: auto minmax(0, 1fr);
      }

      .safety-state {
        grid-column: 1 / -1;
      }
    }

    @media (max-width: 520px) {
      .safety-header-card {
        padding: 15px;
      }

      .safety-shield-tile {
        width: 46px;
        height: 46px;
      }

      .safety-header-copy h2 {
        font-size: 25px;
      }

      .safety-assessment {
        grid-template-columns: 1fr;
        padding: 21px;
      }
    }
  `;

  document.head.append(style);
};

const addAntiCheatSection = () => {
  const downloadSection = document.querySelector('.download-section');
  if (!downloadSection || document.querySelector('#anti-cheat')) return;

  addAntiCheatStyles();

  const section = document.createElement('section');
  section.className = 'section';
  section.id = 'anti-cheat';
  section.setAttribute('aria-labelledby', 'anti-cheat-title');
  section.innerHTML = `
    <div class="shell">
      <div class="safety-header-card">
        <div class="safety-shield-tile" aria-hidden="true">
          <svg viewBox="0 0 24 24">
            <path d="M12 3 5 6v5c0 4.6 2.9 8.2 7 10 4.1-1.8 7-5.4 7-10V6l-7-3Z" />
            <path d="m9 12 2 2 4-5" />
          </svg>
        </div>

        <div class="safety-header-copy">
          <h2 id="anti-cheat-title">Anti-cheat safety</h2>
          <p>External GPU control • no game-process access</p>
        </div>

        <div class="safety-state" aria-label="Safe architecture review status">
          <div class="safety-state-main">
            <span class="safety-state-dot" aria-hidden="true"></span>
            <strong>SAFE</strong>
          </div>
          <small>Architecture review</small>
        </div>
      </div>

      <p class="safety-summary">
        Chroma operates outside game processes and changes display saturation through
        GPU-vendor control APIs. Its current architecture avoids the techniques commonly
        associated with cheats.
      </p>

      <div class="feature-grid safety-feature-grid">
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

      <div class="safety-assessment">
        <div class="safety-assessment-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24"><path d="m5 12 4 4L19 6" /></svg>
        </div>
        <div>
          <p class="eyebrow">Assessment</p>
          <h3>Safe architecture, not a certification</h3>
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

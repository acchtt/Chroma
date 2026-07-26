const header = document.querySelector('.site-header');
const menu = document.querySelector('.menu-button');
const links = document.querySelector('.nav-links');
const releaseVersion = 'v1.5';

const updateHeader = () => header?.classList.toggle('scrolled', window.scrollY > 12);

const closeMenu = () => {
  menu?.classList.remove('open');
  links?.classList.remove('open');
  menu?.setAttribute('aria-expanded', 'false');
};

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

let previousFocus;
let activeDialog;
let previousOverflow;
const focusable = 'button:not(:disabled), input:not(:disabled), a[href], select:not(:disabled), [tabindex="0"]';

export function revealSection(section) {
    const navigation = document.querySelector('.settings-studio .st-navigation');
    const selected = navigation?.querySelector(`[data-section="${section}"]`);
    if (!selected || navigation.scrollWidth <= navigation.clientWidth) return;
    const navBounds = navigation.getBoundingClientRect();
    const selectedBounds = selected.getBoundingClientRect();
    navigation.scrollTo({
        left: navigation.scrollLeft + selectedBounds.left - navBounds.left - (navBounds.width - selectedBounds.width) / 2,
        behavior: 'instant'
    });
}

window.addEventListener('resize', () => {
    const selected = document.querySelector('.settings-studio .st-nav-item[aria-current="page"]');
    if (selected) revealSection(selected.dataset.section);
});

function trapFocus(event) {
    if (event.key !== 'Tab' || !activeDialog) return;
    const controls = [...activeDialog.querySelectorAll(focusable)].filter(el => el.getClientRects().length);
    const first = controls[0], last = controls.at(-1);
    if (!first) { event.preventDefault(); activeDialog.focus(); return; }
    if (event.shiftKey && (document.activeElement === first || document.activeElement === activeDialog)) {
        event.preventDefault(); last.focus();
    } else if (!event.shiftKey && (document.activeElement === last || document.activeElement === activeDialog)) {
        event.preventDefault(); first.focus();
    }
}

export function openDialog(element) {
    closeDialog();
    previousFocus = document.activeElement;
    previousOverflow = document.body.style.overflow;
    activeDialog = element;
    document.body.style.overflow = 'hidden';
    activeDialog.addEventListener('keydown', trapFocus);
    activeDialog.focus({ preventScroll: true });
}

export function closeDialog() {
    if (!activeDialog) return;
    activeDialog.removeEventListener('keydown', trapFocus);
    document.body.style.overflow = previousOverflow;
    activeDialog = undefined;
    if (previousFocus?.isConnected) previousFocus.focus({ preventScroll: true });
    previousFocus = undefined;
}

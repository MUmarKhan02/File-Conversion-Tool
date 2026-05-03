// ── Category selection ──────────────────────────────────────────────────────
const catBtns = document.querySelectorAll('.cat-btn');
const panels = document.querySelectorAll('.fmt-panel');
const radios = document.querySelectorAll('input[type="radio"][name="conversionKey"]');
const inputFmt = document.getElementById('inputFormat');
const outputFmt = document.getElementById('outputFormat');
const dropPill = document.getElementById('dropPill');
const dropTitle = document.getElementById('dropTitle');
const fileInput = document.getElementById('fileInput');
const convertBtn = document.getElementById('convertBtn');
const dropzone = document.getElementById('dropzone');
const browseLink = document.getElementById('browseLink');
const fmtBtns = document.querySelectorAll('.fmt-btn');

let selectedInputFmt = '';
let selectedOutputFmt = '';
let fileReady = false;
let activePanel = null;

catBtns.forEach(btn => {
    btn.addEventListener('click', () => {
        const cat = btn.dataset.cat;
        const panel = document.getElementById('panel-' + cat);
        const isAlreadyOpen = btn.classList.contains('active');

        catBtns.forEach(b => b.classList.remove('active'));
        panels.forEach(p => p.style.display = 'none');

        fmtBtns.forEach(b => b.classList.remove('active'));
        radios.forEach(r => r.checked = false);
        selectedInputFmt = '';
        selectedOutputFmt = '';
        inputFmt.value = '';
        outputFmt.value = '';
        resetFile();
        updateButton();

        if (!isAlreadyOpen) {
            btn.classList.add('active');
            panel.style.display = 'block';
            activePanel = cat;
        } else {
            activePanel = null;
        }
    });
});

// ── Converter selection ─────────────────────────────────────────────────────
radios.forEach(radio => {
    radio.addEventListener('change', () => {
        selectedInputFmt = radio.dataset.input;
        selectedOutputFmt = radio.dataset.output;
        inputFmt.value = selectedInputFmt;
        outputFmt.value = selectedOutputFmt;

        fmtBtns.forEach(b => b.classList.remove('active'));
        radio.closest('.fmt-btn').classList.add('active');

        fileInput.setAttribute('accept', '.' + selectedInputFmt);
        dropPill.textContent = '.' + selectedInputFmt + ' accepted';
        dropPill.classList.add('active');

        resetFile();
        updateButton();
    });
});

// ── Drag-and-drop ───────────────────────────────────────────────────────────
dropzone.addEventListener('click', () => { if (selectedInputFmt) fileInput.click(); });
browseLink.addEventListener('click', e => { e.stopPropagation(); if (selectedInputFmt) fileInput.click(); });

['dragenter', 'dragover'].forEach(evt =>
    dropzone.addEventListener(evt, e => { e.preventDefault(); if (selectedInputFmt) dropzone.classList.add('drag-over'); })
);
['dragleave', 'drop'].forEach(evt =>
    dropzone.addEventListener(evt, e => { e.preventDefault(); dropzone.classList.remove('drag-over'); })
);

dropzone.addEventListener('drop', e => {
    if (!selectedInputFmt) return;
    const file = e.dataTransfer?.files?.[0];
    if (file) applyFile(file, e.dataTransfer);
});

fileInput.addEventListener('change', () => {
    if (fileInput.files[0]) applyFile(fileInput.files[0]);
});

function applyFile(file, dataTransfer) {
    const ext = file.name.split('.').pop().toLowerCase();
    if (ext !== selectedInputFmt) {
        alert(`Wrong file type. Expected .${selectedInputFmt}, got .${ext}`);
        return;
    }
    if (dataTransfer) {
        const dt = new DataTransfer();
        dt.items.add(file);
        fileInput.files = dt.files;
    }
    fileReady = true;
    dropzone.classList.add('has-file');
    dropTitle.textContent = file.name;
    dropPill.textContent = formatBytes(file.size);
    updateButton();
}

function resetFile() {
    fileReady = false;
    fileInput.value = '';
    dropzone.classList.remove('has-file');
    dropTitle.textContent = 'Drag & drop your file here';
    if (!selectedInputFmt) {
        dropPill.textContent = 'Select a conversion first';
        dropPill.classList.remove('active');
    }
}

function updateButton() {
    convertBtn.disabled = !(selectedInputFmt && fileReady);
}

function formatBytes(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
}

// ── Progress bar on submit ─────────────────────────────────────────────────
const progressWrap = document.getElementById('progressWrap');
const progressBar = document.getElementById('progressBar');
const progressLabel = document.getElementById('progressLabel');
const progressPct = document.getElementById('progressPct');

function setProgress(pct, label) {
    progressBar.style.width = pct + '%';
    progressPct.textContent = Math.round(pct) + '%';
    if (label) progressLabel.textContent = label;
}

document.getElementById('convertForm').addEventListener('submit', () => {
    convertBtn.disabled = true;
    convertBtn.style.display = 'none';
    progressWrap.style.display = 'block';

    let pct = 0;
    setProgress(0, 'Uploading file...');

    // Phase 1: 0 → 70% quickly (upload)
    const phase1 = setInterval(() => {
        pct += (70 - pct) * 0.12;
        setProgress(pct, 'Uploading file...');
        if (pct >= 69) {
            clearInterval(phase1);
            setProgress(70, 'Converting...');

            // Phase 2: 70 → 92% slowly (server processing)
            const phase2 = setInterval(() => {
                pct += (92 - pct) * 0.04;
                setProgress(pct, 'Converting...');
                if (pct >= 91.5) clearInterval(phase2);
            }, 120);
        }
    }, 60);
});

// ── Auto-scroll to result ───────────────────────────────────────────────────
const resultCard = document.querySelector('.result-card');
if (resultCard) resultCard.scrollIntoView({ behavior: 'smooth', block: 'center' });

// ── Fade out result card on download ───────────────────────────────────────
// The PRG pattern means refresh always shows a clean page.
// This just makes the UX feel instant — card fades right after download starts.
function fadeOut(el, duration = 400) {
    el.style.transition = `opacity ${duration}ms ease`;
    el.style.opacity = '0';
    setTimeout(() => el.remove(), duration);
}

const dlBtn = document.querySelector('.dl-btn');
if (dlBtn) {
    dlBtn.addEventListener('click', () => {
        const card = dlBtn.closest('.result-card');
        if (card) setTimeout(() => fadeOut(card), 800);
    });
}
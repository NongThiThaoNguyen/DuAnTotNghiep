/* ================================================================
   2026 Lesson Detail Page — Interactive Logic
   Preserves: Web Speech API pronunciation, YouTube transcript sync
   ================================================================ */

document.addEventListener('DOMContentLoaded', function () {

    /* ── 1. PILL TAB SWITCHING ──────────────────────────────── */
    const tabBtns  = document.querySelectorAll('.c26-tab-btn.lesson-details-tab');
    const tabPanels = document.querySelectorAll('.c26-tab-panel.tab-content-item');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const target = this.getAttribute('data-tab');

            // Update buttons
            tabBtns.forEach(b => {
                b.classList.remove('active');
                b.setAttribute('aria-selected', 'false');
            });
            this.classList.add('active');
            this.setAttribute('aria-selected', 'true');

            // Update panels
            tabPanels.forEach(panel => {
                if (panel.id === target + 'Content') {
                    panel.classList.add('active');
                    panel.setAttribute('aria-hidden', 'false');
                } else {
                    panel.classList.remove('active');
                    panel.setAttribute('aria-hidden', 'true');
                }
            });
        });
    });

    /* ── 2. TRANSCRIPT TOGGLE ───────────────────────────────── */
    const transcriptToggle = document.getElementById('transcriptToggleBtn');
    const transcriptBox    = document.getElementById('transcriptBox');
    const transcriptChevron = document.getElementById('transcriptChevron');

    if (transcriptToggle && transcriptBox) {
        transcriptToggle.addEventListener('click', function () {
            const isHidden = transcriptBox.classList.toggle('hidden');
            this.setAttribute('aria-expanded', isHidden ? 'false' : 'true');
            if (transcriptChevron) {
                transcriptChevron.style.transform = isHidden ? 'rotate(-90deg)' : 'rotate(0deg)';
            }
        });
    }

    /* ── 3. WEB SPEECH API — PRONUNCIATION WITH LIVE WORD HIGHLIGHTING ── */
    const pronounceBtns = document.querySelectorAll('.c26-pronounce-btn.btn-pronounce');

    if (pronounceBtns.length > 0 && 'speechSynthesis' in window) {
        let voices = [];
        let currentUtterance = null;
        let activeBtn = null;
        let activeTargetElem = null;
        let originalHtml = '';

        function loadVoices() {
            voices = window.speechSynthesis.getVoices();
        }
        loadVoices();
        window.speechSynthesis.onvoiceschanged = loadVoices;

        function resetSpeechState() {
            if (currentUtterance) {
                window.speechSynthesis.cancel();
                currentUtterance = null;
            }
            if (activeTargetElem && originalHtml !== '') {
                activeTargetElem.innerHTML = originalHtml;
                activeTargetElem.classList.remove('speech-reading');
            }
            if (activeBtn) {
                activeBtn.classList.remove('speaking');
                activeBtn.innerHTML = `
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M15.536 8.464a5 5 0 0 1 0 7.072M12 6v12m-4.243-1.757A9 9 0 0 1 12 3m0 18a9 9 0 0 1-4.243-2.757"/>
                    </svg>
                    Nghe
                `;
            }
            activeBtn = null;
            activeTargetElem = null;
            originalHtml = '';
        }

        pronounceBtns.forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                // If already speaking on this button, stop it
                if (activeBtn === this) {
                    resetSpeechState();
                    return;
                }

                // Reset any existing playback
                resetSpeechState();

                const wordToSay = this.getAttribute('data-word');
                if (!wordToSay) return;

                // Locate the target text element (e.g. .c26-example-english in the card)
                const card = this.closest('.c26-example-card') || this.closest('.c26-example-body') || this.parentElement.parentElement;
                let targetElem = card ? card.querySelector('.c26-example-english') : null;

                activeBtn = this;
                activeTargetElem = targetElem;

                if (targetElem) {
                    originalHtml = targetElem.innerHTML;
                    const fullText = wordToSay;

                    // Build tokenized HTML with data-start and data-end character offsets
                    let htmlWithSpans = '';
                    let charOffset = 0;
                    const tokens = fullText.split(/(\s+|[^\w\s]+)/);

                    tokens.forEach(token => {
                        const start = charOffset;
                        const end = charOffset + token.length;
                        charOffset = end;

                        if (token.trim().length > 0 && /[\w]/.test(token)) {
                            htmlWithSpans += `<span class="speech-word" data-start="${start}" data-end="${end}">${token}</span>`;
                        } else {
                            htmlWithSpans += token;
                        }
                    });

                    targetElem.innerHTML = htmlWithSpans;
                    targetElem.classList.add('speech-reading');
                }

                // Visual button update
                this.classList.add('speaking');
                this.innerHTML = `
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 10a1 1 0 011-1h4a1 1 0 011 1v4a1 1 0 01-1 1h-4a1 1 0 01-1-1v-4z" />
                    </svg>
                    Dừng
                `;

                const utterance = new SpeechSynthesisUtterance(wordToSay);
                utterance.lang = 'en-US';
                utterance.rate = 0.85; // Slightly relaxed reading rate for clarity
                utterance.pitch = 1.0;

                if (voices.length === 0) { loadVoices(); }
                const englishVoice = voices.find(v => v.lang === 'en-US' || v.lang === 'en-GB');
                if (englishVoice) utterance.voice = englishVoice;

                // Highlight word in real time
                utterance.onboundary = function (event) {
                    if (event.name === 'word' && targetElem) {
                        const charIndex = event.charIndex;
                        const wordSpans = targetElem.querySelectorAll('.speech-word');
                        wordSpans.forEach(span => {
                            const start = parseInt(span.getAttribute('data-start'));
                            const end = parseInt(span.getAttribute('data-end'));
                            if (charIndex >= start && charIndex < end) {
                                span.classList.add('active-speaking-word');
                            } else {
                                span.classList.remove('active-speaking-word');
                            }
                        });
                    }
                };

                utterance.onend = function () {
                    resetSpeechState();
                };

                utterance.onerror = function () {
                    resetSpeechState();
                };

                currentUtterance = utterance;
                window.speechSynthesis.speak(utterance);
            });
        });
    }

});

/* ================================================================
   YouTube Interactive Transcript Logic
   (runs immediately, not inside DOMContentLoaded, per YT API spec)
   ================================================================ */
let ytPlayer;
let transcriptCheckInterval;
const transcriptLines = document.querySelectorAll('.transcript-line');

if (document.getElementById('youtubePlayer') && transcriptLines.length > 0) {
    var tag = document.createElement('script');
    tag.src = 'https://www.youtube.com/iframe_api';
    var firstScriptTag = document.getElementsByTagName('script')[0];
    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
}

// Called automatically by YouTube IFrame API when ready
function onYouTubeIframeAPIReady() {
    ytPlayer = new YT.Player('youtubePlayer', {
        events: { onStateChange: onPlayerStateChange }
    });
}

function onPlayerStateChange(event) {
    // YT.PlayerState.PLAYING == 1
    if (event.data === 1) {
        transcriptCheckInterval = setInterval(syncTranscript, 500);
    } else {
        clearInterval(transcriptCheckInterval);
    }
}

function syncTranscript() {
    if (!ytPlayer || !ytPlayer.getCurrentTime) return;
    const currentTime = ytPlayer.getCurrentTime();

    let activeLineIndex = -1;
    for (let i = 0; i < transcriptLines.length; i++) {
        const lineTime = parseFloat(transcriptLines[i].getAttribute('data-time'));
        if (currentTime >= lineTime) {
            activeLineIndex = i;
        } else {
            break;
        }
    }

    if (activeLineIndex !== -1) {
        transcriptLines.forEach(line => line.classList.remove('active'));
        const activeLine = transcriptLines[activeLineIndex];
        activeLine.classList.add('active');

        // Auto-scroll to active line
        const container = activeLine.parentElement;
        const scrollPos = activeLine.offsetTop - container.offsetTop
                        - (container.clientHeight / 2)
                        + (activeLine.clientHeight / 2);
        container.scrollTo({ top: scrollPos, behavior: 'smooth' });
    }
}

// Click transcript line to seek
transcriptLines.forEach(line => {
    line.addEventListener('click', function () {
        if (!ytPlayer || !ytPlayer.seekTo) return;
        const time = parseFloat(this.getAttribute('data-time'));
        ytPlayer.seekTo(time, true);
        ytPlayer.playVideo();
    });
});

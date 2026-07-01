/* Lesson Interactive Navigation Logic */
document.addEventListener("DOMContentLoaded", function () {
    const tabs = document.querySelectorAll(".lesson-details-tab");
    const contents = document.querySelectorAll(".tab-content-item");

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            const target = this.getAttribute("data-tab");
            
            tabs.forEach(t => t.classList.remove("active"));
            this.classList.add("active");

            contents.forEach(content => {
                if (content.id === target + "Content") {
                    content.classList.add("active");
                } else {
                    content.classList.remove("active");
                }
            });
        });
    });

    // Web Speech API for Pronunciation
    const pronounceBtns = document.querySelectorAll(".btn-pronounce");
    if (pronounceBtns.length > 0 && 'speechSynthesis' in window) {
        // Load voices once to ensure they are available
        let voices = [];
        window.speechSynthesis.onvoiceschanged = () => {
            voices = window.speechSynthesis.getVoices();
        };

        pronounceBtns.forEach(btn => {
            btn.addEventListener("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
                
                const wordToSay = this.getAttribute("data-word");
                if (!wordToSay) return;

                // Cancel any ongoing speech
                window.speechSynthesis.cancel();

                const utterance = new SpeechSynthesisUtterance(wordToSay);
                utterance.lang = "en-US";
                utterance.rate = 0.9; // Slightly slower for clarity
                utterance.pitch = 1.0;
                
                // Try to find a high-quality English voice if available
                if (voices.length === 0) {
                    voices = window.speechSynthesis.getVoices();
                }
                const englishVoice = voices.find(v => v.lang === 'en-US' || v.lang === 'en-GB');
                if (englishVoice) {
                    utterance.voice = englishVoice;
                }

                window.speechSynthesis.speak(utterance);
                
                // Visual feedback (optional)
                this.style.transform = "scale(0.9)";
                setTimeout(() => {
                    this.style.transform = "scale(1)";
                }, 200);
            });
        });
    }
});

/* YouTube Interactive Transcript Logic */
let ytPlayer;
let transcriptCheckInterval;
const transcriptLines = document.querySelectorAll('.transcript-line');

if (document.getElementById('youtubePlayer') && transcriptLines.length > 0) {
    // Load YouTube IFrame API
    var tag = document.createElement('script');
    tag.src = "https://www.youtube.com/iframe_api";
    var firstScriptTag = document.getElementsByTagName('script')[0];
    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
}

// Called automatically by YouTube API when loaded
function onYouTubeIframeAPIReady() {
    ytPlayer = new YT.Player('youtubePlayer', {
        events: {
            'onStateChange': onPlayerStateChange
        }
    });
}

function onPlayerStateChange(event) {
    // YT.PlayerState.PLAYING == 1
    if (event.data == 1) {
        // Start checking time to sync transcript
        transcriptCheckInterval = setInterval(syncTranscript, 500);
    } else {
        clearInterval(transcriptCheckInterval);
    }
}

function syncTranscript() {
    if (!ytPlayer || !ytPlayer.getCurrentTime) return;
    const currentTime = ytPlayer.getCurrentTime();
    
    let activeLineIndex = -1;
    // Find the current transcript line based on time
    for (let i = 0; i < transcriptLines.length; i++) {
        const lineTime = parseFloat(transcriptLines[i].getAttribute('data-time'));
        if (currentTime >= lineTime) {
            activeLineIndex = i;
        } else {
            break;
        }
    }
    
    if (activeLineIndex !== -1) {
        // Highlight active line
        transcriptLines.forEach(line => line.classList.remove('active'));
        const activeLine = transcriptLines[activeLineIndex];
        activeLine.classList.add('active');
        
        // Auto scroll to active line
        const container = activeLine.parentElement;
        const scrollPos = activeLine.offsetTop - container.offsetTop - (container.clientHeight / 2) + (activeLine.clientHeight / 2);
        container.scrollTo({ top: scrollPos, behavior: 'smooth' });
    }
}

// Click on transcript to seek video
transcriptLines.forEach(line => {
    line.addEventListener('click', function() {
        if (!ytPlayer || !ytPlayer.seekTo) return;
        const time = parseFloat(this.getAttribute('data-time'));
        ytPlayer.seekTo(time, true);
        ytPlayer.playVideo();
    });
});


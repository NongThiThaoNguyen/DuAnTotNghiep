/* AI Chat interaction */

// Reset chat without reloading the page (called from onclick in HTML)
function resetChat() {
    var btn = document.getElementById("btnResetChat");
    var icon = document.getElementById("resetIcon");
    if (btn && btn.disabled) return; // prevent double-click

    // Show loading spin state
    if (btn) btn.disabled = true;
    if (icon) icon.style.animation = "spin 0.6s linear infinite";

    setTimeout(function () {
        // Clear all messages in UI
        var messagesLog = document.getElementById("chatMessagesLog");
        if (messagesLog) messagesLog.innerHTML = "";

        // Show default greeting bubble
        appendWelcomeMessage();

        // Reset conversationId so server starts a new session on next send
        var convInput = document.getElementById("conversationId");
        if (convInput) convInput.value = "0";

        // Restore button state
        if (btn) btn.disabled = false;
        if (icon) icon.style.animation = "";

        // Focus and clear input
        var chatInput = document.getElementById("chatInput");
        if (chatInput) { chatInput.value = ""; chatInput.focus(); }
    }, 500);
}

function appendWelcomeMessage() {
    var messagesLog = document.getElementById("chatMessagesLog");
    if (!messagesLog) return;
    var time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    var item = document.createElement("div");
    item.className = "chat-bubble-wrapper ai";
    item.innerHTML =
        '<img src="/images/ai-tutor.svg" alt="AI" class="chat-bubble-avatar" onerror="this.onerror=null;this.src=\'/images/default-avatar.svg\';">' +
        '<div class="chat-bubble-content">' +
        '<p class="chat-bubble-text">\uD83D\uDC4B Xin ch\u00E0o! T\u00F4i l\u00E0 AI English Tutor. B\u1EA1n mu\u1ED1n h\u1ECDc g\u00EC h\u00F4m nay?</p>' +
        '<span class="chat-bubble-time">' + time + '</span>' +
        '</div>';
    messagesLog.appendChild(item);
}

document.addEventListener("DOMContentLoaded", function () {
    var chatForm = document.getElementById("chatForm");
    var chatInput = document.getElementById("chatInput");
    var messagesLog = document.getElementById("chatMessagesLog");

    function scrollToBottom() {
        if (messagesLog) {
            messagesLog.scrollTop = messagesLog.scrollHeight;
        }
    }

    scrollToBottom();

    if (chatForm) {
        chatForm.addEventListener("submit", function (e) {
            e.preventDefault();
            var text = chatInput.value.trim();
            if (!text) return;

            // 1. Append Student Message
            appendMessage("STUDENT", text);
            chatInput.value = "";

            // 2. Append Typing Indicator
            var typingIndicator = appendTypingIndicator();
            scrollToBottom();

            // 3. Post to AI endpoint
            var token = document.querySelector('input[name="__RequestVerificationToken"]') &&
                        document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch("/AITutor/SendMessage", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "RequestVerificationToken": token
                },
                body: new URLSearchParams({
                    messageText: text,
                    conversationId: document.getElementById("conversationId") ? document.getElementById("conversationId").value : "0"
                })
            })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                // Remove indicator
                typingIndicator.remove();

                if (data.success) {
                    appendMessage("AI", data.replyText);
                } else {
                    var errorDetail = data.message || "Xin l\u1ED7i b\u1EA1n, k\u1EBFt n\u1ED1i AI c\u1EE7a m\u00ECnh \u0111ang g\u1EB7p s\u1EF1 c\u1ED1. B\u1EA1n vui l\u00F2ng th\u1EED l\u1EA1i nh\u00E9!";
                    appendMessage("AI", "\u274C [L\u1ED7i k\u1EBFt n\u1ED1i AI]: " + errorDetail);
                }
                scrollToBottom();
            })
            .catch(function (err) {
                typingIndicator.remove();
                appendMessage("AI", "\u274C Kh\u00F4ng th\u1EC3 g\u1EEDi tin nh\u1EAFn. L\u1ED7i m\u1EA1ng: " + err.message);
                scrollToBottom();
            });
        });
    }

    function appendMessage(sender, text) {
        var time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        var isStudent = (sender === "STUDENT");

        var item = document.createElement("div");
        item.className = "chat-bubble-wrapper " + (isStudent ? 'student' : 'ai');

        var rawAvatarUrl = document.getElementById("studentAvatarUrl") ? document.getElementById("studentAvatarUrl").value : "";
        var studentAvatarUrl = (rawAvatarUrl.indexOf("/default-images/avatar.png") !== -1 || rawAvatarUrl.indexOf("/images/default-avatar.png") !== -1)
            ? "/images/default-avatar.svg"
            : (rawAvatarUrl || "/images/default-avatar.svg");
        var avatarImg = isStudent ? studentAvatarUrl : "/images/ai-tutor.svg";

        item.innerHTML =
            '<img src="' + avatarImg + '" alt="' + sender + '" class="chat-bubble-avatar">' +
            '<div class="chat-bubble-content">' +
            '<p class="chat-bubble-text">' + escapeHtml(text) + '</p>' +
            '<span class="chat-bubble-time">' + time + '</span>' +
            '</div>';
        messagesLog.appendChild(item);
    }

    function appendTypingIndicator() {
        var item = document.createElement("div");
        item.className = "chat-bubble-wrapper ai typing-indicator-item";
        item.innerHTML =
            '<img src="/images/ai-tutor.svg" alt="AI" class="chat-bubble-avatar">' +
            '<div class="chat-bubble-content">' +
            '<div class="typing-indicator">' +
            '<span class="typing-dot"></span>' +
            '<span class="typing-dot"></span>' +
            '<span class="typing-dot"></span>' +
            '</div></div>';
        messagesLog.appendChild(item);
        return item;
    }

    function escapeHtml(str) {
        return str
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
});

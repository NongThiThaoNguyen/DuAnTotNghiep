/* AI Chat interaction */
document.addEventListener("DOMContentLoaded", function () {
    const chatForm = document.getElementById("chatForm");
    const chatInput = document.getElementById("chatInput");
    const messagesLog = document.getElementById("chatMessagesLog");
    const suggestChips = document.querySelectorAll(".chat-suggestion-chip");

    function scrollToBottom() {
        if (messagesLog) {
            messagesLog.scrollTop = messagesLog.scrollHeight;
        }
    }

    scrollToBottom();

    // Suggested prompts click
    suggestChips.forEach(chip => {
        chip.addEventListener("click", function () {
            if (chatInput) {
                chatInput.value = this.textContent;
                chatInput.focus();
            }
        });
    });

    if (chatForm) {
        chatForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const text = chatInput.value.trim();
            if (!text) return;

            // 1. Append Student Message
            appendMessage("STUDENT", text);
            chatInput.value = "";

            // 2. Append Typing Indicator
            const typingIndicator = appendTypingIndicator();
            scrollToBottom();

            // 3. Post to AI endpoint
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            fetch("/AITutor/SendMessage", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "RequestVerificationToken": token
                },
                body: new URLSearchParams({
                    messageText: text,
                    conversationId: document.getElementById("conversationId")?.value || "0"
                })
            })
            .then(res => res.json())
            .then(data => {
                // Remove indicator
                typingIndicator.remove();

                if (data.success) {
                    appendMessage("AI", data.replyText);
                } else {
                    appendMessage("AI", "Xin lỗi bạn, kết nối AI của mình đang gặp sự cố. Bạn vui lòng thử lại nhé!");
                }
                scrollToBottom();
            })
            .catch(() => {
                typingIndicator.remove();
                appendMessage("AI", "Không thể gửi tin nhắn. Vui lòng kiểm tra lại kết nối mạng!");
                scrollToBottom();
            });
        });
    }

    function appendMessage(sender, text) {
        const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const isStudent = (sender === "STUDENT");

        const item = document.createElement("div");
        item.className = `chat-bubble-wrapper ${isStudent ? 'student' : 'ai'}`;
        
        const avatarImg = isStudent 
            ? document.getElementById("studentAvatarUrl")?.value || "/default-images/avatar.png"
            : "/images/ai-tutor.png";

        item.innerHTML = `
            <img src="${avatarImg}" alt="${sender}" class="chat-bubble-avatar">
            <div class="chat-bubble-content">
                <p class="chat-bubble-text">${escapeHtml(text)}</p>
                <span class="chat-bubble-time">${time}</span>
            </div>
        `;
        messagesLog.appendChild(item);
    }

    function appendTypingIndicator() {
        const item = document.createElement("div");
        item.className = "chat-bubble-wrapper ai typing-indicator-item";
        item.innerHTML = `
            <img src="/images/ai-tutor.png" alt="AI" class="chat-bubble-avatar">
            <div class="chat-bubble-content">
                <div class="typing-indicator">
                    <span class="typing-dot"></span>
                    <span class="typing-dot"></span>
                    <span class="typing-dot"></span>
                </div>
            </div>
        `;
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

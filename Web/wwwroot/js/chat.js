document.addEventListener("DOMContentLoaded", function () {
    if (window.chatConfig) {
        const { chatId, currentUserId, interlocutorId } = window.chatConfig;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("ReceiveMessage", function (json) {
            const msg = JSON.parse(json);
            renderMessage(msg, currentUserId);

            if (msg.senderId !== currentUserId) {
                connection.invoke("MarkAsRead", parseInt(chatId), interlocutorId);
            }
        });

        connection.on("MessagesWereRead", function (readChatId) {
            if (parseInt(readChatId) === parseInt(chatId)) {
                document.querySelectorAll('.fa-check.message-status').forEach(icon => {
                    icon.classList.remove('fa-check');
                    icon.classList.add('fa-check-double', 'text-green-300');
                });
            }
        });

        connection.on("UserOnline", function (userId) {
            if (userId === interlocutorId) updateStatusUI(true);
        });

        connection.on("UserOffline", function (userId) {
            if (userId === interlocutorId) updateStatusUI(false);
        });

        connection.start().then(async () => {
            connection.invoke("JoinChat", parseInt(chatId));
            const isOnline = await connection.invoke("CheckUserOnline", interlocutorId);
            updateStatusUI(isOnline);
            scrollToBottom();

            connection.invoke("MarkAsRead", parseInt(chatId), interlocutorId);
        }).catch(err => console.error("Помилка підключення SignalR:", err));

        const chatForm = document.getElementById("chatForm");
        if (chatForm) {
            chatForm.addEventListener("submit", async (e) => {
                e.preventDefault();
                const input = document.getElementById("messageInput");
                const fileInput = document.getElementById("fileInput");
                if (!input.value.trim() && (!fileInput || !fileInput.files.length)) return;

                const formData = new FormData(e.target);
                const response = await fetch("/Chat/SendMessage", { method: "POST", body: formData });

                if (response.ok) {
                    e.target.reset();
                    resetFile();
                    input.style.height = "auto";
                    scrollToBottom();
                }
            });
        }

        const messageInput = document.getElementById("messageInput");
        if (messageInput) {
            messageInput.addEventListener("input", function () {
                this.style.height = "auto";
                this.style.height = (this.scrollHeight) + "px";
            });
        }

        window.handleFileSelect = function() {
            const file = document.getElementById("fileInput").files[0];
            if (file) {
                document.getElementById("fileName").innerText = file.name;
                document.getElementById("filePreview").classList.remove("hidden");
            }
        };

        window.resetFile = function() {
            document.getElementById("fileInput").value = "";
            document.getElementById("filePreview").classList.add("hidden");
        };

        window.deleteMessage = async function(id) {
            if (!confirm("Видалити це повідомлення для всіх?")) return;
            const res = await fetch(`/Chat/DeleteMessage?messageId=${id}&forEveryone=true`, { method: 'POST' });
            if (res.ok) location.reload();
        };

        window.togglePin = async function(id) {
            const response = await fetch(`/Chat/TogglePin?chatId=${id}`, { method: 'POST' });
            if (response.ok) location.reload();
        };

        window.toggleMenu = function() {
            document.getElementById("chatMenu").classList.toggle("hidden");
        };

        window.confirmDeleteChat = async function(forEveryone) {
            const text = forEveryone ? "Видалити цей чат для вас і для співрозмовника?" : "Видалити цей чат тільки для себе?";
            if (!confirm(text)) return;
            const response = await fetch(`/Chat/DeleteChat?chatId=${chatId}&forEveryone=${forEveryone}`, { method: 'POST' });
            if (response.ok) window.location.href = "/Chat";
            else alert("Помилка видалення чату.");
        };

        document.addEventListener("click", function (event) {
            const menu = document.getElementById("chatMenu");
            if (!menu) return;
            const button = menu.previousElementSibling;
            if (!menu.contains(event.target) && !button.contains(event.target)) {
                menu.classList.add("hidden");
            }
        });

        function scrollToBottom() {
            const container = document.getElementById("messagesContainer");
            if (container) container.scrollTop = container.scrollHeight;
        }

        function updateStatusUI(isOnline) {
            const statusEl = document.getElementById("userStatus");
            if (!statusEl) return;
            if (isOnline) {
                statusEl.textContent = "В мережі";
                statusEl.className = "text-[10px] text-green-500 font-bold uppercase tracking-widest mt-1 transition-colors duration-300";
            } else {
                statusEl.textContent = "Офлайн";
                statusEl.className = "text-[10px] text-gray-400 font-bold uppercase tracking-widest mt-1 transition-colors duration-300";
            }
        }

        function renderMessage(msg, currentUserId) {
            const container = document.getElementById("messagesContainer");
            const isMine = msg.senderId === currentUserId;
            let timeText = "ЩОЙНО";
            if (msg.createdAt) {
                const d = new Date(msg.createdAt);
                timeText = d.toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' });
            }

            const html = `
            <div class="flex ${isMine ? 'justify-end' : 'justify-start'} group">
                <div class="max-w-[80%] md:max-w-[60%]">
                    <div class="p-4 ${isMine ? 'bg-orange-600 text-white rounded-2xl rounded-tr-none shadow-lg shadow-orange-100' : 'bg-white text-gray-800 rounded-2xl rounded-tl-none border border-gray-100 shadow-sm'}">
                        ${msg.filePath ? (msg.fileType === 'Image' ? `<img src="${msg.filePath}" class="rounded-xl w-full h-auto mb-2" />` : `<a href="${msg.filePath}" target="_blank" class="flex items-center gap-3 p-3 bg-black/10 rounded-xl text-xs font-bold transition mb-2"><i class="fa-solid fa-file-arrow-down text-lg"></i><span class="truncate">${msg.fileName}</span></a>`) : ''}
                        <p class="text-sm font-medium leading-relaxed">${msg.content || ''}</p>
                        <div class="flex items-center justify-end gap-1 mt-2 opacity-60">
                            <span class="text-[9px] font-black uppercase">${timeText}</span>
                            ${isMine ? (msg.isRead ? '<i class="fa-solid fa-check-double text-[10px] text-blue-200 message-status"></i>' : '<i class="fa-solid fa-check text-[10px] message-status"></i>') : ''}
                        </div>
                    </div>
                </div>
            </div>`;
            container.insertAdjacentHTML('beforeend', html);
            scrollToBottom();
        }
    }
});
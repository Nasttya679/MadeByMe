document.addEventListener("DOMContentLoaded", function () {
    if (!window.isAuthenticated || !window.currentUserId) return;

    function formatTime(dateString) {
        if (!dateString) return '';
        var date = new Date(dateString);
        return date.toLocaleString('uk-UA', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    function renderNotificationHtml(n, sizeClass, isToast) {
        var isSystem = true;
        var senderName = "Система";
        var msgText = n.message || "Нове сповіщення";

        var isChat = n.actionUrl && n.actionUrl.toLowerCase().indexOf('/chat/room') !== -1;

        if (isChat) {
            isSystem = false; 

            var separator = null;
            if (n.message.indexOf(' -- ') !== -1) separator = ' -- ';
            else if (n.message.indexOf(' - ') !== -1) separator = ' - ';

            if (separator) {
                var parts = n.message.split(separator);
                senderName = parts[0].trim();
                msgText = parts.slice(1).join(separator).trim(); 
            } else {
                senderName = "Користувач";
                msgText = n.message;
            }
        }

        var avatarHtml = "";
        if (isSystem) {
            avatarHtml = '<div class="' + sizeClass + ' rounded-full bg-orange-100 flex items-center justify-center text-orange-500 shrink-0 shadow-sm border border-orange-200"><i class="fa-solid fa-bell text-lg"></i></div>';
        } else {
            var photo = n.senderAvatar || n.imageUrl;
            if (photo) {
                avatarHtml = '<img src="' + photo + '" class="' + sizeClass + ' rounded-full object-cover shrink-0 shadow-sm border border-gray-100" />';
            } else {
                var initial = senderName.charAt(0).toUpperCase();
                avatarHtml = '<div class="' + sizeClass + ' rounded-full bg-gradient-to-br from-orange-400 to-orange-600 flex items-center justify-center text-white font-bold text-lg shrink-0 shadow-sm">' + initial + '</div>';
            }
        }

        var timeHtml = '<span class="text-[10px] text-gray-400 font-bold mt-1 block">' + formatTime(n.createdAt) + '</span>';

        var textClasses = isToast ? "text-sm font-bold text-gray-900 leading-snug truncate" : "text-xs font-bold text-gray-800 truncate";
        var subTextClasses = isToast ? "text-xs text-gray-600 leading-snug truncate mt-1" : "text-xs text-gray-600 truncate mt-1";

        return '<div class="flex items-start gap-3.5 w-full">' +
            avatarHtml +
            '<div class="flex-1 min-w-0">' +
            '<p class="' + textClasses + '">' + senderName + '</p>' +
            '<p class="' + subTextClasses + '">' + msgText + '</p>' +
            timeHtml +
            '</div>' +
            '</div>';
    }

    fetch('/Notification/GetHistory')
        .then(function(response) { return response.json(); })
        .then(function(data) {
            var notifs = Array.isArray(data) ? data : (data.notifications || []);
            var unreadCount = data.unreadCount !== undefined ? data.unreadCount : notifs.filter(function(x) { return !x.isRead; }).length;

            window.updateNotificationBadge(unreadCount, false, true);

            var list = document.getElementById("notificationList");
            if (notifs && notifs.length > 0) {
                var hasUnread = false;
                list.innerHTML = "";

                for (var i = 0; i < notifs.length; i++) {
                    var n = notifs[i];
                    if (n.isRead) continue;

                    hasUnread = true;
                    addNotificationToList(n, true);
                }

                if (!hasUnread) {
                    list.innerHTML = '<div class="p-8 text-center text-gray-400 text-xs italic">Поки що немає нових сповіщень</div>';
                }
            }
        });

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", function (n) {
        if (window.chatConfig && window.chatConfig.chatId && n.actionUrl) {
            var currentRoomUrlPart = "/chat/room/" + window.chatConfig.chatId;
            if (n.actionUrl.toLowerCase().indexOf(currentRoomUrlPart) !== -1) {
                fetch('/Notification/MarkAsRead?id=' + n.id, { method: 'POST' }).catch(function(){});
                return;
            }
        }

        showNotificationToast(n);

        if (!document.getElementById("list-item-" + n.id)) {
            addNotificationToList(n, false);
            window.updateNotificationBadge(1, true, false);
        }
    });

    connection.start().catch(function(err) { console.error("SignalR Error: ", err); });

    function showNotificationToast(n) {
        if (document.getElementById("toast-" + n.id)) return;

        var container = document.getElementById("toastContainer");
        if (!container) return;

        var toast = document.createElement("div");
        toast.id = "toast-" + n.id;

        toast.className = "bg-white p-4 rounded-2xl shadow-2xl border border-gray-100 block cursor-pointer hover:bg-gray-50";
        toast.style.animation = "slideIn 0.5s cubic-bezier(0.25, 1, 0.5, 1) forwards";
        toast.style.minWidth = "300px";

        toast.innerHTML = renderNotificationHtml(n, "w-10 h-10", true);

        if (n.actionUrl) {
            toast.onclick = function() {
                fetch('/Notification/MarkAsRead?id=' + n.id, { method: 'POST' }).then(function() {
                    window.location.href = n.actionUrl;
                });
            };
        }

        container.appendChild(toast);

        setTimeout(function() {
            toast.style.animation = "slideOut 0.4s ease-in forwards";
            setTimeout(function() { toast.remove(); }, 400);
        }, 5000);
    }

    function addNotificationToList(n, isHistory) {
        if (document.getElementById("list-item-" + n.id)) return;

        var list = document.getElementById("notificationList");
        if (list.innerHTML.indexOf("немає нових сповіщень") !== -1) {
            list.innerHTML = "";
        }

        var item = document.createElement("a");
        item.id = "list-item-" + n.id;
        item.href = n.actionUrl ? n.actionUrl : "#";
        item.className = "block p-4 hover:bg-orange-50 transition border-l-4 border-orange-500 bg-orange-50";

        item.innerHTML = renderNotificationHtml(n, "w-9 h-9", false);

        item.onclick = function() { fetch('/Notification/MarkAsRead?id=' + n.id, { method: 'POST' }); };

        if (isHistory) {
            list.appendChild(item);
        } else {
            list.insertBefore(item, list.firstChild);
        }
    }

    var btn = document.getElementById("notificationBtn");
    var dropdown = document.getElementById("notificationDropdown");

    if (btn) {
        btn.onclick = function(e) {
            e.stopPropagation();
            dropdown.classList.toggle("hidden");
        };
    }

    window.addEventListener('click', function(e) {
        if (dropdown && !dropdown.contains(e.target) && btn && !btn.contains(e.target)) {
            dropdown.classList.add("hidden");
        }
    });
});

window.clearAllNotifications = function(e) {
    if (e) e.stopPropagation();
    fetch('/Notification/MarkAllAsRead', { method: 'POST' }).then(function() {
        var list = document.getElementById("notificationList");
        if (list) {
            list.innerHTML = '<div class="p-8 text-center text-gray-400 text-xs italic">Поки що немає нових сповіщень</div>';
        }
        window.updateNotificationBadge(0, false, true);
    });
};

window.updateNotificationBadge = function(amount, animate, isExact) {
    var badge = document.getElementById("notification-badge");
    if (!badge) return;

    var current = parseInt(badge.innerText) || 0;
    var next = isExact ? amount : current + amount;

    if (next > 0) {
        badge.innerText = next;
        badge.classList.remove("hidden");
        if (animate) {
            badge.classList.add("scale-125");
            setTimeout(function() { badge.classList.remove("scale-125"); }, 200);
        }
    } else {
        badge.classList.add("hidden");
        badge.innerText = "0";
    }
};
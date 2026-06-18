// File Path: src/HRManagement.Web/wwwroot/js/notifications.js

$(document).ready(function () {
    const fetchNotifications = function () {
        $.ajax({
            url: '/api/notifications/recent',
            method: 'GET',
            success: function (data) {
                renderNotifications(data.notifications, data.unreadCount);
            },
            error: function (err) {
                console.error("Failed to fetch notifications", err);
            }
        });
    };

    const renderNotifications = function (notifications, unreadCount) {
        // Update badge
        const badgeSpan = $('#notifyBadge');
        if (unreadCount > 0) {
            badgeSpan.text(unreadCount).show();
        } else {
            badgeSpan.hide();
        }

        const listContainer = $('#notificationList');
        listContainer.empty();

        if (notifications.length === 0) {
            listContainer.append(`
                <li class="p-4 text-center text-muted">
                    <div class="mb-2 text-secondary" style="font-size: 2rem;">
                        <i class="fa-solid fa-bell-slash text-muted"></i>
                    </div>
                    <div class="fw-semibold small">No notifications available</div>
                    <div class="text-secondary" style="font-size: 0.75rem;">We'll alert you when there are updates.</div>
                </li>
            `);
            return;
        }

        notifications.forEach(n => {
            const readClass = n.isRead ? 'text-muted' : 'fw-bold text-dark';
            const bgClass = n.isRead ? '' : 'bg-light';
            const html = `
                <li class="border-bottom p-3 ${bgClass}" style="cursor: pointer;" onclick="markAsRead(${n.id}, '${n.targetUrl || '/Home/Index'}')">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="${readClass}">${n.title}</span>
                        <span class="small text-muted">${n.timeAgo}</span>
                    </div>
                    <div class="small text-muted">${n.message}</div>
                </li>
            `;
            listContainer.append(html);
        });

        // Add 'Mark all as read' if there are unread
        if (unreadCount > 0) {
            listContainer.append(`
                <li class="p-2 text-center border-top">
                    <button class="btn btn-sm btn-link text-decoration-none w-100" onclick="markAllAsRead()">Mark all as read</button>
                </li>
            `);
        }
    };

    window.markAsRead = function (id, targetUrl) {
        $.ajax({
            url: '/api/notifications/mark-read/' + id,
            method: 'POST',
            success: function () {
                if (targetUrl) {
                    window.location.href = targetUrl;
                } else {
                    window.location.href = '/Home/Index';
                }
            }
        });
    };

    window.markAllAsRead = function () {
        $.ajax({
            url: '/api/notifications/mark-all-read',
            method: 'POST',
            success: function () {
                fetchNotifications();
            }
        });
    };

    // Initial fetch
    fetchNotifications();

    // Poll every 60 seconds
    setInterval(fetchNotifications, 60000);
});

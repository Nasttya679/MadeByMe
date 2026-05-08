document.addEventListener("DOMContentLoaded", function() {
    setTimeout(function() {
        const alerts = document.querySelectorAll('.js-alert');
        alerts.forEach(alert => {
            alert.classList.add('fade-out');
            setTimeout(() => alert.remove(), 1000);
        });
    }, 5000);

    if (window.isAuthenticated) {
        fetch('/Wishlist/GetWishlistCount')
            .then(response => response.json())
            .then(data => {
                updateWishlistBadge(data.count, false);
            })
            .catch(err => console.error("Помилка завантаження обраного:", err));
    }
});

function updateWishlistBadge(count, animate) {
    const badge = document.getElementById('wishlist-badge');
    if (!badge) return;

    if (count > 0) {
        badge.innerText = count;
        badge.classList.remove('hidden');
        if (animate) {
            badge.classList.add('animate-bounce');
            setTimeout(() => badge.classList.remove('animate-bounce'), 1000);
        }
    } else {
        badge.classList.add('hidden');
        badge.innerText = '0';
    }
}

window.clearCartAndRedirect = function() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenInput ? tokenInput.value : '';
    fetch('/Cart/ClearCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        }
    }).then(() => {
        window.location.href = '/Order/History';
    });
}

window.removeWishlistItem = function(btn, postId) {
    if (typeof toggleWishlist === 'function') {
        toggleWishlist(btn, postId);
    }
    const card = document.getElementById(`wishlist-item-${postId}`);
    if (card) {
        card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
        card.style.opacity = '0';
        card.style.transform = 'scale(0.8)';
        setTimeout(() => {
            card.remove();
            const remainingItems = document.querySelectorAll('[id^="wishlist-item-"]').length;
            if (remainingItems === 0) {
                window.location.reload();
            }
        }, 400);
    }
};

window.addToCartSilent = async function(event, form) {
    event.preventDefault();
    const btn = form.querySelector('button');
    const originalHtml = btn.innerHTML;

    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin mr-2"></i> Додаємо...';
    btn.disabled = true;
    try {
        const formData = new FormData(form);
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData
        });
        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                btn.innerHTML = '<i class="fa-solid fa-check mr-2"></i> В кошику!';
                btn.classList.remove('bg-orange-600', 'hover:bg-orange-700');
                btn.classList.add('bg-green-600', 'hover:bg-green-700');

                const cartBadge = document.getElementById('cart-badge');
                if (cartBadge) {
                    cartBadge.innerText = result.cartCount;
                    cartBadge.classList.remove('hidden');
                    cartBadge.classList.add('scale-125');
                    setTimeout(() => cartBadge.classList.remove('scale-125'), 300);
                }

                setTimeout(() => {
                    const postId = form.querySelector('input[name="postId"]').value;
                    window.removeWishlistItem(btn, postId);
                }, 1200);
            } else {
                alert('Помилка: ' + result.message);
                btn.innerHTML = originalHtml;
                btn.disabled = false;
            }
        }
    } catch (error) {
        console.error('Помилка:', error);
        btn.innerHTML = originalHtml;
        btn.disabled = false;
    }
    return false;
};

document.addEventListener("DOMContentLoaded", function() {
    if (typeof flatpickr !== "undefined") {
        flatpickr("#datePicker, #sellerDatePicker", {
            locale: "uk",
            dateFormat: "Y-m-d",
            altInput: true,
            altFormat: "d.m.Y",
            disableMobile: "true"
        });
    }
});

window.animateRemoval = function(event, itemId) {
    event.preventDefault();
    var form = event.target;
    var itemRow = document.getElementById(itemId);

    if (itemRow) {
        itemRow.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
        itemRow.style.opacity = '0';
        itemRow.style.transform = 'translateX(-30px)';

        setTimeout(function() {
            form.submit();
        }, 300);
    } else {
        form.submit();
    }
};
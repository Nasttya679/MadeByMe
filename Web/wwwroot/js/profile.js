function toggleEditMode(isEdit) {
    var viewElements = document.querySelectorAll('.view-mode');
    var editElements = document.querySelectorAll('.edit-mode');
    var editBtn = document.getElementById('editBtn');
    var cancelBtn = document.getElementById('cancelBtn');
    var saveBtn = document.getElementById('saveBtn');

    if (isEdit) {
        viewElements.forEach(function(el) { el.classList.add('hidden'); });
        editElements.forEach(function(el) { el.classList.remove('hidden'); });
        editBtn.classList.add('hidden');
        cancelBtn.classList.remove('hidden');
        saveBtn.classList.remove('hidden');
    } else {
        viewElements.forEach(function(el) { el.classList.remove('hidden'); });
        editElements.forEach(function(el) { el.classList.add('hidden'); });
        editBtn.classList.remove('hidden');
        cancelBtn.classList.add('hidden');
        saveBtn.classList.add('hidden');
        var avatarInput = document.getElementById('avatarInput');
        if (avatarInput) avatarInput.value = '';
    }
}

document.addEventListener("DOMContentLoaded", function() {
    var avatarInput = document.getElementById('avatarInput');
    if (avatarInput) {
        avatarInput.addEventListener('change', function (e) {
            if (e.target.files && e.target.files[0]) {
                var reader = new FileReader();
                reader.onload = function (event) {
                    var container = document.getElementById('avatarContainer');
                    if (container) container.innerHTML = '<img id="profileImagePreview" src="' + event.target.result + '" class="w-full h-full object-cover" />';
                }
                reader.readAsDataURL(e.target.files[0]);
            }
        });
    }
});

window.toggleDescriptionEdit = function(showEdit) {
    var viewBlock = document.getElementById('viewDescriptionBlock');
    var editForm = document.getElementById('editDescriptionForm');
    var inputField = document.getElementById('sellerDescriptionInput');

    if (viewBlock && editForm && inputField) {
        if (showEdit) {
            viewBlock.classList.add('hidden');
            editForm.classList.remove('hidden');
            inputField.focus();
        } else {
            viewBlock.classList.remove('hidden');
            editForm.classList.add('hidden');
            inputField.value = inputField.getAttribute('data-original');
        }
    }
};

window.toggleWishlist = function(button, postId) {
    if (button.classList.contains('pointer-events-none')) return;
    button.classList.add('pointer-events-none');

    var icon = button.querySelector('i');
    var formData = new FormData();
    formData.append('postId', postId);

    // Безпечне отримання токена без "?."
    var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    var token = tokenElement ? tokenElement.value : '';

    fetch('/Wishlist/ToggleFavorite', {
        method: 'POST',
        body: formData,
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(function(response) {
            if (!response.ok) throw new Error('Помилка мережі');
            return response.json();
        })
        .then(function(data) {
            if (data.success) {
                if (data.isAdded) {
                    icon.classList.remove('fa-regular');
                    icon.classList.add('fa-solid', 'text-red-500');
                } else {
                    icon.classList.remove('fa-solid', 'text-red-500');
                    icon.classList.add('fa-regular');
                    icon.classList.remove('text-red-500');
                }

                var badge = document.getElementById('wishlist-badge');
                if (badge) {
                    badge.innerText = data.totalCount;
                    if (data.totalCount > 0) {
                        badge.classList.remove('hidden');
                    } else {
                        badge.classList.add('hidden');
                    }
                }
            }
        })
        .catch(function(err) { console.error('Wishlist Error:', err); })
        .finally(function() {
            button.classList.remove('pointer-events-none');
        });
};
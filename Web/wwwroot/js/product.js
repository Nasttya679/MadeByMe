document.addEventListener("DOMContentLoaded", function() {
    const stars = document.querySelectorAll('#starRating i');
    const ratingInput = document.getElementById('ratingInput');

    if (stars.length > 0 && ratingInput) {
        if (ratingInput.value > 0) highlightStars(ratingInput.value);

        stars.forEach(star => {
            star.addEventListener('mouseover', function() { highlightStars(this.getAttribute('data-value')); });
            star.addEventListener('mouseout', function() { highlightStars(ratingInput.value || 0); });
            star.addEventListener('click', function() {
                ratingInput.value = this.getAttribute('data-value');
                highlightStars(this.getAttribute('data-value'));
            });
        });

        function highlightStars(count) {
            stars.forEach(star => {
                const value = star.getAttribute('data-value');
                if (value <= count) {
                    star.classList.remove('text-gray-300');
                    star.classList.add('text-yellow-400');
                } else {
                    star.classList.remove('text-yellow-400');
                    star.classList.add('text-gray-300');
                }
            });
        }
    }
});

window.previewImage = function(input) {
    const preview = document.getElementById('preview');
    const previewContainer = document.getElementById('imagePreview');
    const uploadPrompt = document.getElementById('uploadPrompt');

    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            preview.src = e.target.result;
            if(previewContainer) previewContainer.classList.remove('hidden');
            if(uploadPrompt) uploadPrompt.classList.add('hidden');
        }
        reader.readAsDataURL(input.files[0]);
    } else {
        if (previewContainer) previewContainer.classList.add('hidden');
        if (uploadPrompt) uploadPrompt.classList.remove('hidden');
    }
};
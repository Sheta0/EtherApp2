document.addEventListener('DOMContentLoaded', function() {
    const mobileToggle = document.getElementById('mobile-search-toggle');
    const searchForm = document.getElementById('search--box');
    
    if (mobileToggle && searchForm) {
        mobileToggle.addEventListener('click', function() {
            searchForm.classList.toggle('max-md:block');
        });
    }
});
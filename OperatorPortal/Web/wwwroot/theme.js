document.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('theme');
    savedTheme && document.documentElement.setAttribute('data-theme', savedTheme);
});
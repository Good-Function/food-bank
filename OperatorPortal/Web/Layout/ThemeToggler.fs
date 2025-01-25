module Layout.ThemeToggler

open Oxpecker.ViewEngine

let Component =
    raw
        """
<button class="outline" style="border:none; border-radius:50%; background-color: var(--pico-contrast); padding: 8px; outline: none; box-shadow: none; color: var(--pico-contrast-inverse);"
        onclick="
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            console.log('HAH');
        ">
    <div style="width: 30px; height: 30px;">
        <svg xmlns="http://www.w3.org/2000/svg" 
             width="24" 
             height="24" 
             viewBox="0 0 24 24" 
             fill="none" 
             stroke="currentColor" 
             stroke-width="2" 
             stroke-linecap="round" 
             stroke-linejoin="round" 
             class="lucide lucide-lamp-desk">
            <path d="m14 5-3 3 2 7 8-8-7-2Z"></path>
            <path d="m14 5-3 3-3-3 3-3 3 3Z"></path>
            <path d="M9.5 6.5 4 12l3 6"></path>
            <path d="M3 22v-2c0-1.1.9-2 2-2h4a2 2 0 0 1 2 2v2H3Z"></path>
        </svg>
    </div>
</button>
"""

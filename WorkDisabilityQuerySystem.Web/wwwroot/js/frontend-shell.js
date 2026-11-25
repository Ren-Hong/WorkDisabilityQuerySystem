const sidebarToggle = document.querySelector("#wdqs-sidebar-toggle");
const sidebar = document.querySelector("#wdqs-sidebar");
const wideScreen = matchMedia("(min-width: 992px)");

function setSidebarOpen(open) {
    sidebar.classList.toggle("is-open", open);
    sidebar.inert = !open;
    sidebarToggle.setAttribute("aria-expanded", String(open));
}

setSidebarOpen(wideScreen.matches);
wideScreen.addEventListener("change", (event) => setSidebarOpen(event.matches));
sidebarToggle.addEventListener("click", () => setSidebarOpen(sidebar.inert));
document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && !sidebar.inert && !wideScreen.matches) {
        setSidebarOpen(false);
        sidebarToggle.focus();
    }
});


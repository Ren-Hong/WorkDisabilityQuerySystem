const sidebarToggle = document.querySelector("#wdqs-sidebar-toggle");
const sidebar = document.querySelector("#wdqs-sidebar");
const backdrop = document.querySelector("#wdqs-sidebar-backdrop");
const firstLink = sidebar.querySelector("nav a:first-child");
const header = document.querySelector(".wdqs-header");
const main = document.querySelector(".wdqs-workspace-main");

function setSidebarOpen(open) {
    sidebar.classList.toggle("is-open", open);
    backdrop.classList.toggle("is-open", open);
    document.body.classList.toggle("wdqs-drawer-open", open);
    sidebar.inert = !open;
    header.inert = open;
    main.inert = open;
    sidebarToggle.setAttribute("aria-expanded", String(open));
    (open ? firstLink : sidebarToggle).focus();
}

sidebarToggle.addEventListener("click", () => setSidebarOpen(true));
backdrop.addEventListener("click", () => setSidebarOpen(false));

sidebar.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        event.preventDefault();
        setSidebarOpen(false);
    } else if (event.key === "Tab") {
        const lastLink = sidebar.querySelector("nav a:last-child");
        if (event.shiftKey && document.activeElement === firstLink && lastLink) {
            event.preventDefault();
            lastLink.focus();
        } else if (!event.shiftKey && document.activeElement === lastLink) {
            event.preventDefault();
            firstLink.focus();
        }
    }
});

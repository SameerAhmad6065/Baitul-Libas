// Bait Al Libas — admin console interaction layer
document.addEventListener("DOMContentLoaded", function () {

    var sidebar = document.getElementById("adminSidebar");
    var toggle = document.getElementById("adminSidebarToggle");

    if (toggle && sidebar) {
        toggle.addEventListener("click", function () {
            sidebar.classList.toggle("is-open");
        });

        document.addEventListener("click", function (e) {
            if (window.innerWidth > 991.98) return;
            if (!sidebar.contains(e.target) && !toggle.contains(e.target)) {
                sidebar.classList.remove("is-open");
            }
        });
    }

    // mark the current nav link active based on the URL path
    var links = document.querySelectorAll(".admin-nav-link:not(.is-disabled)");
    var path = window.location.pathname.replace(/\/+$/, "").toLowerCase();
    links.forEach(function (link) {
        var linkPath = link.getAttribute("href");
        if (!linkPath) return;
        linkPath = linkPath.replace(/\/+$/, "").toLowerCase();
        if (linkPath && (path === linkPath || path.indexOf(linkPath) === 0)) {
            link.classList.add("active");
        }
    });
});

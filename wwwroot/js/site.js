// Bait Al Libas — interaction layer
document.addEventListener("DOMContentLoaded", function () {

    var nav = document.getElementById("mainNav");
    if (nav) {
        var onScroll = function () {
            if (window.scrollY > 30) nav.classList.add("scrolled");
            else nav.classList.remove("scrolled");
        };
        document.addEventListener("scroll", onScroll, { passive: true });
        onScroll();
    }

    var targets = document.querySelectorAll(".reveal, .arch-divider");
    if ("IntersectionObserver" in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("in-view");
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });
        targets.forEach(function (el) { io.observe(el); });
    } else {
        targets.forEach(function (el) { el.classList.add("in-view"); });
    }
});

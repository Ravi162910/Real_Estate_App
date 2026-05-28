// Real Estate App - front-end polish
// Progressive enhancement only: if any piece fails or JS is disabled,
// the page still works and content stays fully visible.

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        initNavbarScrollShadow();
        initScrollReveal();
        initBackToTop();
    });

    // Add a subtle shadow to the sticky navbar once the page is scrolled.
    function initNavbarScrollShadow() {
        var navbar = document.querySelector(".navbar");
        if (!navbar) {
            return;
        }
        var onScroll = function () {
            if (window.scrollY > 8) {
                navbar.classList.add("rea-scrolled");
            } else {
                navbar.classList.remove("rea-scrolled");
            }
        };
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();
    }

    // Fade-and-rise cards into view as they enter the viewport.
    function initScrollReveal() {
        var cards = document.querySelectorAll("main .card");
        if (!cards.length) {
            return;
        }

        var reduceMotion = window.matchMedia &&
            window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        // No IntersectionObserver support, or user prefers reduced motion:
        // leave everything visible (CSS keeps non-reveal cards visible).
        if (reduceMotion || typeof IntersectionObserver === "undefined") {
            return;
        }

        var observer = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("rea-in");
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.08, rootMargin: "0px 0px -40px 0px" });

        cards.forEach(function (card) {
            card.classList.add("rea-reveal");
            observer.observe(card);
        });
    }

    // Inject a floating "back to top" button that appears after scrolling.
    function initBackToTop() {
        var btn = document.createElement("button");
        btn.type = "button";
        btn.className = "rea-to-top";
        btn.setAttribute("aria-label", "Back to top");
        btn.innerHTML = '<i class="bi bi-arrow-up"></i>';
        document.body.appendChild(btn);

        var toggle = function () {
            if (window.scrollY > 320) {
                btn.classList.add("rea-show");
            } else {
                btn.classList.remove("rea-show");
            }
        };

        btn.addEventListener("click", function () {
            var reduceMotion = window.matchMedia &&
                window.matchMedia("(prefers-reduced-motion: reduce)").matches;
            window.scrollTo({ top: 0, behavior: reduceMotion ? "auto" : "smooth" });
        });

        window.addEventListener("scroll", toggle, { passive: true });
        toggle();
    }
})();

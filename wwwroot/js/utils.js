// General utility functions for dashboard
(function () {
    window.Utils = window.Utils || {};

    Utils.debounce = function (func, wait) {
        var timeout;
        return function executedFunction() {
            var later = function () {
                clearTimeout(timeout);
                func();
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    };

    Utils.throttle = function (func, limit) {
        var inThrottle;
        return function () {
            if (!inThrottle) {
                func.apply(this, arguments);
                inThrottle = true;
                setTimeout(function () {
                    inThrottle = false;
                }, limit);
            }
        };
    };

    Utils.parseJson = function (jsonString) {
        try {
            return JSON.parse(jsonString);
        } catch (e) {
            console.error('JSON parse error: ', e);
            return null;
        }
    };

    Utils.stringifyJson = function (obj) {
        try {
            return JSON.stringify(obj, null, 2);
        } catch (e) {
            console.error('JSON stringify error: ', e);
            return '';
        }
    };

    Utils.addClass = function (element, className) {
        if (element && element.classList) {
            element.classList.add(className);
        }
    };

    Utils.removeClass = function (element, className) {
        if (element && element.classList) {
            element.classList.remove(className);
        }
    };

    Utils.hasClass = function (element, className) {
        if (element && element.classList) {
            return element.classList.contains(className);
        }
        return false;
    };

    Utils.toggleClass = function (element, className) {
        if (element && element.classList) {
            element.classList.toggle(className);
        }
    };

    Utils.queryAll = function (selector) {
        return document.querySelectorAll(selector);
    };

    Utils.query = function (selector) {
        return document.querySelector(selector);
    };

    Utils.log = function (message, data) {
        if (window.console && typeof console.log === 'function') {
            if (data) {
                console.log(message, data);
            } else {
                console.log(message);
            }
        }
    };

    Utils.warn = function (message, data) {
        if (window.console && typeof console.warn === 'function') {
            if (data) {
                console.warn(message, data);
            } else {
                console.warn(message);
            }
        }
    };

    Utils.error = function (message, data) {
        if (window.console && typeof console.error === 'function') {
            if (data) {
                console.error(message, data);
            } else {
                console.error(message);
            }
        }
    };
})();
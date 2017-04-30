﻿// Tween.js - http://github.com/sole/tween.js
var TWEEN = TWEEN || function () { var a, e, c, d, f = []; return { start: function (g) { c = setInterval(this.update, 1E3 / (g || 60)) }, stop: function () { clearInterval(c) }, add: function (g) { f.push(g) }, remove: function (g) { a = f.indexOf(g); a !== -1 && f.splice(a, 1) }, update: function () { a = 0; e = f.length; for (d = (new Date).getTime(); a < e;)if (f[a].update(d)) a++; else { f.splice(a, 1); e-- } } } }();
TWEEN.Tween = function (a) {
    var e = {}, c = {}, d = {}, f = 1E3, g = 0, j = null, n = TWEEN.Easing.Linear.EaseNone, k = null, l = null, m = null; this.to = function (b, h) { if (h !== null) f = h; for (var i in b) if (a[i] !== null) d[i] = b[i]; return this }; this.start = function () { TWEEN.add(this); j = (new Date).getTime() + g; for (var b in d) if (a[b] !== null) { e[b] = a[b]; c[b] = d[b] - a[b] } return this }; this.stop = function () { TWEEN.remove(this); return this }; this.delay = function (b) { g = b; return this }; this.easing = function (b) { n = b; return this }; this.chain = function (b) { k = b }; this.onUpdate =
        function (b) { l = b; return this }; this.onComplete = function (b) { m = b; return this }; this.update = function (b) { var h, i; if (b < j) return true; b = (b - j) / f; b = b > 1 ? 1 : b; i = n(b); for (h in c) a[h] = e[h] + c[h] * i; l !== null && l.call(a, i); if (b == 1) { m !== null && m.call(a); k !== null && k.start(); return false } return true }
}; TWEEN.Easing = { Linear: {}, Quadratic: {}, Cubic: {}, Quartic: {}, Quintic: {}, Sinusoidal: {}, Exponential: {}, Circular: {}, Elastic: {}, Back: {}, Bounce: {} }; TWEEN.Easing.Linear.EaseNone = function (a) { return a };
TWEEN.Easing.Quadratic.EaseIn = function (a) { return a * a }; TWEEN.Easing.Quadratic.EaseOut = function (a) { return -a * (a - 2) }; TWEEN.Easing.Quadratic.EaseInOut = function (a) { if ((a *= 2) < 1) return 0.5 * a * a; return -0.5 * (--a * (a - 2) - 1) }; TWEEN.Easing.Cubic.EaseIn = function (a) { return a * a * a }; TWEEN.Easing.Cubic.EaseOut = function (a) { return --a * a * a + 1 }; TWEEN.Easing.Cubic.EaseInOut = function (a) { if ((a *= 2) < 1) return 0.5 * a * a * a; return 0.5 * ((a -= 2) * a * a + 2) }; TWEEN.Easing.Quartic.EaseIn = function (a) { return a * a * a * a };
TWEEN.Easing.Quartic.EaseOut = function (a) { return -(--a * a * a * a - 1) }; TWEEN.Easing.Quartic.EaseInOut = function (a) { if ((a *= 2) < 1) return 0.5 * a * a * a * a; return -0.5 * ((a -= 2) * a * a * a - 2) }; TWEEN.Easing.Quintic.EaseIn = function (a) { return a * a * a * a * a }; TWEEN.Easing.Quintic.EaseOut = function (a) { return (a -= 1) * a * a * a * a + 1 }; TWEEN.Easing.Quintic.EaseInOut = function (a) { if ((a *= 2) < 1) return 0.5 * a * a * a * a * a; return 0.5 * ((a -= 2) * a * a * a * a + 2) }; TWEEN.Easing.Sinusoidal.EaseIn = function (a) { return -Math.cos(a * Math.PI / 2) + 1 };
TWEEN.Easing.Sinusoidal.EaseOut = function (a) { return Math.sin(a * Math.PI / 2) }; TWEEN.Easing.Sinusoidal.EaseInOut = function (a) { return -0.5 * (Math.cos(Math.PI * a) - 1) }; TWEEN.Easing.Exponential.EaseIn = function (a) { return a == 0 ? 0 : Math.pow(2, 10 * (a - 1)) }; TWEEN.Easing.Exponential.EaseOut = function (a) { return a == 1 ? 1 : -Math.pow(2, -10 * a) + 1 }; TWEEN.Easing.Exponential.EaseInOut = function (a) { if (a == 0) return 0; if (a == 1) return 1; if ((a *= 2) < 1) return 0.5 * Math.pow(2, 10 * (a - 1)); return 0.5 * (-Math.pow(2, -10 * (a - 1)) + 2) };
TWEEN.Easing.Circular.EaseIn = function (a) { return -(Math.sqrt(1 - a * a) - 1) }; TWEEN.Easing.Circular.EaseOut = function (a) { return Math.sqrt(1 - --a * a) }; TWEEN.Easing.Circular.EaseInOut = function (a) { if ((a /= 0.5) < 1) return -0.5 * (Math.sqrt(1 - a * a) - 1); return 0.5 * (Math.sqrt(1 - (a -= 2) * a) + 1) }; TWEEN.Easing.Elastic.EaseIn = function (a) { var e, c = 0.1, d = 0.4; if (a == 0) return 0; if (a == 1) return 1; d || (d = 0.3); if (!c || c < 1) { c = 1; e = d / 4 } else e = d / (2 * Math.PI) * Math.asin(1 / c); return -(c * Math.pow(2, 10 * (a -= 1)) * Math.sin((a - e) * 2 * Math.PI / d)) };
TWEEN.Easing.Elastic.EaseOut = function (a) { var e, c = 0.1, d = 0.4; if (a == 0) return 0; if (a == 1) return 1; d || (d = 0.3); if (!c || c < 1) { c = 1; e = d / 4 } else e = d / (2 * Math.PI) * Math.asin(1 / c); return c * Math.pow(2, -10 * a) * Math.sin((a - e) * 2 * Math.PI / d) + 1 };
TWEEN.Easing.Elastic.EaseInOut = function (a) { var e, c = 0.1, d = 0.4; if (a == 0) return 0; if (a == 1) return 1; d || (d = 0.3); if (!c || c < 1) { c = 1; e = d / 4 } else e = d / (2 * Math.PI) * Math.asin(1 / c); if ((a *= 2) < 1) return -0.5 * c * Math.pow(2, 10 * (a -= 1)) * Math.sin((a - e) * 2 * Math.PI / d); return c * Math.pow(2, -10 * (a -= 1)) * Math.sin((a - e) * 2 * Math.PI / d) * 0.5 + 1 }; TWEEN.Easing.Back.EaseIn = function (a) { return a * a * (2.70158 * a - 1.70158) }; TWEEN.Easing.Back.EaseOut = function (a) { return (a -= 1) * a * (2.70158 * a + 1.70158) + 1 };
TWEEN.Easing.Back.EaseInOut = function (a) { if ((a *= 2) < 1) return 0.5 * a * a * (3.5949095 * a - 2.5949095); return 0.5 * ((a -= 2) * a * (3.5949095 * a + 2.5949095) + 2) }; TWEEN.Easing.Bounce.EaseIn = function (a) { return 1 - TWEEN.Easing.Bounce.EaseOut(1 - a) }; TWEEN.Easing.Bounce.EaseOut = function (a) { return (a /= 1) < 1 / 2.75 ? 7.5625 * a * a : a < 2 / 2.75 ? 7.5625 * (a -= 1.5 / 2.75) * a + 0.75 : a < 2.5 / 2.75 ? 7.5625 * (a -= 2.25 / 2.75) * a + 0.9375 : 7.5625 * (a -= 2.625 / 2.75) * a + 0.984375 };
TWEEN.Easing.Bounce.EaseInOut = function (a) { if (a < 0.5) return TWEEN.Easing.Bounce.EaseIn(a * 2) * 0.5; return TWEEN.Easing.Bounce.EaseOut(a * 2 - 1) * 0.5 + 0.5 };

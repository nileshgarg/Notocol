(function () {
// Prevent double embedding
if (typeof(window.annotator) === 'undefined') {
    window.annotator = {};
} else {
    return;
}



// Injects the hypothesis dependencies. These can be either js or css, the
// file extension is used to determine the loading method. This file is
// pre-processed in order to insert the wgxpath, url and inject scripts.
//
// Custom injectors can be provided to load the scripts into a different
// environment. Both script and stylesheet methods are provided with a url
// and a callback fn that expects either an error object or null as the only
// argument.
//
// For example a Chrome extension may look something like:
//
//   window.hypothesisInstall({
//     script: function (src, fn) {
//       chrome.tabs.executeScript(tab.id, {file: src}, fn);
//     },
//     stylesheet: function (href, fn) {
//       chrome.tabs.insertCSS(tab.id, {file: href}, fn);
//     }
//   });
window.hypothesisInstall = function (inject) {
  inject = inject || {};

  var resources = [];
  var injectStylesheet = inject.stylesheet || function injectStylesheet(href, fn) {
    var link = document.createElement('link');
    link.rel = 'stylesheet';
    link.type = 'text/css';
    link.href = href;

    document.head.appendChild(link);
    fn(null);
  };

  var injectScript = inject.script || function injectScript(src, fn) {
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.onload = function () { fn(null) };
    script.onerror = function () { fn(new Error('Failed to load script: ' + src)) };
    script.src = src;

    document.head.appendChild(script);
  };

  

  if (!window.document.evaluate) {
    resources.push('app/public/scripts/vendor/wgxpath.install.min.js?0cd5ec8a');
  }

  // https://github.com/Modernizr/Modernizr/blob/master/feature-detects/url/parser.js
  var url, urlWorks;
  try {
    // have to actually try use it, because Safari defines a dud constructor
    url = new URL('http://modernizr.com/');
    urlWorks = url.href === 'http://modernizr.com/';
  }
  catch (err) {
    urlWorks = false;
  }
  if (!urlWorks) {
    resources.push('app/public/scripts/vendor/url.min.js?de686538');
  }

  if (typeof window.Annotator === 'undefined') {
    resources.push('app/public/styles/hypothesis.min.css?53837104');
    resources.push('app/public/scripts/hypothesis.min.js?8e970d82');
  }

  (function next(err) {
    if (err) { throw err; }

    if (resources.length) {
      var url = resources.shift();
      var ext = url.split('?')[0].split('.').pop();
      var fn = (ext === 'css' ? injectStylesheet : injectScript);
      fn(url, next);
    }
  })();
}

var baseUrl = document.createElement('link');
baseUrl.rel = 'sidebar';
baseUrl.href = 'app/public/app.html';
baseUrl.type = 'application/annotator+html';
document.head.appendChild(baseUrl);

window.hypothesisInstall();
})();
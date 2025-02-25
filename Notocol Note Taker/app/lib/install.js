(function () {
  'use strict';

  var browserExtension = new h.HypothesisChromeExtension({
    chromeTabs: chrome.tabs,
    chromeBrowserAction: chrome.browserAction,
    extensionURL: function (path) {
      return chrome.extension.getURL(path);
    },
    isAllowedFileSchemeAccess: function (fn) {
      return chrome.extension.isAllowedFileSchemeAccess(fn);
    },
  });

  browserExtension.listen(window);
  chrome.runtime.onInstalled.addListener(onInstalled);
  chrome.runtime.requestUpdateCheck(function (status) {
    chrome.runtime.onUpdateAvailable.addListener(onUpdateAvailable);
  });

  var notocolUtil = new h.NotocolUtil({
      chromeTabs: chrome.tabs,
      extensionURL: function (path) {
          return chrome.extension.getURL(path);
      },
      isAllowedFileSchemeAccess: function (fn) {
          return chrome.extension.isAllowedFileSchemeAccess(fn);
      },
      hypothesis: browserExtension
  })


  function onInstalled(installDetails) {
    if (installDetails.reason === 'install') {
      browserExtension.firstRun();
    }

    // We need this so that 3rd party cookie blocking does not kill us.
    // See https://github.com/hypothesis/h/issues/634 for more info.
    // This is intended to be a temporary fix only.
    var details = {
      primaryPattern: 'https://notocol.tenet.res.in:8443/*',
      setting: 'allow'
    };
    chrome.contentSettings.cookies.set(details);
    chrome.contentSettings.images.set(details);
    chrome.contentSettings.javascript.set(details);

    browserExtension.install();
    notocolUtil.install();
  }

  function onUpdateAvailable() {
    chrome.runtime.reload();
  }

  

  chrome.runtime.onMessageExternal.addListener(
    function (request, sender, sendResponse) {
      
        if (request.reloadExtension)
            chrome.runtime.reload();
  });

  //Notocol Specifici Action Handlers
  chrome.extension.onMessage.addListener(function (request, sender, sendResponse) {
      if (request.greeting === "PageDetails") {
          if (!notocolUtil.userLoggedIn) {
              chrome.tabs.create({ url: "https://notocol.tenet.res.in:8443/User/Login" });
          }
          chrome.tabs.query({ active: true }, function (tabs) {
              if (!(tabs.length === 0)) {
                  var tabInfo = notocolUtil.gettabsData(tabs[0]) || {};
                  tabInfo.favIconUrl = tabs[0].faviconUrl;
                  tabInfo.title = tabs[0].title;
                  var userFolderTreeJson = notocolUtil.getUserFolderTreeJson();
                  if (typeof tabInfo != "undefined") {
                      sendResponse({ "tabInfo": tabInfo, "userFolderTreeJson" : userFolderTreeJson });
                  } else {
                      sendResponse({ status: false });
                  }
              } else {
                  sendResponse({ status: false });
              }
              
          });
          
      } else if (request.greeting === "PageDetailsUpdated") {
          
          if (typeof request.pageInfo.tabID != undefined && request.pageInfo.tabID > 0) {
              notocolUtil.settabsData({ id: request.pageInfo.tabID }, request.pageInfo);
          }
      } else if (request.greeting === "ToggleAnnotation") {
          chrome.tabs.query({ active: true }, function (tabs) {
              if (!(tabs.length === 0)) {
                  //Toggle Hypothesis Sidebar
                  browserExtension.onBrowserActionClicked(tabs[0]);
                  //browserExtension.sidebar.injectIntoTab(tabs[0]);
                  notocolUtil.updateAnnotatorStatus(tabs[0].id);
              }
          });
          sendResponse({ message: "Toggled Annotation" });
      } else if (request.greeting === "UserFoldersUpdated") {
          notocolUtil.setUserFolderTreeJson(request.userFolderTree);
      }
      return true;
  });
  notocolUtil.listen();

})();

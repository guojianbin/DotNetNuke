﻿var dnnJscriptVersion = "6.0.0"; if (typeof (Sys.Browser.Chrome) == "undefined") { Sys.Browser.Chrome = {}; if (navigator.userAgent.indexOf(" Chrome/") > -1) { Sys.Browser.agent = Sys.Browser.Chrome; Sys.Browser.version = parseFloat(navigator.userAgent.match(/Chrome\/(\d+\.\d+)/)[1]); Sys.Browser.name = "Chrome"; Sys.Browser.hasDebuggerStatement = true } } var DNN_HIGHLIGHT_COLOR = "#9999FF"; var COL_DELIMITER = String.fromCharCode(18); var ROW_DELIMITER = String.fromCharCode(17); var QUOTE_REPLACEMENT = String.fromCharCode(19); var KEY_LEFT_ARROW = 37; var KEY_UP_ARROW = 38; var KEY_RIGHT_ARROW = 39; var KEY_DOWN_ARROW = 40; var KEY_RETURN = 13; var KEY_ESCAPE = 27; Type.registerNamespace("dnn"); dnn.extend = function (a, b) { for (s in b) { a[s] = b[s] } return a }; dnn.extend(dnn, { apiversion: new Number("04.02"), pns: "", ns: "dnn", diagnostics: null, vars: null, dependencies: new Array(), isLoaded: false, delay: [], _delayedSet: null, getVars: function () { if (this.vars == null) { var a = dnn.dom.getById("__dnnVariable"); if (a.value.indexOf("`") == 0) { a.value = a.value.substring(1).replace(/`/g, '"') } if (a.value.indexOf("__scdoff") != -1) { COL_DELIMITER = "~|~"; ROW_DELIMITER = "~`~"; QUOTE_REPLACEMENT = "~!~" } if (a != null && a.value.length > 0) { this.vars = Sys.Serialization.JavaScriptSerializer.deserialize(a.value) } else { this.vars = [] } } return this.vars }, getVar: function (key, def) { if (this.getVars()[key] != null) { var re = eval("/" + QUOTE_REPLACEMENT + "/g"); return this.getVars()[key].replace(re, '"') } return def }, setVar: function (b, c) { if (this.vars == null) { this.getVars() } this.vars[b] = c; var a = dnn.dom.getById("__dnnVariable"); if (a == null) { a = dnn.dom.createElement("INPUT"); a.type = "hidden"; a.id = "__dnnVariable"; dnn.dom.appendChild(dnn.dom.getByTagName("body")[0], a) } if (dnn.isLoaded) { a.value = Sys.Serialization.JavaScriptSerializer.serialize(this.vars) } else { dnn._delayedSet = { key: b, val: c} } return true }, callPostBack: function (action) { var postBack = dnn.getVar("__dnn_postBack"); var data = ""; if (postBack.length > 0) { data += action; for (var i = 1; i < arguments.length; i++) { var aryParam = arguments[i].split("="); data += COL_DELIMITER + aryParam[0] + COL_DELIMITER + aryParam[1] } eval(postBack.replace("[DATA]", data)); return true } return false }, createDelegate: function (a, b) { return Function.createDelegate(a, b) }, doDelay: function (b, c, d, a) { if (this.delay[b] == null) { this.delay[b] = new dnn.delayObject(d, a, b); this.delay[b].num = window.setTimeout(dnn.createDelegate(this.delay[b], this.delay[b].complete), c) } }, cancelDelay: function (a) { if (this.delay[a] != null) { window.clearTimeout(this.delay[a].num); this.delay[a] = null } }, decodeHTML: function (a) { return a.toString().replace(/&amp;/g, "&").replace(/&lt;/g, "<").replace(/&gt;/g, ">").replace(/&quot;/g, '"') }, encode: function (a, c) { var b = a; if (encodeURIComponent) { b = encodeURIComponent(b) } else { b = escape(b) } if (c == false) { return b } return b.replace(/%/g, "%25") }, encodeHTML: function (a) { return a.toString().replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/'/g, "&apos;").replace(/\"/g, "&quot;") }, encodeJSON: function (a) { return a.toString().replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/'/g, "\u0027").replace(/\"/g, "&quot;").replace(/\\/g, "\\\\") }, evalJSON: function (a) { return Sys.Serialization.JavaScriptSerializer.deserialize(a) }, escapeForEval: function (a) { return a.replace(/\\/g, "\\\\").replace(/\'/g, "\\'").replace(/\r/g, "").replace(/\n/g, "\\n").replace(/\./, "\\.") }, getEnumByValue: function (a, b) { for (var c in a) { if (typeof (a[c]) == "number" && a[c] == b) { return c } } }, _onload: function () { dnn.isLoaded = true; if (dnn._delayedSet) { dnn.setVar(dnn._delayedSet.key, dnn._delayedSet.val) } } }); dnn.delayObject = function (c, a, b) { this.num = null; this.pfunc = c; this.context = a; this.type = b }; dnn.delayObject.prototype = { complete: function () { dnn.delay[this.type] = null; this.pfunc(this.context) } }; dnn.delayObject.registerClass("dnn.delayObject"); dnn.ScriptRequest = function (e, d, c) { this.ctl = null; this.xmlhttp = null; this.src = null; this.text = null; if (e != null && e.length > 0) { var b = dnn.dom.scriptFile(e); var a = dnn.getVar(b + ".resx", ""); if (a.length > 0) { this.src = a } else { this.src = e } } if (d != null && d.length > 0) { this.text = d } this.callBack = c; this.status = "init"; this.timeOut = 5000; this._xmlhttpStatusChangeDelegate = dnn.createDelegate(this, this.xmlhttpStatusChange); this._statusChangeDelegate = dnn.createDelegate(this, this.statusChange); this._completeDelegate = dnn.createDelegate(this, this.complete); this._reloadDelegate = dnn.createDelegate(this, this.reload) }; dnn.ScriptRequest.prototype = { load: function () { this.status = "loading"; this.ctl = document.createElement("script"); this.ctl.type = "text/javascript"; if (this.src != null) { if (dnn.dom.browser.isType(dnn.dom.browser.Safari)) { this.xmlhttp = new XMLHttpRequest(); this.xmlhttp.open("GET", this.src, true); this.xmlhttp.onreadystatechange = this._xmlhttpStatusChangeDelegate; this.xmlhttp.send(null); return } else { if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer)) { this.ctl.onreadystatechange = this._statusChangeDelegate } else { if (dnn.dom.browser.isType(dnn.dom.browser.Opera) == false) { this.ctl.onload = this._completeDelegate } } this.ctl.src = this.src } dnn.dom.scriptElements[this.src] = this.ctl } else { if (dnn.dom.browser.isType(dnn.dom.browser.Safari)) { this.ctl.innerHTML = dnn.encodeHTML(this.text) } else { this.ctl.text = this.text } } var a = dnn.dom.getByTagName("HEAD"); if (a) { if (dnn.dom.browser.isType(dnn.dom.browser.Opera) == false || this.src != null) { a[0].appendChild(this.ctl) } } else { alert("Cannot load dynamic script, no HEAD tag present.") } if (this.src == null || dnn.dom.browser.isType(dnn.dom.browser.Opera)) { this.complete() } else { if (this.timeOut) { dnn.doDelay("loadScript_" + this.src, this.timeOut, this._reloadDelegate, null) } } }, xmlhttpStatusChange: function () { if (this.xmlhttp.readyState != 4) { return } this.src = null; this.text = this.xmlhttp.responseText; this.load() }, statusChange: function () { if ((this.ctl.readyState == "loaded" || this.ctl.readyState == "complete") && this.status != "complete") { this.complete() } }, reload: function () { if (dnn.dom.scriptStatus(this.src) == "complete") { this.complete() } else { this.load() } }, complete: function () { dnn.cancelDelay("loadScript_" + this.src); this.status = "complete"; if (typeof (this.callBack) != "undefined") { this.callBack(this) } this.dispose() }, dispose: function () { this.callBack = null; if (this.ctl) { if (this.ctl.onreadystatechange) { this.ctl.onreadystatechange = new function () { } } else { if (this.ctl.onload) { this.ctl.onload = null } } this.ctl = null } this.xmlhttp = null; this._xmlhttpStatusChangeDelegate = null; this._statusChangeDelegate = null; this._completeDelegate = null; this._reloadDelegate = null } }; dnn.ScriptRequest.registerClass("dnn.ScriptRequest"); Type.registerNamespace("dnn.dom"); dnn.extend(dnn.dom, { pns: "dnn", ns: "dom", browser: null, __leakEvts: [], scripts: [], scriptElements: [], tweens: [], attachEvent: function (a, c, d) { if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer) == false) { var b = c.substring(2); a.addEventListener(b, function (e) { dnn.dom.event = new dnn.dom.eventObject(e, e.target); return d() }, false) } else { a.attachEvent(c, function () { dnn.dom.event = new dnn.dom.eventObject(window.event, window.event.srcElement); return d() }) } return true }, cursorPos: function (b) { if (b.value.length == 0) { return 0 } var h = -1; if (b.selectionStart) { h = b.selectionStart } else { if (b.createTextRange) { var f = window.document.selection.createRange(); var a = b.createTextRange(); if (a == null || f == null || ((f.text != "") && a.inRange(f) == false)) { return -1 } if (f.text == "") { if (a.boundingLeft == f.boundingLeft) { h = 0 } else { var d = b.tagName.toLowerCase(); if (d == "input") { var g = a.text; var c = 1; while (c < g.length) { a.findText(g.substring(c)); if (a.boundingLeft == f.boundingLeft) { break } c++ } } else { if (d == "textarea") { var c = b.value.length + 1; var e = document.selection.createRange().duplicate(); while (e.parentElement() == b && e.move("character", 1) == 1) { --c } if (c == b.value.length + 1) { c = -1 } } } h = c } } else { h = a.text.indexOf(f.text) } } } return h }, cancelCollapseElement: function (a) { dnn.cancelDelay(a.id + "col"); a.style.display = "none" }, collapseElement: function (b, c, d) { if (c == null) { c = 10 } b.style.overflow = "hidden"; var a = new Object(); a.num = c; a.ctl = b; a.pfunc = d; b.origHeight = b.offsetHeight; dnn.dom.__collapseElement(a) }, __collapseElement: function (a) { var c = a.num; var b = a.ctl; var d = b.origHeight / c; if (b.offsetHeight - (d * 2) > 0) { b.style.height = (b.offsetHeight - d).toString() + "px"; dnn.doDelay(b.id + "col", 10, dnn.dom.__collapseElement, a) } else { b.style.display = "none"; if (a.pfunc != null) { a.pfunc() } } }, cancelExpandElement: function (a) { dnn.cancelDelay(a.id + "exp"); a.style.overflow = ""; a.style.height = "" }, disableTextSelect: function (a) { if (typeof a.onselectstart != "undefined") { a.onselectstart = function () { return false } } else { if (typeof a.style.MozUserSelect != "undefined") { a.style.MozUserSelect = "none" } else { a.onmousedown = function () { return false } } } }, expandElement: function (b, c, d) { if (c == null) { c = 10 } if (b.style.display == "none" && b.origHeight == null) { b.style.display = ""; b.style.overflow = ""; b.origHeight = b.offsetHeight; b.style.overflow = "hidden"; b.style.height = "1px" } b.style.display = ""; var a = new Object(); a.num = c; a.ctl = b; a.pfunc = d; dnn.dom.__expandElement(a) }, __expandElement: function (a) { var c = a.num; var b = a.ctl; var d = b.origHeight / c; if (b.offsetHeight + d < b.origHeight) { b.style.height = (b.offsetHeight + d).toString() + "px"; dnn.doDelay(b.id + "exp", 10, dnn.dom.__expandElement, a) } else { b.style.overflow = ""; b.style.height = ""; if (a.pfunc != null) { a.pfunc() } } }, deleteCookie: function (a, c, b) { if (this.getCookie(a)) { this.setCookie(a, "", -1, c, b); return true } return false }, getAttr: function (b, a, c) { if (b.getAttribute == null) { return c } var d = b.getAttribute(a); if (d == null || d == "") { return c } else { return d } }, getById: function (b, a) { return $get(b, a) }, getByTagName: function (a, b) { if (b == null) { b = document } if (b.getElementsByTagName) { return b.getElementsByTagName(a) } else { if (b.all && b.all.tags) { return b.all.tags(a) } else { return null } } }, getParentByTagName: function (b, a) { var c = b.parentNode; a = a.toLowerCase(); while (c != null) { if (c.tagName && c.tagName.toLowerCase() == a) { return c } c = c.parentNode } return null }, getCookie: function (c) { var e = " " + document.cookie; var d = " " + c + "="; var b = null; var f = 0; var a = 0; if (e.length > 0) { f = e.indexOf(d); if (f != -1) { f += d.length; a = e.indexOf(";", f); if (a == -1) { a = e.length } b = unescape(e.substring(f, a)) } } return (b) }, getNonTextNode: function (a) { if (this.isNonTextNode(a)) { return a } while (a != null && this.isNonTextNode(a)) { a = this.getSibling(a, 1) } return a }, addSafeHandler: function (b, a, c, d) { b[a] = this.getObjMethRef(c, d); if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer)) { if (this.__leakEvts.length == 0) { dnn.dom.attachEvent(window, "onunload", dnn.dom.destroyHandlers) } this.__leakEvts[this.__leakEvts.length] = new dnn.dom.leakEvt(a, b, b[a]) } }, destroyHandlers: function () { var c = dnn.dom.__leakEvts.length - 1; for (var a = c; a >= 0; a--) { var b = dnn.dom.__leakEvts[a]; b.ctl.detachEvent(b.name, b.ptr); b.ctl[b.name] = null; dnn.dom.__leakEvts.length = dnn.dom.__leakEvts.length - 1 } }, getObjMethRef: function (b, a) { return (function (c) { c = c || window.event; return b[a](c, this) }) }, getSibling: function (a, c) { if (a != null && a.parentNode != null) { for (var b = 0; b < a.parentNode.childNodes.length; b++) { if (a.parentNode.childNodes[b].id == a.id) { if (a.parentNode.childNodes[b + c] != null) { return a.parentNode.childNodes[b + c] } } } } return null }, isNonTextNode: function (a) { return (a.nodeType != 3 && a.nodeType != 8) }, getScript: function (c) { if (this.scriptElements[c]) { return this.scriptElements[c] } var a = dnn.dom.getByTagName("SCRIPT"); for (var b = 0; b < a.length; b++) { if (a[b].src != null && a[b].src.indexOf(c) > -1) { this.scriptElements[c] = a[b]; return a[b] } } }, getScriptSrc: function (b) { var a = dnn.getVar(b + ".resx", ""); if (a.length > 0) { return a } return b }, getScriptPath: function () { var a = dnn.dom.getScript("dnn.js"); if (a) { return a.src.replace("dnn.js", "") } var b = dnn.getVar("__sp"); if (b) { return b } return "" }, scriptFile: function (b) { var a = b.split("/"); return a[a.length - 1] }, loadScript: function (e, d, b) { var c; if (e != null && e.length > 0) { c = this.scriptFile(e); if (this.scripts[c] != null) { return } } var a = new dnn.ScriptRequest(e, d, b); if (c) { this.scripts[c] = a } a.load(); return a }, loadScripts: function (a, b, c) { if (dnn.scripts == null) { var e = function (f, g, h) { return (function () { dnn.dom.loadScripts(f, g, h) }) }; dnn.dom.loadScript(dnn.dom.getScriptPath() + "dnn.scripts.js", null, e(a, b, c)); return } var d = new dnn.scripts.ScriptBatchRequest(a, b, c); d.load() }, scriptStatus: function (c) { var b = this.scriptFile(c); if (this.scripts[b]) { return this.scripts[b].status } var a = this.getScript(c); if (a != null) { return "complete" } else { return "" } }, setScriptLoaded: function (b) { var a = this.scriptFile(b); if (this.scripts[a] && dnn.dom.scripts[a].status != "complete") { dnn.dom.scripts[a].complete() } }, navigate: function (b, a) { if (a != null && a.length > 0) { if (a == "_blank") { window.open(b) } else { document.frames[a].location.href = b } } else { if (Sys.Browser.agent === Sys.Browser.InternetExplorer) { window.navigate(b) } else { window.location.href = b } } return false }, setCookie: function (a, e, g, d, c, b) { var f; if (g) { f = new Date(); f.setTime(f.getTime() + (g * 24 * 60 * 60 * 1000)) } document.cookie = a + "=" + escape(e) + ((f) ? "; expires=" + f.toGMTString() : "") + ((d) ? "; path=" + d : "") + ((c) ? "; domain=" + c : "") + ((b) ? "; secure" : ""); if (document.cookie.length > 0) { return true } }, getCurrentStyle: function (b, c) { var a = Sys.UI.DomElement._getCurrentStyle(b); if (a) { return a[c] } return "" }, getFormPostString: function (a) { var c = ""; if (a != null) { if (a.tagName && a.tagName.toLowerCase() == "form") { for (var b = 0; b < a.elements.length; b++) { c += this.getElementPostString(a.elements[b]) } } else { c = this.getElementPostString(a); for (var b = 0; b < a.childNodes.length; b++) { c += this.getFormPostString(a.childNodes[b]) } } } return c }, getElementPostString: function (a) { var c; if (a.tagName) { c = a.tagName.toLowerCase() } if (c == "input") { var d = a.type.toLowerCase(); if (d == "text" || d == "password" || d == "hidden" || ((d == "checkbox" || d == "radio") && a.checked)) { return a.name + "=" + dnn.encode(a.value, false) + "&" } } else { if (c == "select") { for (var b = 0; b < a.options.length; b++) { if (a.options[b].selected) { return a.name + "=" + dnn.encode(a.options[b].value, false) + "&" } } } else { if (c == "textarea") { return a.name + "=" + dnn.encode(a.value, false) + "&" } } } return "" }, appendChild: function (b, a) { return b.appendChild(a) }, removeChild: function (a) { return a.parentNode.removeChild(a) }, createElement: function (a) { return document.createElement(a.toLowerCase()) } }); dnn.dom.leakEvt = function (c, a, b) { this.name = c; this.ctl = a; this.ptr = b }; dnn.dom.leakEvt.registerClass("dnn.dom.leakEvt"); dnn.dom.eventObject = function (b, a) { this.object = b; this.srcElement = a }; dnn.dom.eventObject.registerClass("dnn.dom.eventObject"); dnn.dom.browserObject = function () { this.InternetExplorer = "ie"; this.Netscape = "ns"; this.Mozilla = "mo"; this.Opera = "op"; this.Safari = "safari"; this.Konqueror = "kq"; this.MacIE = "macie"; var b; var d = navigator.userAgent.toLowerCase(); if (d.indexOf("konqueror") != -1) { b = this.Konqueror } else { if (d.indexOf("msie") != -1 && d.indexOf("mac") != -1) { b = this.MacIE } else { if (Sys.Browser.agent === Sys.Browser.InternetExplorer) { b = this.InternetExplorer } else { if (Sys.Browser.agent === Sys.Browser.FireFox) { b = this.Mozilla } else { if (Sys.Browser.agent === Sys.Browser.Safari) { b = this.Safari } else { if (Sys.Browser.agent === Sys.Browser.Opera) { b = this.Opera } else { b = this.Mozilla } } } } } } this.type = b; this.version = Sys.Browser.version; var c = navigator.userAgent.toLowerCase(); if (this.type == this.InternetExplorer) { var a = navigator.appVersion.split("MSIE"); this.version = parseFloat(a[1]) } if (this.type == this.Netscape) { var a = c.split("netscape"); this.version = parseFloat(a[1].split("/")[1]) } }; dnn.dom.browserObject.prototype = { toString: function () { return this.type + " " + this.version }, isType: function () { for (var a = 0; a < arguments.length; a++) { if (dnn.dom.browser.type == arguments[a]) { return true } } return false } }; dnn.dom.browserObject.registerClass("dnn.dom.browserObject"); dnn.dom.browser = new dnn.dom.browserObject(); if (typeof ($) == "undefined") { eval("function $() {var ary = new Array(); for (var i=0; i<arguments.length; i++) {var arg = arguments[i]; var ctl; if (typeof arg == 'string') ctl = dnn.dom.getById(arg); else ctl = arg; if (ctl != null && typeof(Element) != 'undefined' && typeof(Element.extend) != 'undefined') Element.extend(ctl); if (arguments.length == 1) return ctl; ary[ary.length] = ctl;} return ary;}") } try { document.execCommand("BackgroundImageCache", false, true) } catch (err) { } Sys.Application.add_load(dnn._onload);
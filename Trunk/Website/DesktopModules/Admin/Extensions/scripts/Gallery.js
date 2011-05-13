﻿
//  touch base with dang regarding layout & design
//  add support for minDnnVersion filtering
//  finalized support for TagCloud/Type filtering



Gallery.isDebug = !(typeof console == 'undefined');
Gallery.log = function () {
    if (Gallery.isDebug) {
        for (var i in arguments) console.log(arguments[i]);
    }
}

function Gallery(params) {

    //defaults
    var options = {
        rowId: 0
        , index: 1
        , pageSize: 10
        , sortBy: "extensionName"
        , sortDir: "asc"
        , animationSpeed: "slow"
        , pageIdx: 1
        , pageSze: 10
        , smoothScrolling: true
        , extensions: new Object()
        , action: "filter"
        , extensionFilter: "module"
        , tagFilter: ""
        , tagFilterName: ""
        , ownerFilter: ""
        , tags: new Object()
        , loadTags: true
        , pagedExtensions: new Object()
        , protocol: ('https:' == location.protocol ? 'https://' : 'http://')
        , host: "appgallery.dotnetnuke.com"
        , ServiceRoot: "/AppGalleryService.svc"
        , ExtensionServiceName: "/Extensions"
        , TagsServiceName: "/Tags"
        , TagCloudServiceName: "/GetTagCloudData"
        , extensionDetailDialog: $("#extensionDetail").dialog(this.DefaultDialogOptions)
        , loading: $("#loading")
        , NameTextASC: "Name: A-Z"
        , NameTextDESC: "Name: Z-A"
        , PriceextASC: "Price: High to Low"
        , PriceTextDESC: "Price: Low to High"
        , TagCount: 50
        , CacheTimeoutMinutes: 1440
        , searchFilters: $("#searchFilters")
        , tagLabel: "Tag"
        , searchLabel: "Search"
        , vendorLabel: "Vendor"
        , typeLabel: "Type"
        , noneLabel: "None"
        , orderLabel: "Order:"
        , errorLabel: "Error..."
        , loadingLabel: "Loading..."
        , badCharacters: /[^a-zA-Z 0-9]+/g
        , searchText: ""
        , versionImages: ["DNN-00.png", "DNN-01.png", "DNN-02.png", "DNN-03.png", "DNN-04.png", "DNN-05.png", "DNN-06.png", ]
    };
    //extend defaults with ctor params
    if (params) {
        $.extend(options, params);
    }

    //load up our object with default options
    for (var i in options) {
        if (options.hasOwnProperty(i)) this[i] = options[i];
    }

    this.extensionList = $("#extensionList");
    //setup smooth scrolling pager
    if (this.smoothScrolling) {
        var s = new Scroller(100, false, function (scroller) {
            _gallery.index++;
            _gallery.action = "page";
            _gallery.Search();
        }).watch();
    }

    //load up our urls
    this.ExtensionsUrl = this.getServiceUrl(this.ExtensionServiceName);
    this.TagCloudUrl = this.getServiceUrl(this.TagCloudServiceName);
    this.TagsUrl = this.getServiceUrl(this.TagsServiceName);

    this.Cache = new Cache("_Gallery_", this.CacheTimeoutMinutes);

    //bind to our document events


    $("#typeDDL").change(function (event) {
        var e = event || window.event;
        _gallery.FilterGallery(e, this);
        return false;
    });

    $("#tag-list").click(function (e) {
        e = e || window.event;
        _gallery.TagFilterGallery(e, this);
        return false;
    });

    $("#search-reset").click(function (e) {
        e = e || window.event;
        $('#searchText').val('');
        _gallery.tagFilter = '';
        _gallery.tagFilterName = '';
        _gallery.ownerFilter = '';
        _gallery.sortBy = 'extensionName';
        _gallery.sortDir = 'asc';
        _gallery.extensionFilter = 'module';
        $("#typeDDL").val('module');
        _gallery.SearchGallery('');
        return false;
    });

    $("#searchText").change(function (e) {
        e = e || window.event;
        _gallery.SearchGallery($('#searchText').val());
        return false;
    });
    $(document).keydown(function (e) {
        e = e || window.event;
        if (e.which == 13) {
            window.stop();
            e.stopPropagation();
            e.preventDefault();
            _gallery.SearchGallery($('#searchText').val());
        }
    });
    $("#searchText").keyup(function (e) {
        e = e || window.event;
        if (e.which == 27) {
            $("#search-reset").click();
        }
        return false;
    });
    $("#search-go").click(function (e) {
        e = e || window.event;
        _gallery.SearchGallery($('#searchText').val());
        return false;
    });

    $("#NameSorter").click(function (e) {
        e = e || window.event;
        _gallery.SortExtensions('extensionName');
        return false;
    });

    $("#PriceSorter").click(function (e) {
        e = e || window.event;
        _gallery.SortExtensions('price');
        return false;
    });
}

Gallery.tagSort = function (a, b) {
    return (0.5 - Math.random());
}
Gallery.validTag = function (tag) {
    return (tag && tag.tagName && (tag.tagName.indexOf("DotNetNuke") < 0) && (tag.TagCount > 0));
}
Gallery.prototype.resolveTags = function (msg) {
    var fixed = new Array();
    var max = msg.d.length - 1;
    var biggest = 0;
    var maxFont = 250;
    var minFont = 75;
    var x = 0;
    var item;

    //first pass to get biggest
    for (x = max; x >= 0; x--) {
        item = msg.d[x];
        if (Gallery.validTag(item)) {
            if (item.TagCount > biggest) biggest = item.TagCount;
            fixed.push(item);
        }
    }

    //second pass to set sizes
    max = fixed.length;
    for (x in fixed) {
        item = fixed[x];
        item.fontSize = ((item.TagCount / biggest) * maxFont).toFixed(2);
        if (item.fontSize < minFont) item.fontSize = minFont;
    }


    this.loadTags = false;
    this.tags = fixed.sort(Gallery.tagSort);
    this.Cache.setItem("tags_" + this.extensionFilter, _gallery.tags);
    this.showTags();
    this.hideLoading();

}
Gallery.gotTags = function (msg) {
    _gallery.resolveTags(msg);
}
Gallery.prototype.resolveImage = function (img) {
    return this.imgRoot + img;
}

Gallery.prototype.showLoading = function (a) {
    this.reposition();
    this.loading.css("background-color", "");
    this.loading.text(this.loadingLabel);
    this.loading.show();
}
Gallery.prototype.hideLoading = function (a) {
    this.loading.hide();
}
Gallery.prototype.errorLoading = function (a) {
    this.loading.css("background-color", "red");
    this.loading.text(this.errorText);
    this.loading.attr("title", a[0].statusText);
    this.loading.attr("alt", a[0].statusText);
}

Gallery.prototype.reposition = function () {
    var wnd = $(window);
    this.loading.css("top", wnd.scrollTop());
    this.loading.css("left", (wnd.width() / 2) - (this.loading.width() / 2));
}

Gallery.prototype.SortExtensions = function (fld, order) {
    this.index = 1;
    this.action = "sort";
    if (!order)
        this.ToggleSort(fld);
    else {
        this.sortBy = fld;
        this.sortDir = order;
    }

    if (this.sortBy && this.sortDir) {
        var NameSorter = $("#NameSorter");
        var PriceSorter = $("#PriceSorter");
        if (this.sortBy == "extensionName") {
            if (this.sortDir == "asc")
                NameSorter.text(this.NameTextDESC);
            else
                NameSorter.text(this.NameTextASC);
        } else if (this.sortBy == "price") {
            if (this.sortDir == "asc")
                PriceSorter.text(this.PriceTextDESC);
            else
                PriceSorter.text(this.PriceTextASC);
        }
    }
    return this.Search();
}
Gallery.prototype.SearchGallery = function (search) {
    this.action = "search";
    if (search) {
        this.searchText = search;
    } else {
        this.searchText = '';
    }
    this.index = 1;
    return this.Search();
}

Gallery.prototype.OwnerFilterGallery = function (owner) {
    this.action = "filter";
    if (owner)
        this.ownerFilter = owner;

    this.index = 1;

    return this.Search();
}

Gallery.prototype.TagFilterGallery = function (e, caller) {
    e = e || window.event;
    var target = $((e.srcElement || e.target));
    var filter = target.attr('value');
    this.action = "filter";
    if (filter) {
        this.tagFilter = filter;
        this.tagFilterName = target.html();
    }
    this.index = 1;
    return this.Search();
}

Gallery.prototype.FilterGallery = function (e, ddl) {
    var filter = $((e.srcElement || e.target)).attr('value');

    this.action = "filter";
    if (filter)
        this.extensionFilter = filter;

    this.getTags();
    this.index = 1;
    return this.Search();
}

Gallery.prototype.Search = function () {

    this.getExtensions();
    return this;
}


Gallery.prototype.ToggleSort = function (field) {
    this.sortBy = field;

    if (!this.sortDir || this.sortDir === "" || this.sortDir == "desc") {
        this.sortDir = 'asc';
    } else {
        this.sortDir = 'desc';
    }
}

Gallery.prototype.getServiceUrl = function (ServiceName) {
    return this.protocol + this.host + this.ServiceRoot + ServiceName;
}

Gallery.prototype.getExtensions = function (callback) {

    var filterDesc = this.tagLabel + " ";
    var url = ""
    var prefix = "";
    var skip = (this.index - 1) * this.pageSize;
    var hasCriteria = (skip > 0);

    //basic sanitization    
    this.searchText = this.searchText.replace(this.badCharacters, "");
    this.ownerFilter = this.ownerFilter.replace(this.badCharacters, "");


    if (this.tagFilter && this.tagFilter !== "") {
        url = this.TagsUrl + "(" + this.tagFilter + ")/ExtensionTags/?$expand=Extension";
        prefix = "Extension/";
        filterDesc = filterDesc + this.tagFilterName;
        hasCriteria = true;
    } else {
        url = this.ExtensionsUrl + "?";
        filterDesc = filterDesc + this.noneLabel;
    }
    filterDesc = filterDesc + ", " + this.searchLabel + " ";
    url += "&$inlinecount=allpages" + 			// get total number of records
	    	    			"&$skip=" + skip + 	// skip to first record of page
	    	    			"&$top=" + this.pageSize;

    if (this.searchText && this.searchText !== "") {
        url = url + "&$filter=" + encodeURIComponent("(substringof('" + this.searchText + "', " + prefix + "extensionName) eq true or substringof('" + this.searchText + "', " + prefix + "Description) eq true or substringof('" + this.searchText + "', " + prefix + "Title) eq true)");
        filterDesc = filterDesc + this.searchText;
        hasCriteria = true;
    } else {
        filterDesc = filterDesc + this.noneLabel;
    }



    filterDesc = filterDesc + ", " + this.typeLabel + " ";

    if (this.extensionFilter && this.extensionFilter !== "" && this.extensionFilter !== "all") {
        if (this.extensionFilter == "module") {
            filterDesc = filterDesc + "Module";
        } else {
            hasCriteria = true;
            filterDesc = filterDesc + "Skin";
        }
        if (url.indexOf("$filter") < 0)
            url = url + "&$filter=";
        else
            url = url + "and ";

        url = url + "" + prefix + "extensionType eq '" + this.extensionFilter + "'";
    } else {
        hasCriteria = true;
        filterDesc = filterDesc + "All";
    }

    filterDesc = filterDesc + ", " + this.vendorLabel + " ";

    if (this.ownerFilter && this.ownerFilter !== "") {
        filterDesc = filterDesc + this.ownerFilter;
        if (url.indexOf("$filter") < 0)
            url = url + "&$filter=";
        else
            url = url + "and ";

        url = url + encodeURIComponent("" + prefix + "ownerName eq '" + this.ownerFilter + "'");
        hasCriteria = true;
    } else {
        filterDesc = filterDesc + this.noneLabel;
    }

    if (this.DataBaseVersion && this.DataBaseVersion !== "") {
        if (url.indexOf("$filter") < 0)
            url = url + "&$filter=";
        else
            url = url + "and ";

        url = url + encodeURIComponent("" + prefix + "MinDnnVersion lt '" + this.DataBaseVersion + "'");
    } else {
        filterDesc = filterDesc + this.noneLabel;
    }


    filterDesc = filterDesc + ", " + this.orderLabel + " ";


    if (this.sortBy !== "") {

        url = url + "&$orderby=" + encodeURIComponent(prefix + "" + this.sortBy + " " + this.sortDir);

        if (this.sortBy == "extensionName") {
            if (this.sortDir == "asc") {
                filterDesc = filterDesc + this.NameTextASC;
            } else {
                hasCriteria = true;
                filterDesc = filterDesc + this.NameTextDESC;
            }
        } else {
            hasCriteria = true;
            if (this.sortDir == "asc") {
                filterDesc = filterDesc + this.PriceextASC;
            } else {
                filterDesc = filterDesc + this.PriceTextDESC;
            }
        }
    } else {
        filterDesc = filterDesc + this.noneLabel;
    }


    this.searchFilters.text(filterDesc);
    url = url + "&$callback=Gallery.gotExtensions&$format=json";

    if (!_gallery.extensions.d || !hasCriteria) {
        var exts = this.Cache.getItem("__FIRSTLOAD");
        if (exts) {
            Gallery.gotExtensions(exts);
            return;
        }
    }
    Gallery.log(url);
    this.showLoading();
    this.eXHR = $.ajax({
        type: "GET",
        crossDomain: true,
        jsonp: false,
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).error(function () {
        _gallery.errorLoading(arguments);
    });
    this.eXHR.url = url;
}

Gallery.gotExtensions = function () {

    var msg = arguments[0];

    var g = _gallery;

    ////TODO: MOCK DATA  --- REMOVED FOR PROD    
    if (msg && msg.d && msg.d.results) {
        for (var i in msg.d.results) {
            var item = msg.d.results[i];
            item.catalog = g.catalogs[Math.random.range(0, 1)];
        }
    }



    if (!_gallery.Cache.hasItem("__FIRSTLOAD")) _gallery.Cache.setItem("__FIRSTLOAD", msg);

    if (msg.d.results.length > 0 && (typeof msg.d.results[0].Extension != 'undefined')) {
        for (var j in msg.d.results) {
            for (var i in msg.d.results[j].Extension) {
                if (msg.d.results[j].Extension.hasOwnProperty(i)) msg.d.results[j][i] = msg.d.results[j].Extension[i];
            }
        }
    }

    _gallery.pagedExtensions = msg.d.results;
    if (_gallery.extensions && _gallery.extensions.d && _gallery.extensions.d.results && !(_gallery.action == "search" || _gallery.action == "filter" || _gallery.action == "sort"))
        _gallery.extensions.d.results = _gallery.extensions.d.results.concat(msg.d.results);
    else
        _gallery.extensions = msg;


    _gallery.showExtensions(function () {
        if (_gallery.loadTags) {
            _gallery.getTags();
        }
    });
    _gallery.hideLoading();
}

Gallery.prototype.getTags = function (callback) {

    var max = (this.TagCount ? this.TagCount : 15);
    var url = this.TagCloudUrl + "?tagcount=" + max;
    //url=url + "&$filter=Extension/extensionType eq '" + this.extensionFilter + "'";
    url = url + "&$callback=Gallery.gotTags&$format=json";

    var tags = this.Cache.getItem("tags_" + this.extensionFilter);
    if (tags) {
        _gallery.tags = tags.sort(Gallery.tagSort);
        _gallery.loadTags = false;
        _gallery.showTags();
        return;
    }
    this.showLoading();

    Gallery.log(url);

    this.tagXHR = $.ajax({
        type: "GET",
        crossDomain: true,
        jsonp: false,
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    }).error(function () {
        _gallery.errorLoading(arguments);
    });
}


Gallery.prototype.showTags = function (callback) {
    // show tags in template
    var taglist = $("#tag-list");
    var tagTmpl = $("#tag-tmpl");

    taglist.empty();

    if (this.tags) {
        tagTmpl.tmpl(this.tags).appendTo(taglist);
        taglist.fadeIn(this.animationSpeed);
    } else {
        taglist.fadeOut(this.animationSpeed);
    }
    if (callback) callback(this);
}


Gallery.prototype.showExtensions = function (callback) {
    this.pageCount = Math.ceil(this.extensions.d.__count / this.pageSize);

    if (!this.smoothScrolling) {
        this.extensionList.empty();
    }
    if (this.action == "search" || this.action == "filter" || this.action == "sort") this.extensionList.empty();


    if (this.pagedExtensions.length > 0) {
        $("#eTmpl").tmpl(this.pagedExtensions).find("div").appendTo(this.extensionList).fadeIn(this.animationSpeed);
    }
    this.pagedExtensions = [];
    if (callback) callback(this);
}

Gallery.prototype.getExtensionById = function (extensionID) {
    if (!this.extensions || !this.extensions.d) return;
    var list = this.extensions.d.results;
    for (var x = list.length; x--; x >= 0) {
        if (list[x].extensionID == extensionID) return list[x];
    }
    return;
}
Gallery.prototype.FormatCurrency = function (num) {
    num = num.toString().replace(/\$|\,/g, '');
    if (isNaN(num))
        num = "0";
    sign = (num == (num = Math.abs(num)));
    num = Math.floor(num * 100 + 0.50000000001);
    cents = num % 100;
    num = Math.floor(num / 100).toString();
    if (cents < 10)
        cents = "0" + cents;
    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
        num = num.substring(0, num.length - (4 * i + 3)) + ',' + num.substring(num.length - (4 * i + 3));
    return (((sign) ? '' : '-') + '$' + num + '.' + cents);
}



Gallery.prototype.DefaultDialogOptions = {
    modal: true,
    autoOpen: false,
    width: 800,
    height: 600,
    resizable: true,
    closeOnEscape: true
}


Gallery.prototype.ShowDetails = function (extensionID) {

    var ext = this.getExtensionById(extensionID);
    if (ext) {
        var extensionDetailInner = $("#extensionDetailInner");
        extensionDetailInner.empty();
        $("#extDetailTmpl").tmpl(ext).appendTo(extensionDetailInner);

        $("#extensionDetail-tabs").tabs();

        this.extensionDetailDialog.dialog({ title: ext.extensionName });
        this.extensionDetailDialog.dialog('open');


    }
    return false;
}


Cache = function (Scope, TimeoutInMinutes, StorageType, ExpireCallback) {
    this.StorageType = (StorageType || "localStorage");
    if (this.StorageType != "localStorage" || this.StorageType != "sessionStorage" || this.StorageType != "globalStorage") this.StorageType = "localStorage";
    this.Scope = Scope || "";
    this.TimeoutInMinutes = TimeoutInMinutes;
    this.expireCallback = ExpireCallback;
    this.loadStore();
    Cache.isEnabled = (typeof this.store != 'undefined') && (typeof JSON != 'undefined');
    if (Cache.isEnabled) {
        if (this.TimeoutInMinutes != 'undefined') {
            this.cacheExpire = window.setInterval(Cache.ClearInterval, (TimeoutInMinutes * 60000), this);
        }
    }
    return this;
}
Cache.isEnabled = false;
Cache.prototype.loadStore = function () {
    switch (this.StorageType) {
        case "globalStorage":
            try {
                if (window.globalStorage) {
                    this.store = window.globalStorage[window.location.hostname];
                }
            } catch (E4) { }
            break;
        case "sessionStorage":
            try {
                if (window.sessionStorage) {
                    this.store = window.sessionStorage;
                }
            } catch (E3) { }
            break;
        default:
            try {
                if (window.localStorage) {
                    this.store = window.localStorage;
                }
            } catch (E3) { }
            break;
    }
    return this;
}
Cache.ClearInterval = function (source) {
    if (typeof source.TimeoutInMinutes != 'undefined') {
        if (source.hasItem(source.Scope + "_expire")) {
            var exp = source.getItem(source.Scope + "_expire");
            var mins = ((new Date()) - Date.parse(exp)) / 60000;
            if (mins > source.TimeoutInMinutes) {
                source.EmptyCache();
                if (typeof source.expireCallback != 'undefined') source.expireCallback(i);
            }
        } else {
            source.setItem(source.Scope + "_expire", new Date());
        }
    }
    return this;
}
Cache.prototype.EmptyCache = function () {

    var max, i, X;
    try {
        for (i in this.store) {
            if (this.store.hasOwnProperty(i)) {
                if (i.substr(0, this.Scope.length) == this.Scope) {
                    this.store.removeItem(i);
                }
            }
        }
    } catch (E) { //\go figure, FF will throw on the above, but chrome it works
        try {
            max = this.store.length - 1;
            for (x = max; x >= 0; x--) {
                i = this.store[x];
                if (this.store.hasOwnProperty(i)) {
                    if (i.substr(0, this.Scope.length) == this.Scope) {
                        this.store.removeItem(i);
                    }
                }
            }
        } catch (EFF) { this.store.clear(); } //worst case
    }
    return this;
}
Cache.prototype.hasItem = function (key) {
    var d;
    if (Cache.isEnabled) {
        d = this.store.getItem(this.Scope + key);
        return (d && (d !== null) && !(typeof d === 'undefined'));
    }
    return false;
}
Cache.prototype.getItem = function (key) {
    var d, x;
    if (Cache.isEnabled) {
        d = this.store.getItem(this.Scope + key);
        try {
            x = $.parseJSON(d);
        } catch (e) {
            this.store.removeItem(this.Scope + key);
        }
    }
    return x;
}

Cache.prototype.setItem = function (key, value) {
    if (Cache.isEnabled) {
        this.store.setItem(this.Scope + key, JSON.stringify(value));
    }
    return this;
}



Scroller = function (maxPage, loadScroll, scrollcallback) {
    this.page = 1;
    this.maxPage = (maxPage) ? maxPage : 100;
    this.loadScroll = (typeof loadScroll != 'undefined') ? loadScroll : false;
    this.scrollcallback = scrollcallback;
    if (this.loadScroll) this.loadScroller();
}

Scroller.prototype.handleScroll = function () {
    this.page++;

    if (this.scrollcallback) this.scrollcallback(this);
    if (this.page >= this.maxPage) this.unwatch();
}

Scroller.prototype.loadScroller = function () {
    var more = true;
    while (more) {
        more = ($(window).scrollTop() >= ($(document).height() - $(window).height()));
        if (more) this.handleScroll();
        if (this.page >= this.maxPage) more = false;
    }
}

Scroller.prototype.watch = function () {
    window.Scroller = this;
    //var root = $(document);
    var root = $(window);
    if (!root.scroll) root = $(document);
    root.scroll(function () {
        var s = window.Scroller;
        if ($(window).scrollTop() >= ($(document).height() - $(window).height())) {
            s.handleScroll();
        }
    });
}
Scroller.prototype.unwatch = function () {
    $(document).unbind("scroll");
}
Math.random.range = function (min, max, inclusive) {
    if (typeof inclusive !== 'undefined' || !inclusive) min = min - 1; max = max + 1;
    return Math.floor(max + (1 + min - max) * Math.random());
}
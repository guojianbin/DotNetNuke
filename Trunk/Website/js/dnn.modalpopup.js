﻿(function(a,b){dnnModal={load:function(){try{if(parent.location.href!==undefined){var f=parent;var e=f.parent;if(typeof(e.$find)!="undefined"){if(location.href.indexOf("popUp")==-1||f.location.href.indexOf("popUp")>-1){f.location.href=location.href;var c=f.$("#iPopUp");if(c.dialog("isOpen")===true){c.dialog("option",{close:function(g,h){}}).dialog("close").remove()}}else{f.$("#iPopUp").dialog({title:document.title})}}}}catch(d){return false}},show:function(c,k,h,d){var f=b("#iPopUp");if(f.length==0){f=b('<iframe id="iPopUp" src="about:blank" scrolling="auto" frameborder="0" ></iframe>');b(document).find("html").css("overflow","hidden");b(document).append(f)}var i=parent;f.dialog({modal:true,autoOpen:true,dialogClass:"dnnFormPopup",position:"center",minWidth:d,minHeight:h,maxWidth:1920,maxHeight:1080,resizable:true,closeOnEscape:true,zIndex:100000,close:function(l,m){i.location.reload();b(this).remove()}}).width(d-11).height(h-11);var g=b('<a href="#" class="dnnToggleMax"><span>Max</span></a>');b(".ui-dialog-title").after(g);g.click(function(p){p.preventDefault();var q=b(a),o=b(this),l,n,m;if(f.data("isMaximized")){l=f.data("height");n=f.data("width");m=f.data("position");f.data("isMaximized",false)}else{f.data("height",f.dialog("option","minHeight")).data("width",f.dialog("option","minWidth")).data("position",f.dialog("option","position"));l=q.height()-11;n=q.width()-11;m=[0,0];f.data("isMaximized",true)}o.toggleClass("ui-dialog-titlebar-max");f.dialog({height:l,width:n});f.dialog({position:"center"})});var e=function(){var l=b('<div class="dnnLoading"></div>');l.css({width:f.width(),height:f.height()});f.before(l);f.hide()};var j=function(){f.prev(".dnnLoading").remove();f.show()};setTimeout(function(){e()},0);f[0].src=c;f.bind("load",function(){j()});if(k.toString()=="true"){return false}}};dnnModal.load()}(this,jQuery));
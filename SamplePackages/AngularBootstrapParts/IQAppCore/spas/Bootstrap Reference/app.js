var iqAppPartModule = angular.module('iqAppPartModule', []);

(function ($) {
    //Resize the iframe if it is in a CEWP instead of a Page Viewer
    var container = $(window.frameElement.parentElement).attr('webpartid');
    if(!container){
        container = $(window.frameElement.parentElement.parentElement).attr('webpartid');
        if(!container) return;
        $(window.frameElement).height($(window.frameElement.parentElement.parentElement).height() - 10);
    }
})(jQuery);

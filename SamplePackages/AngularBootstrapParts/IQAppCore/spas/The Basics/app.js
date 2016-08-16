//This is the outer shell of your app,
//It must be named iqAppPartModule
var iqAppPartModule = angular.module('iqAppPartModule', []);
var iqAppPartController = iqAppPartController || {};

//The demo controller uses angular's $interval service for the timer and the sharePointService context wrapper service
iqAppPartController.IQAppPart = function ($scope, sharePointService) {
    //Resize the outer containers if you need to
    var container = $(window.frameElement.parentElement).attr('webpartid');
    if (!container) {
        //If in a CEWP
        $(window.frameElement.parentElement.parentElement).height(80);
        $(window.frameElement).height(70);
    } else {
        //in a Page Viewer
        window.frameElement.style.height = '200px';
        window.frameElement.parentElement.style.height = '200px';
    }

    //Fail invisibly if there is no config 
    var config = window.iqPart.CurrentConfiguration;
    if (!config) return;
};

iqAppPartModule.controller('IQAppPart', ['$scope', '$interval', 'SharePointService', iqAppPartController.IQAppPart]);



//This is the outer shell of your app,
//It must be named iqAppPartModule
var iqAppPartModule = angular.module('iqAppPartModule', []);
var iqAppPartController = iqAppPartController || {};

//The demo controller uses angular's $interval service for the timer and the sharePointService context wrapper service
iqAppPartController.IQAppPart = function ($scope, $interval, sharePointService) {
    //Resize the outer containers if you need to
    $scope.setContainerHeight(70);

    //Fail invisibly if there is no config 
    var config = window.iqPart.CurrentConfiguration;
    if (!config) return;

    //Set the desired defaults for the custom part
    //In this case SelectedDemo is the custom property and 'hello' is the default
    if (!config.SelectedDemo) {
        config.SelectedDemo = 'hello';
        $scope.saveConfig();
    }

    //Do custom stuff!
    $scope.Demo = config.SelectedDemo;

    if ($scope.Demo === 'hello') {
        //Use the sharePointService to load the user's name
        var ctx = $scope.ctx || SP.ClientContext.get_current();
        var user = ctx.get_web().get_currentUser();
        ctx.load(user);

        sharePointService.executeQuery($scope.ctx)
            .then(function (profile) {
                $scope.UserName = user.get_title();
            },
            function (error) {
                alert(error.get_message());
            });
    }
    else {
        //Update the time and then set up a timer to refresh every second using the $interval service
        updateTime();
        $scope.timer = $interval(updateTime, 1000);
    }

    function updateTime() {
        $scope.Time = (new Date()).toJSON();
    };
};

//This is the controller for the configuration view
iqAppPartController.IQAppPartConfig = function ($scope) {
    //Resize the outer containers
    var container = $(window.frameElement.parentElement).attr('webpartid');
    if (!container) {
        //If in a CEWP
        $(window.frameElement.parentElement.parentElement).height(310);
        $(window.frameElement).height(300);
    } else {
        //in a Page Viewer
        window.frameElement.style.height = '300px';
        window.frameElement.parentElement.style.height = '300px';
    }

    var p = window.iqPart;
    $scope.Demos = ['hello', 'timer'];
    $scope.config = {};
    $scope.config.SelectedDemo = !p.CurrentConfiguration.SelectedDemo ? 'hello' : p.CurrentConfiguration.SelectedDemo;
    $scope.ConfigTemplateUrl = p.webServerRelativeUrl + '/IQAppCore/SPAs/' + p.CurrentConfiguration.Name + '/config.html';
    $scope.CurrentConfiguration = p.CurrentConfiguration;

    $scope.save = function () {
        //Add whatever custom properties you need and call $scope.saveConfig();
        p.CurrentConfiguration.SelectedDemo = $scope.config.SelectedDemo;
        $scope.saveConfig();
    };
};

iqAppPartModule.controller('IQAppPart', ['$scope', '$interval', 'SharePointService', iqAppPartController.IQAppPart]);
iqAppPartModule.controller('IQAppPartConfig', ['$scope', 'SharePointService', iqAppPartController.IQAppPartConfig]);


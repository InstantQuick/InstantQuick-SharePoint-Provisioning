(function ($) {
    //Adds a style sheet to the current document head
    function dLink(n) {
        var t = document.createElement("link"); t.type = "text/css"; t.rel = "stylesheet"; t.href = n; document.getElementsByTagName("head")[0].appendChild(t)
    };

    //Collection of query parameters
    var urlParams = {};
    (function () {
        var e,
        a = /\+/g, // Regex for replacing addition symbol with a space
            r = /([^&=]+)=?([^&]*)/g,
            d = function (s) {
                return decodeURIComponent(s.replace(a, " "));
            },
            q = window.location.search.substring(1);

        while (e = r.exec(q))
            urlParams[d(e[1])] = d(e[2]);
    })();

    $(function () {
        //At document ready all static scripts (such as angular and sp.js) should be loaded, this script should be last (See spabody.aspx.)

        //Defaults and config
        var p = window.iqPart = window.iqPart || {};
        var parentFrameId, partConfigurationKey, parentPathParts;
        var configTemplate = 'var p=window.iqPart=window.iqPart||{};p.Configurations={{config}};';
        p.webServerRelativeUrl = window.parent._spPageContextInfo.webServerRelativeUrl !== '/' ? window.parent._spPageContextInfo.webServerRelativeUrl : '';
        p.InEditMode = !!window.parent.SP.Ribbon && window.parent.SP.Ribbon.PageState.Handlers.isInEditMode();
        p.urlParams = urlParams;
        if (p.InEditMode) {
            //Hide the SharePoint edit mode border
            $(window.frameElement.parentElement).css('border', 'none');
        }

        //Declare the Angular module
        p.module = angular.module('iqPart', []);

        //Load and wait for the config
        GetCurrentPartConfig(ConfigLoaded);

        function ConfigLoaded() {
            //Bail if this isn't inside a frame!
            if (!parentFrameId) return;

            //Specify the default app if no unique config for this web part
            //then bootstrap AngularJS and the run app
            if (p.CurrentConfiguration.Name === p.Configurations.default.Name) {
                p.module.rootTemplate = 'default.html';
                //Finish inititializing, then boostrap
                setTimeout(boostrap, 1);
            }
                //Specify the custom app and load its dependencies
            else {
                p.module.rootUrl = p.webServerRelativeUrl + '/IQAppCore/SPAs/' + p.CurrentConfiguration.Name + '/';

                //Show either the app or its configuration editor
                if (!p.InEditMode) {
                    p.module.rootTemplate = p.module.rootUrl + p.CurrentConfiguration.RootTemplate;
                }
                else {
                    p.module.rootTemplate = p.module.rootUrl + 'config.html';
                }

                //Load scripts and style sheets
                if (!!p.CurrentConfiguration.Styles && Array.isArray(p.CurrentConfiguration.Styles)) {
                    for (var i = 0; i < p.CurrentConfiguration.Styles.length; i++) {
                        if (p.CurrentConfiguration.Styles[i].indexOf('//') === -1) {
                            dLink(p.module.rootUrl + p.CurrentConfiguration.Styles[i]);
                        }
                        else {
                            dLink(p.CurrentConfiguration.Styles[i]);
                        }
                    }
                }
                if (!!p.CurrentConfiguration.Scripts && Array.isArray(p.CurrentConfiguration.Scripts)) {
                    LoadAndWaitForScript(0);
                }
                else {
                    //Finish inititializing, then boostrap
                    setTimeout(boostrap, 1);
                }
            }
        }

        //Loads a script and waits until it is loaded before loading the next script
        //When none are left it bootstraps Angular.js and the selected app
        function LoadAndWaitForScript(counter) {
            if (counter < p.CurrentConfiguration.Scripts.length) {
                var url = p.CurrentConfiguration.Scripts[counter];
                if (url.indexOf('//') === -1) {
                    url = p.module.rootUrl + url;
                }
                $LAB.script(url).wait(function () { counter++; LoadAndWaitForScript(counter) });
            }
            else {
                boostrap();
            }
        }

        //AngularJS Functions----------------------------------------------------------------------------------------
        //Starts Angular - called after configuration and script and style dependencies
        function boostrap() {
            //Custom app behavior
            if (p.CurrentConfiguration.Name !== p.Configurations.default.Name) {
                //The root module of a custom app *must* be named iqAppPartModule
                //Or you must provide the modules array via the app's defaultConfig.js file
                var m = p.CurrentConfiguration.Modules;
                if (!m || !Array.isArray(m)) {
                    angular.bootstrap(document.getElementById('appBody'), ['iqPart', 'iqAppPartModule']);
                }
                else {
                    angular.bootstrap(document.getElementById('appBody'), ['iqPart'].concat(m));
                }
            }
                //Default behavior
            else {
                angular.bootstrap(document.getElementById('appBody'), ['iqPart']);
            }
        };

        //Factories
        //Angular service Wrapper for ClientContext executeQuery.
        //The $q service makes it easy to wrap SharePoint's context.executeQueryAsync for use with Angular
        p.module.factory('SharePointService', ['$q', function ($q) {
            var SharePointService = {};

            SharePointService.executeQuery = function (context) {
                var deferred = $q.defer();
                context.executeQueryAsync(deferred.resolve, function (o, args) {
                    deferred.reject(args);
                });
                return deferred.promise;
            };

            return SharePointService;
        }]);

        //Controllers
        //Contoller for the bootstrapper. This is the top level scope of the app
        p.module.SPABootStrap = function ($scope, sharePointService) {
            //The partial file that is the root of the app
            //Out of the box this is either default.html, app.html, or config.html
            $scope.RootTemplate = p.module.rootTemplate;

            $scope.ctx = SP.ClientContext.get_current();
            $scope.InEditMode = !!window.parent.SP.Ribbon && window.parent.SP.Ribbon.PageState.Handlers.isInEditMode();

            //Used as a cache buster on the bootstrap's ng-include of the RootTemplate
            //<div ng-include="RootTemplate + '?m=' + Modified"></div>
            $scope.Modified = p.CurrentConfiguration.Modified || '1';

            //Saves the config js and then reloads the window to refresh the loaded config
            $scope.saveConfig = function () {
                SaveConfigFile($scope.ctx, function () { window.location.reload(true); });
            };

            $scope.setContainerHeight = function (height) {
                var container = $(window.frameElement.parentElement).attr('webpartid');
                if (!container) {
                    //If in a CEWP
                    $(window.frameElement.parentElement.parentElement).height(height + 10);
                    $(window.frameElement).height(height);
                } else {
                    //in a Page Viewer
                    var px = height + 'px';
                    window.frameElement.style.height = px;
                    window.frameElement.parentElement.style.height = px;
                }
                //IE11 and earlier bug with extra height added for scroll bars
                $('body').height(height - 20);
            };
        };

        //Controller for the default app
        p.module.Default = function ($scope, sharePointService) {
            //Size the container
            if ($scope.InEditMode) {
                $scope.setContainerHeight(400);
            }
            else {
                $scope.setContainerHeight(150);
            }

            //Create an array in $scope of the folders in the SPAs folder
            //Each folder is an app of the same name
            var ctx = SP.ClientContext.get_current();
            var folderToSearch = ctx.get_web().getFolderByServerRelativeUrl(p.webServerRelativeUrl + '/iqappcore/SPAs');
            ctx.load(folderToSearch, "Folders");
            sharePointService.executeQuery(ctx)
                .then(function (profile) {
                    var enumerator = folderToSearch.get_folders().getEnumerator();
                    while (enumerator.moveNext()) {
                        var folderItem = enumerator.get_current();
                        $scope.Apps.push(folderItem.get_name());
                    }
                },
                function (error) {
                    alert(error.get_message());
                });

            //Initialize the scope
            var newAppText = 'Create new AngularJS app...';
            $scope.Apps = [newAppText];
            $scope.model = {};
            $scope.model.SelectedApp = '';
            $scope.State = 'view';

            $scope.appChanged = function () {
                if ($scope.model.SelectedApp === newAppText) {
                    $scope.State = 'new';
                    $scope.newApp = {};
                }
                else {
                    $LAB.script(p.webServerRelativeUrl + '/iqappcore/SPAs/' + $scope.model.SelectedApp + '/defaultConfig.js?m=' + (new Date()).toJSON());
                }
            };

            //Setup a callback to allow the config files to insert their config at runtime
            $scope.SelectedConfig = null;
            p.SetNewConfiguration = (function () {
                return function (config) {
                    $scope.SelectedConfig = config;
                    $scope.$apply();
                };
            })();

            //Apply a selected app to the web part
            $scope.setPart = function () {
                p.CurrentConfiguration = p.Configurations[partConfigurationKey] = $scope.SelectedConfig;
                SaveConfigFile(ctx, function () { window.location.reload(true) });
            };

            //Create a new app from the templates
            $scope.createApp = function () {
                var folderUrl = 'iqappcore/SPAs/' + $scope.newApp.Name;
                var root = ctx.get_web().get_rootFolder();
                root.get_folders().add(folderUrl);

                var srcBase = p.webServerRelativeUrl + '/iqappcore/SPAContainer/template/';
                copyFile(srcBase + 'app.css', folderUrl + '/app.css');
                copyFile(srcBase + 'config.html', folderUrl + '/config.html');
                copyFile(srcBase + 'app.html', folderUrl + '/app.html');
                copyFile(srcBase + 'app.js', folderUrl + '/app.js');

                var defaultConfigTemplate = '//This is the default configuration for this app part\r\n' +
                'var p = window.iqPart; if (!!p && !!p.SetNewConfiguration) { \r\n' +
                '    p.SetNewConfiguration( \r\n' +
                '        {        \r\n' +
                '            //Add your custom properties here\r\n' +
                '            "SelectedDemo": "timer", \r\n' +
                '            //Or overide defaults\r\n' +
                '            "Name": "{{Name}}", \r\n' +
                '            "Description": "{{Description}}",\r\n' +
                '            "RootTemplate": "app.html",\r\n' +
                '            "Scripts": [ \r\n' +
                '                "app.js" \r\n' +
                '            ],\r\n' +
                '            "Styles": [\r\n' +
                '                "app.css"\r\n' +
                '            ],\r\n' +
                '            "Modules": [\r\n' +
                '                "iqAppPartModule"\r\n' +
                '            ]\r\n' +
                '        });\r\n' +
                '}';

                var iqCoreLib = ctx.get_web().get_lists().getByTitle('IQAppCore');

                var defaultConfigFile = (defaultConfigTemplate.replace('{{Name}}', $scope.newApp.Name)).replace('{{Description}}', $scope.newApp.Description);

                //Create the new app and config 
                SaveFile(defaultConfigFile, folderUrl + '/defaultConfig.js', iqCoreLib, ctx, function () {
                    p.CurrentConfiguration = p.Configurations[partConfigurationKey] = JSON.parse(JSON.stringify(p.Configurations.template));
                    p.CurrentConfiguration.Name = $scope.newApp.Name;
                    p.CurrentConfiguration.Description = $scope.newApp.Description;

                    //Then save the global config
                    SaveConfigFile(ctx, function () { window.location.reload(true) });
                });
            }

            $scope.cancelAdd = function () {
                $scope.model.SelectedApp = '';
                $scope.State = 'view';
                $scope.SelectedConfig = {};
            };

            function copyFile(src, dest) {
                (ctx.get_web().getFileByServerRelativeUrl(src)).copyTo(dest);
            }
        };

        p.module.controller('SPABootStrap', ['$scope', 'SharePointService', p.module.SPABootStrap]);
        p.module.controller('Default', ['$scope', 'SharePointService', p.module.Default]);
        //End AngularJS functions


        //Configuration----------------------------------------------------------------------------------------------------------------------
        function GetCurrentPartConfig(ConfigLoadedCallback) {
            //partConfigurationKey is the property of the Configurations object for the current part's configuration
            //It is the ID of the web part
            //Works for PageViewer and CEWP with iframe
            parentFrameId = !!window.frameElement ? $(window.frameElement.parentElement).attr('webpartid') || $(window.frameElement.parentElement.parentElement).attr('webpartid') : null;
            partConfigurationKey = parentFrameId;

            if (!p.Configurations[partConfigurationKey] && !!urlParams.app) {
                //This is called by defaultConfig.js if it loads successfully 
                p.SetNewConfiguration = (function () {
                    return function (config) {
                        p.Configurations[partConfigurationKey] = config;
                        p.CurrentConfiguration = p.Configurations[partConfigurationKey];
                        ConfigLoadedCallback();
                    };
                })();
                $LAB.script(p.webServerRelativeUrl + '/iqappcore/SPAs/' + urlParams.app + '/defaultConfig.js?m=' + (new Date()).toJSON());
            }
            else {
                p.Configurations[partConfigurationKey] = p.Configurations[partConfigurationKey] || p.Configurations.default;
                p.CurrentConfiguration = p.Configurations[partConfigurationKey];
                ConfigLoadedCallback();
            }
        }

        function SaveConfigFile(ctx, callback) {
            //Saves the config file in SharePoint
            p.CurrentConfiguration.Modified = (new Date()).toJSON();
            var currentConfig = JSON.stringify(p.CurrentConfiguration);
            var pageParts = window.location.pathname.split('/');
            var pageName = pageParts.length > 0 ? pageParts[pageParts.length - 1] : window.location.pathname;
            var iqCoreLib = ctx.get_web().get_lists().getByTitle('IQAppCore');
            var configFileUrl = window.location.pathname.replace(pageName, 'configurations.js');

            //Reload the config and update this part
            //to minimize concurrency issues from edit collisions
            $LAB.script(configFileUrl).wait(function () {
                p.Configurations[partConfigurationKey] = JSON.parse(currentConfig);
                var fileContent = configTemplate.replace('{{config}}', JSON.stringify(p.Configurations, null, 4));
                SaveFile(fileContent, configFileUrl, iqCoreLib, ctx, callback);
            });
        }

        function SaveFile(fileContent, fileUrl, library, ctx, callback) {
            var fileCreateInfo = new SP.FileCreationInformation();
            fileCreateInfo.set_url(fileUrl);
            fileCreateInfo.set_content(new SP.Base64EncodedByteArray());
            fileCreateInfo.set_overwrite(true);

            for (var i = 0; i < fileContent.length; i++) {
                fileCreateInfo.get_content().append(fileContent.charCodeAt(i));
            }

            var service = {
                execute: function () {
                    var newFile = library.get_rootFolder().get_files().add(fileCreateInfo);
                    ctx.executeQueryAsync(this.success, this.failure);
                },
                success: function () {
                    //Reload the config file
                    if (fileUrl.startsWith(p.webServerRelativeUrl)) {
                        $LAB.script(fileUrl + '?m=' + new Date().toJSON()).wait(callback);
                    }
                    else {
                        $LAB.script(p.webServerRelativeUrl + '/' + fileUrl + '?m=' + new Date().toJSON()).wait(callback);
                    }
                },
                failure: function (sender, args) {
                    //TODO: Issue winding up here whene everything seems to have worked
                    //there is no error provided when this happens
                    if (!!args && !!args.get_message()) {
                        alert('Config setup request failed! ' + args.get_message() + '\n' + args.get_stackTrace());
                    }
                }
            };
            service.execute();
        }
    });
})(jQuery);
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <title>SPA Scaffolding</title>
    <link type="text/css" rel="stylesheet" href="../bootstrap/css/bootstrap.min.css" />
    <script type="text/javascript" src="https://ajax.aspnetcdn.com/ajax/4.0/1/MicrosoftAjax.js"></script>
    <script type="text/javascript" src="_layouts/15/sp.runtime.js"></script>
    <script type="text/javascript" src="_layouts/15/sp.js"></script>
</head>
<body style="background-color: transparent">
    <div id="appBody" ng-controller="SPABootStrap">
        <div ng-include="RootTemplate + '?m=' + Modified"></div>
    </div>
    <script src="../Scripts/lab.min.js"></script>
    <script src="../Scripts/jquery-2.1.1.min.js"></script>
    <script src="../Scripts/bootstrap.js"></script>
    <script src="../Scripts/angular.js"></script>
    <script src="../Scripts/angular-ui/ui-bootstrap-tpls.js"></script>
    <script src="configurations.js"></script>
    <script src="iqpart.js"></script>
</body>
</html>

//This is the default configuration for this app part
var p = window.iqPart; if (!!p && !!p.SetNewConfiguration) { 
    p.SetNewConfiguration( 
        {        
            "Name": "Create Components",
            "Description": "The third AngularJS demo. https://angularjs.org/",
            "RootTemplate": "app.html",
            "Scripts": [ 
                "app.js" 
            ],
            "Styles": [
                "app.css"
            ],
            "Modules": [
                "app"
            ]
        });
}
//This is the default configuration for this app part
var p = window.iqPart; if (!!p && !!p.SetNewConfiguration) { 
    p.SetNewConfiguration( 
        {        
            "Name": "The Basics", 
            "Description": "The first AngularJS demo.",
            "RootTemplate": "app.html",
            "Scripts": [ 
                "app.js" 
            ],
            "Styles": [
                "app.css"
            ]
        });
}
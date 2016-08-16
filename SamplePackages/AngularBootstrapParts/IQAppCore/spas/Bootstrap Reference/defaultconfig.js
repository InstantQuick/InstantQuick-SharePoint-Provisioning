//This is the default configuration for this app part
var p = window.iqPart; if (!!p && !!p.SetNewConfiguration) { 
    p.SetNewConfiguration( 
        {        
            "Name": "Bootstrap Reference", 
            "Description": "Handy bootstrap 3 reference.",
            "RootTemplate": "app.html",
            "Scripts": [
                "app.js" 
            ],
            "Styles": [
                "//bootstrapdocs.com/v3.0.0/docs/examples/theme/theme.css",
                "app.css"
            ]
        });
}
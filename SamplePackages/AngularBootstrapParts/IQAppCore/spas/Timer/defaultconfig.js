//This is the default configuration for this app part
var p = window.iqPart; if (!!p && !!p.SetNewConfiguration) { 
    p.SetNewConfiguration( 
        {        
            //Add your custom properties here
            "SelectedDemo": "timer", 
            //Or overide defaults
            "Name": "Timer", 
            "Description": "Timer configuration example",
            "RootTemplate": "app.html",
            "Scripts": [ 
                "app.js" 
            ],
            "Styles": [
                "app.css"
            ]
        });
}
//This is the default configuration for this app part
var p = window.iqPart; if (!!p && !!p.SetNewConfiguration) { 
    p.SetNewConfiguration( 
        {        
            //Add your custom properties here
            "SelectedDemo": "hello", 
            //Or overide defaults
            "Name": "Greeting", 
            "Description": "Say hello to the current user using the SharePoint client object model.",
            "RootTemplate": "app.html",
            "Scripts": [ 
                "app.js" 
            ],
            "Styles": [
                "app.css"
            ]
        });
}
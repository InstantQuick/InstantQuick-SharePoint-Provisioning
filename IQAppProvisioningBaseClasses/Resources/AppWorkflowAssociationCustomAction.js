(function () {
    var workflowCreators = {@WorkflowCreatorsJSON};
    var serviceUrl = '{@WebServerRelativeUrl}/_vti_bin/webpartpages.asmx';

    var index = 0;
    var urls;
    if (!!workflowCreators) {
        urls = Object.keys(workflowCreators);
        document.addEventListener('DOMContentLoaded', registerWorkflows);
    }

    function registerWorkflows() {
        if(index !== urls.length){
            registerWorkflow(urls[index]);
        }
        else{
            selfDestruct();
        }
    }

    function registerWorkflow(url) {
        var soapRequest = new XMLHttpRequest();
        soapRequest.open('POST', serviceUrl, true);
        soapRequest.setRequestHeader('SOAPAction', 'http://microsoft.com/sharepoint/webpartpages/AssociateWorkflowMarkup');
        soapRequest.setRequestHeader('Content-Type', 'text/xml; charset=utf-8');
        soapRequest.send('<?xml version="1.0" encoding="utf-8"?><soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><soap:Body><AssociateWorkflowMarkup xmlns="http://microsoft.com/sharepoint/webpartpages"><configUrl>' + url + '</configUrl><configVersion>V1.0</configVersion></AssociateWorkflowMarkup></soap:Body></soap:Envelope>');

        soapRequest.onreadystatechange = function() {
            if(soapRequest.readyState == 4 && soapRequest.status == 200) {
                index++;
                //Seems to be a race condition when WF's are on the same list, so pause before the next one
                setTimeout(registerWorkflows, 500);
            }
        }
    }

    function selfDestruct(){
        if (typeof ('SP') === 'undefined' || SP.ClientContext == undefined) {
            ExecuteOrDelayUntilScriptLoaded(selfDestruct, "sp.js");
            return;
        }
        var ctx = SP.ClientContext.get_current();
        
        var existingActions = ctx.get_web().get_userCustomActions();

        ctx.load(existingActions);
        ctx.executeQueryAsync(searchAndDestroy, function () { alert("Unable to get custom actions!"); });

        function searchAndDestroy() {
            var e = existingActions.getEnumerator();
            var del = [];

            while (e.moveNext()) {
                var action = e.get_current();
                if (action.get_title() === "{@UserCustomActionTitle}") {
                    del.push(action);
                }
            }

            if (del.length > 0) {
                for (var i = 0; i < del.length; i++) {
                    del[i].deleteObject();
                }
                ctx.executeQueryAsync();
            }
        }
    }
})();


var SuperOffice = SuperOffice || {};
SuperOffice.ClientCrossMessaging = SuperOffice.ClientCrossMessaging || {};

(function(ns)
{
    var sendCommand = function(command, args) {
        var message = { "command": command, "arguments": args};
        parent.postMessage(message, "*");
    }

    ns.refresh = function() {
        sendCommand("refresh");
    }

    ns.executeSoProtocol = function(protocol) {
        sendCommand("soprotocol", protocol);
    }

    ns.openDocument = function(documentId) {
        sendCommand("openDocument", documentId);
    }

    ns.ajaxMethod = function(method,...args) {
        var a = [method,...args];
        sendCommand("ajaxMethod", a);
        }
    
}(SuperOffice.ClientCrossMessaging));
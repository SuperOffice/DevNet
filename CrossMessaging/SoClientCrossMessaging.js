var SuperOffice = SuperOffice || {};
SuperOffice.ClientCrossMessaging = SuperOffice.ClientCrossMessaging || {};

(function(ns)
{
    var sendCommand = function(command, arguments) {
        var message = { "command": command, "arguments": arguments};
        parent.postMessage(message, "*");
    }

    ns.refresh = function() {
        sendCommand("refresh");
    }

    ns.executeSoProtocol = function(protocol) {
        sendCommand("soprotocol", protocol);
    }

}(SuperOffice.ClientCrossMessaging));
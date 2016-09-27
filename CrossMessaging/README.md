# Cross messaging between SuperOffice CRM (Web) and embedded web panels/partner applications
This sample demonstrates how window.postMessage can be used for communication from embedded web panels to SuperOffice CRM.
The script file SoClientCrossMessaging.js is provided as a convenience wrapper around window.postMessage with the supported commands. 

There are two commands which are supported:

* SuperOffice.ClientCrossMessaging.refresh()
* SuperOffice.ClientCrossMessaging.executeSoProtocol(soprotocol)

# Sample instructions 
* Add a web panel to your SuperOffice CRM installation linked to index.html
* Open SuperOffice and the web panel
* Click refresh and soprotocol buttons
* Observe the changes in the SuperOffice CRM client

# Add to your partner application/web panel
* Include the file SoClientCrossMessaging.js file in your application 
* Call the methods SuperOffice.ClientCrossMessaging.refresh/executeSoProtocol when neccessary

# Availability
Note that this functionality is currently not available, but will be made available in Online on the next release in October 2016. 


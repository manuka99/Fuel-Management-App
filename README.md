# Fuel Management Application (Backend)

# Project Specification
The purpose of this assignment is to create a Mobile application to manage Fuel queues in
fuel stations using client-server architecture. The mobile application should facilitate below
functionality.
1. End user Login facility (keep credentials in SQLite DB)
2. User should be able to update arrival and depart time from fuel queue of selected
fuel station
3. Station Owner should be able to update fuel arrival time, fuel finish time and the
fuel type of the fuel station
4. User could see the current fuel queue length of the given station. (Is this the total number of
vehicles or vehicles by type), fuel status of fuel station. (Show available until it is updated as finished) and how long people waiting at the queue for selected fuel station.
5. Extensive User Interfaces
6. Also, add additional functionality which is not given in the specification.

Mobile application is the client application, and it should connect to Centralized web
service via network to get processed data and the server-side data storage type is
NoSQL database. Therefore, it should develop an Android mobile application which has
only interfaces to interact and Web service to facilitate data and Business logic and the
NoSQL data base which should connected with Webservice to store and retrieve data. 

# Project Scenario:
As vehicle owner you entered to the Nearest petrol queue. Then you login to the
application and search for the petrol shed you entered. Then you click on “Joined to the
queue” button and if you leave before refueling, you click on “Exit before pump fuel”
button otherwise , click on “Exit after pump fuel” buttons.
When another person logs in and checks for the nearest queue ( for an example the queue
you entered ), then should be able to see how many are in the queue by vehicle type.
Further, Shed owners can update the fuel status based on arrival of fuel and finish status
of the fuel. That status should be visible to all users.

# Technologies
1. Client – Mobile Application - Android /java
2. Server(Backend) – ASP.net (c#) REST API hosted in windows server IIS and heroku cloud
3. Backend Database - NoSQL DB (Mongo DB)

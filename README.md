# ccc-coordinator-webapp

Webapp to wrangle cats.

Our bike club has weekly rides, and we use a spread sheet to track them.
The amount of info to manage is too much for a spreadsheet.

# Models
- CueEntry - turn left, turn right, go straight, put it in the ditch, etc.
- Route - A distance, desc, and a list of cues
- RideEvent - A collection of Group Rides that take place together.
- RideType - What kind of bike ride are we doing?
- GroupRide - The actual group ride happening as part of the event. What route? how fast.


# API
c# api. 
azure b2c for social logins. Coordinators use Facebook or gmail logins

possible to support AzureAd for admin tasks, and azureb2c for end user login? or just make 2 apis, 1 for webapp, and 1 for admin access

# Webapp
razor? something else trendy?

# Infrastructure
Use terraform to create resources.
Use Azure storage container for remote state storage.
Initial run from local pc.
End state - run from azure pipeline.

# Depoyment
azure pipeline to build and deploy to webapp/api services
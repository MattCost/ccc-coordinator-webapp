# ccc-coordinator-webapp

Webapp to wrangle cats.

Our bike club has weekly rides, and we use a spread sheet to track them.
The amount of info to manage is too much for a spreadsheet.

# Models
- CueEntry - Go fast, turn left, on MainSt at mile 5, etc
- Route - A distance, desc, and a list of cues
- GroupRide - The group ride happening as part of the event. What route? how fast, etc.
- RideEvent - A collection of Group Rides that take place with a common starting time/place.

## Enums
- CueOperation - turn left, turn right, go straight, put it in the ditch, etc.
- RideType - What kind of bike ride are we doing?

# API
c# api. 
azure b2c for social logins. Coordinators use Facebook or gmail logins
website and phone app?


# Webapp
Using Razor for now, might try blazor for more response coordinator signup pages


# Infrastructure
Use terraform to create resources.
Use Azure storage container for remote state storage.
Initial run from local pc.
End state - run from azure pipeline

# Deployment
azure pipeline to build and deploy to webapp/api services

# User Roles (via custom Attributes in Azure B2C)
- Coordinator - can sign up to help lead a ride.
- CoordinatorAdmin - can assign coordinator role to other users. Can modify coordinator assignments
- Contributor - CRUD operations
- Admin - CRUD operations, role management.

# TODOs

setup facebook login
    done but not verified

setup gmail login
    done for default domain

test all policy based access

setup custom domain (requires move away from Free App Service)
    google login with custom domain
    facebook login with custom domain and verify


coordinator admin page to let admins assign coordinator role to others
    search - find userId by display name / email
    can we require email via b2c?

CSS Styling on the entire app!

views for riders vs coordinators
    public access for riders?
    allow anon access to view rides?
    view model returned has different data based on user

bottom of cue list - "print this cue sheet"

overall "print cue sheets"

cue operations need some attributes so we can get those for printing.
Left => L
Left at => L @
Cross => X 
etc

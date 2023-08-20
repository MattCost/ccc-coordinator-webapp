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

# how to start

run postgresql with table vstops
run docker with osrm engine
finally 
dotnet run

# use api

use postman

post http://localhost:8080/osrm
with body type application/json
{
    "waypoints": [
          { "Lng" : "126.79939689052419", "Lat" : "37.527319039426736" },
          { "Lng" : "126.87491792408139","Lat" : "37.627320777159646" },
          { "Lng" : "127.06568457951413","Lat" : "37.665543301893806" },
          { "Lng" : "127.16568457951413", "Lat" : "37.565543301893806" } 
    ],
    "demands": [ 
          { "Lng" : "126.80939689052419", "Lat" : "37.547319039426736" },
          { "Lng" : "126.90491792408139", "Lat" : "37.657320777159646" } 
    ]
}

# how to start

run postgresql with table vstops
run docker with osrm engine
```
docker run -t -i -p 5000:5000 -v "${PWD}:/data" osrm/osrm-backend osrm-routed --algorithm mld /data/south-korea-latest.osrm
```
finally 
dotnet run

* use api

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

* waypoints sequence optimizing
post http://localhost:8080/dispatch
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

==========
# 임시로 223.130.146.101 에 실행되고 있는 방식
screen 에 실행 되고 있음
```
ciel@core-test:~/CielSimulatorDispatchEngine$ screen -r
There are several suitable screens on:
        4027075.pts-0.core-test (07/17/2024 10:31:57 AM)        (Detached)
        4011137.pts-0.core-test (07/17/2024 10:03:23 AM)        (Detached)
Type "screen [-d] -r [pid.]tty.host" to resume one of them.
```
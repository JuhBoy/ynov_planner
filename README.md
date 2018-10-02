# Planner API  
  
This project has been initialized by the wild group Ynov. Its objective is to provide a Multiplateforme Api for Event Management.  
The stack:  
- ASP.NET Core  
- Entity Framwork  
- Xunit  
- JWT (HmacSha256)  
  
# Run Application  
  
It'll be necessary to have dotnet 2.0 and mysql installed on your server or local device.  
If you don't have it yet, please follow the steps : https://www.microsoft.com/net/learn/get-started/windows
  
## Command tools  
  
**In the root solutions directory:**  

* `dotnet restore`  
  
**In the PlannerApi/ directory:**  
  
* `dotnet ef database update`  
  
if you get issues with database connection, don't forget to change values in the appsettings.json file.
connectionStrings => Mysql &| MysqlTests
  
* `dotnet run`  
  
If you encounter problem, ensure all tools and package are properly installed.

* `dotnet publish -c Release`

Publish a production version of the API as a dll. If you use linux service, serveur will be started automatically.
A restart of the service can be needed.   
  
# Run Unit Tests  
  
Units tests are localized in the PlannerApi.Tests folder from the base solution. It merge a complete solution of Unit, Integration, Functional tests.  
  
### Command tools  
  
1. cd .../PlannerApi.Tests  
2. dotnet xunit  
  
Running on mac or windows with visual studio, you can run the tests directly from the UI of your IDE.  
  
### Design New Tests  
  
If you are planning to create new tests, make sure to use the nested class pattern. Its better for visibilty and architecturing.  
However for Integration tests, It's needed to keep a lone instance of fixtures. Use the classic schema One File, One Test Class.  
Implentation of IClassFixture<T> Generic Interface is required to use dependency injection.

### Production setup

Using systemctl on Linux configuration to watch dotnet process:

````
[Unit]
Description=Ynov Api For planner

[Service]
WorkingDirectory=/home/<user>/ynov_planner/PlannerApi/
ExecStart=/usr/bin/dotnet /home/<user>/ynov_planner/PlannerApi/bin/Release/netcoreapp2.0/events_planner.dll
Restart=always
RestartSec=5
SyslogIdentifier=dotnet-ynov-planner-api
User=<user>
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
````
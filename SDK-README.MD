# Hydra Windows SDK #

This is the SDK targeting .NET Framework 4.5 that helps easily develop windows application with required Hydra custom VPN protocol support.

Download [the last version SDK](https://firebasestorage.googleapis.com/v0/b/web-portal-for-partners.appspot.com/o/products%2FHydra-sdk-windows.zip?alt=media&token=494ec0bc-a513-4521-adba-3be2f8cd79b3)

# Requirements #

- Windows 7 or later
- .NET Framework 4.5
- Visual Studio 2015 (2017 for WPF sample project)

# Components #

1. **Core**
  - **`Hydra.Sdk.Common`** - contains common IoC and logging components
  - **`Hydra.Sdk.Backend`** - contains backend related logic (interfaces and implementation of backend service) 
  - **`Hydra.Sdk.Vpn`** - contains VPN related logic (interfaces and default implementation of VPN service), vpn executable and related binary files 
2. **Windows**
  - **`Hydra.Sdk.Windows`** - contains VPN service implementation which is dependent on windows service below
  - **`Hydra.Sdk.Windows.Service`** - windows hydra management service which is used to avoid elevation issues
3. **Sample**
  - **`Hydra.Sdk.Wpf`** - example of how to work with backend and vpn
4. **Test**
  - **`Hydra.Sdk.Backend.Test`** - tests for backend service
  - **`Hydra.Sdk.Vpn.Test`** - tests for vpn service
5. **tap** - TAP driver  

# TAP driver installation #  

To install TAP driver go to the `tap` folder and execute `install-tap.bat` from folder from `32bit` folder for Windows x86 or from `64bit` folder for Windows x64 as Administrator.
  
# Working with backend #

### Bootstrapping ###

To be able to work with backend service, you need to bootstrap backend module by providing valid ***Carrier ID*** and ***VPN server URL***. This could be done by using following code snippet (you need to reference `Hydra.Sdk.Common` and `Hydra.Sdk.Backend` assemblies):

```C#
var backendServerConfiguration = new BackendServerConfiguration(carrierId, vpnServerUrl);
var hydraBackendBootstrapper = new HydraBackendBootstrapper(backendServerConfiguration);
hydraBackendBootstrapper.Bootstrap();
``` 

### Getting the backend service instance ###

After bootstrapping you can get the backend service instance by simply resolving `IPartnerBackendService` from IoC container:

```C#
var vpnServerService = HydraIoc.Container.Resolve<IPartnerBackendService>();
```

### Backend service methods ###

You can find all available backend service methods in `IPartnerBackendService` interface, and examples for each of them in the `Hydra.Sdk.Backend.Test` and `Hydra.Sdk.Wpf` projects. All required information for establishing VPN connection is shown below.

#### `IPartnerBackendService` interface ####

Manages client user: authentication, credentials retrieval, user info.

|Method                                                                                         |Description                                                      |
|-----------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
|Task<PostLoginResponse> LoginAsync(LoginParam parameters)                                      |Logs in to vpn server                                            |
|Task<GetCredentialsResponse> GetCredentialsAsync(GetCredentialsParam parameters)               |Gets credentials for establishing vpn connection                 |
|Task<GetCountriesResponse> GetCountriesAsync(string accessToken)                               |Gets available vpn countries. Requires access token as parameter |
|Task<GetCountriesResponse> GetCurrentUserAsync(string accessToken)                             |Gets current user information. Requires access token as parameter|
|Task<GetLogoutResponse> LogoutAsync(LogoutRequestParam parameters)                             |Performs logout on server side                                   |
|Task<GetIsConnectedResponse> IsConnectedAsync()                                                |Checks if VPN connection is established                          |
|Task<GetConfigResponse> GetConfigAsync()                                                       |Gets configuration data from server                              |
|Task<GetRemainingTrafficResponse> GetRemainingTrafficAsync(GetRemaningTrafficParam parameters) |Gets incoming and outcoming vpn traffic from the server          |

#### Login example ####

At first, you need to log in to the vpn server by specifying five parameters:

- `AuthenticationMethod` - desired authentication method. Use `AuthenticationMethod.Anonymous` for anonymous login
- `DeviceId` - unique identificator of your device
- `DeviceName` - name of your device, e.g. `Environment.MachineName`
- `DeviceType` - specify `DeviceType.Desktop` or `DeviceType.Mobile`
- `OAuthAccessToken` - access token for **OAuth** protocol. Leave this empty for anonymous login

 All those parameters are contained in the `LoginParam` class, so you need to instantiate it and then call `LoginAsync` method:

```C#
var loginParam = new LoginParam(
            AuthenticationMethod.Anonymous,
            DeviceId,
            Environment.MachineName,
            DeviceType.Desktop,
            string.Empty);

var loginResponse = await vpnServerService.LoginAsync(loginParam);
```

Do not forget to check whether the request was successful:

```C#
if (!loginResponse.IsSuccess || loginResponse.Result != ResponseResult.Ok)
{
    // Handle unsuccessful request
}
```

You will need `AccessToken` from login response to get VPN credentials. 

#### Get credentials example ####

To get VPN credentials you have to specify two parameters:

- `AccessToken` - access token from login response
- `Country` - desired country, optional

 All those parameters are contained in the `GetCredentialsParam` class, so you need to instantiate it and then call `GetCredentialsAsync` method:

```C#
var credentialsParam = new GetCredentialsParam(accessToken);

var credentialsResponse = await vpnServerService.GetCredentialsAsync(credentialsParam);
```

Do not forget to check whether the request was successful:

```C#
if (!credentialsResponse.IsSuccess || credentialsResponse.Result != ResponseResult.Ok)
{
    // Handle unsuccessful request
}
```

VPN credentials are located in the `VpnCredentials` property of get credentials response:

```C#
var credentials = credentialsResponse.VpnCredentials;
```

# Connecting to the VPN server #

### Bootstrapping ###

To be able to work with VPN client, first you need to bootstrap VPN module. It could be done by using following code snippet (you need to reference `Hydra.Sdk.Common` and `Hydra.Sdk.Vpn` assemblies):

```C#
var hydraVpnBootstrapper = new HydraVpnBootstrapper();
hydraVpnBootstrapper.Bootstrap();
``` 

### Getting the VPN client instance ###

After bootstrapping you can get the VPN client instance by simply resolving `IHydraVpnClient` from IoC container:

```C#
var vpnClient = HydraIoc.Container.Resolve<IHydraVpnClient>();
```

### VPN Client methods ###

You can find all available vpn client methods, properties and events at `IHydraVpnClient` interface, and examples for each of them in the `Hydra.Sdk.Vpn.Test` and `Hydra.Sdk.Wpf` projects. All required information for establishing VPN connection is shown below.

#### `IHydraVpnClient` interface ####

Manages device's VPN connection. To establish VPN connection user should manually install TAP driver.

|Method                                                         |Description          |
|---------------------------------------------------------------|---------------------|
|Task<ConnectionInfo> Connect(HydraConfigParams parameters)     |Opens VPN connection |
|Task Disconnect()                                              |Closes VPN connection|

|Property                                                       |Description                                                             |
|---------------------------------------------------------------|------------------------------------------------------------------------|
|bool IsConnected                                               |Gets a value indicating whether the client is connected to a destination|
|VpnStatistics Statistics                                       |Provides the statistics for the VPN connection                          |

|Event                                                          |Description                                                              |
|---------------------------------------------------------------|-------------------------------------------------------------------------|
|EventHandler\<VpnStatisticsChangedEventArgs\> StatisticsChanged|Notifies subscribers about changes of consumed network traffic statistics|
|EventHandler\<VpnConnectedEventArgs\> Connected                |Notifies subscribers about successful VPN connection                     |
|EventHandler\<VpnDisconnectedEventArgs\> Disconnected          |Notifies subscribers about VPN disconnection                             |

#### Connect example ####

To connect to the VPN server you have to specify following parameters:

- `Channel` - reserved
- `ConnectionString` - reserved
- `DestinationIp` - IP of VPN server to connect to, required
- `DestinationPort` - port of VPN server to connect to, 443 by default
- `Patch` - optional hydra configuration patch
- `UserHash` - put here value of the `UserName` property of VPN credentials
- `Version` - version of your application, optional

 All those parameters are contained in the `HydraConfigParams` class. Subscribe to the following events:
 - `Connected` event for being notified when client will be connected;
 - `Disconnected` event for being notified when client will be disconnected;
 - `StatisticsChanged` event for being notified when bytes are sent and/or bytes are received (count will be changed).
 
 You need to instantiate `HydraConfigParams` class and then call `Connect` method:

```C#
var configuration = new HydraConfigParams
{
    UserHash = credentials.UserName,
    DestinationIp = credentials.Ip,
    DestinationPort = !string.IsNullOrWhiteSpace(credentials.Port)
                          ? int.Parse(credentials.Port)
                          : 443,    
};

vpnClient.Connected += (sender, args) => 
{
    // Handle connected state
};
vpnClient.Disconnected += (sender, args) => 
{
    // Handle disconnected state
};
vpnClient.StatisticsChanged += (sender, args) => 
{
    // Handle bytes count
}

var connectionResult = await vpnClient.Connect(configuration);
if (connectionResult == null)
{
	// Handle unsuccessful connection
}
```

Note that `Disconnected` event fires only when hydra is disconnected by itself for some reason, it does not fire when you disconnect it manually.

#### Disconnect example ####

To disconnect from VPN server simply call `Disconnect` method:

```C#
await vpnClient.Disconnect();
``` 

# Using VPN service #

Setting up VPN connection requires elevation. As workaround you can use Hydra management service (Hydra.Sdk.Windows.Service).

**Note**: windows service also does several useful things, such as process observing, so we strongly recommend you to use this approach.  

### Installation ###

To be able to use the service you need to install it into the system once. It could be done by following command:

```
Hydra.Sdk.Windows.Service.exe -install <serviceName>
``` 

where `serviceName` is the name of your service.

### Uninstallation ###

To uninstall the service run the following command:

```
Hydra.Sdk.Windows.Service.exe -uninstall <serviceName>
```

where `serviceName` is the name of your service.

### Bootstrapping ###

To use windows service based vpn client instead of default, reference `Hydra.Sdk.Windows` assembly and use following code snippet for the bootstrapping:

```C#
var hydraVpnBootstrapper = new HydraWindowsBootstrapper();
hydraVpnBootstrapper.Bootstrap(ServiceName);
```

`ServiceName` is the name of your installed Hydra management service.
That's all, you can resolve `IHydraVpnClient` from `HydraIoC` and use it as usual.  

# Setting up logging #
 
Key places of Hydra SDK are instrumented with logging for debug purposes. If you really need to see those log messages, you have to provide your own `LoggerListener`.

First, create class derived from `BaseLoggerListener` and implement abstract methods:

```C#
internal class MyLoggerListener : BaseLoggerListener
{
    public override void Trace(string message)
    {
        Console.WriteLine(message);    
    }

    // Here goes implementation of other methods
}
```

Then add an instance of your logger listener as handler to the HydraLogger:

```C#
HydraLogger.AddHandler(new MyLoggerListener());
```

That's all. You will get log output of your logger.

# Hydra Windows SDK #

This is the SDK targeting .NET Framework 4.5 that helps to easily develop Windows application with required Hydra custom VPN protocol support.

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
  - **`Hydra.Sdk.Windows`** - contains VPN service implementation which is dependent on Windows service below
  - **`Hydra.Sdk.Windows.Service`** - Windows hydra management service which is used to avoid elevation issues
3. **Sample**
  - **`Hydra.Sdk.Wpf`** - example of how to work with backend and vpn
4. **Test**
  - **`Hydra.Sdk.Backend.Test`** - tests for backend service
  - **`Hydra.Sdk.Vpn.Test`** - tests for vpn service
  - **`Hydra.Sdk.Windows.Test`** - tests for Hydra Windows SDK implementation
5. **tap** - TAP driver  

# TAP driver installation #  

To install TAP driver go to the `tap` folder and execute `install-tap.bat`  from `32bit` folder for Windows x86 or from `64bit` folder for Windows x64 as Administrator.
  
# Windows service installation #

To be able to use the hydra service you need to install it into the system once. It could be done by the following command:

```
Hydra.Sdk.Windows.Service.exe -install <serviceName>
``` 

where `serviceName` is the name of your service.
  
# Bootstrapping #  
  
To be able to work with hydra sdk, you need to bootstrap hydra by providing valid backend server configuration and hydra configuration. This could be done by using following code snippet (you need to reference `Hydra.Sdk.Windows`, `Hydra.Sdk.Common` and `Hydra.Sdk.Backend` assemblies):

```C#
var bootstrapper = new HydraWindowsBootstrapper();

var backendConfiguration = new BackendServerConfiguration(CarrierId, VpnServerUrl, DeviceId);
var hydraConfiguration = new HydraWindowsConfiguration(ServiceName);

bootstrapper.Bootstrap(backendConfiguration, hydraConfiguration);
``` 

To specify bypass domains or disable automatic reconnection after coming back from sleep mode, you need to set corresponding hydra configuration properties before bootstrapping:

```C#
var hydraConfiguration = new HydraWindowsConfiguration(ServiceName)
    .AddBypassDomains(BypassDomainsList)
    .SetReconnectOnWakeUp(ReconnectOnWakeUp);
```
  
# Authentication #

Anchorfree Partner VPN Backend supports OAuth authentication with a partner's OAuth server. This is a primary authentication method.
Follow next steps for implementing OAuth:

1. Deploy and configure OAuth service. Service should be publicly available in Internet
2. Configure Partner Backend to use OAuth service
3. Implement client OAuth for your application
4. Retrieve access token in client app. This token will be used to initialize and sign in to Android Partner

There are some auth method types:

```C#
AuthMethod.Anonymous();
AuthMethod.Firebase(token);
AuthMethod.GitHub(token);
AuthMethod.OAuth(token); // Custom OAuth
```
For more AuthMethod types, see API reference.

An example of login implementation:

```C#
var hydraSdk = HydraIoc.Container.Resolve<IHydraSdk>();

var authMethod = AuthMethod.Firebase(token);
var result = await hydraSdk.Login(authMethod);

if (!loginResponse.IsSuccess)
{
    // Handle login error
}
else
{
    // Handle successful login 
}
```
  
# Connection #

To be able to successfully connect to the VPN server, you need to execute next three steps:
1. Bootstrap hydra SDK
2. Perform login
3. Call `StartVpn` method

An example of connect implementation:

```C#
var bootstrapper = new HydraWindowsBootstrapper();

var backendConfiguration = new BackendServerConfiguration(CarrierId, VpnServerUrl, DeviceId);
var hydraConfiguration = new HydraWindowsConfiguration(ServiceName);

bootstrapper.Bootstrap(backendConfiguration, hydraConfiguration);

var hydraSdk = HydraIoc.Container.Resolve<IHydraSdk>()

var authMethod = AuthMethod.Firebase(token);
var result = await hydraSdk.Login(authMethod);

if (!loginResponse.IsSuccess)
{
    // Handle login error
}
else
{
    hydraSdk.ConnectedChanged += (sender, args) => Console.WriteLine($"Connected: {args.Connected}");
    hydraSdk.StartVpn();
}
```
  
# SDK API #

### Interface IHydraSdk ###

This is core interface to work with user account and to control vpn connection.

**Methods**

|Method                                                                          |Description                             |
|--------------------------------------------------------------------------------|----------------------------------------|
|Task<PostLoginResponse> Login(AuthMethod authMethod)                            |Login user using supplied AuthMethod    |
|Task<GetLogoutResponse> Logout()                                                |Logout current user                     |
|Task<GetCountriesResponse> GetCountries()                                       |Get list of countries                   |
|Task<GetCurrentUserResponse> GetCurrentUser()                                   |Get current logged in user information  |
|Task<GetTCountersResponse> GetTrafficCounters(string countryCode = null)        |Get traffic counters for current user   |
|Task<GetRemainingTrafficResponse> GetRemainingTraffic()                         |Gets remaining traffic for current user |
|Task<PostPurchaseResponse> Purchase(string type, string token)                  |Post purchase to the backend server     |
|Task<bool> StartVpn(string countryCode = null)                                  |Start VPN connection                    |
|Task StopVpn()                                                                  |Stop VPN connection                     |

**Properties**

|Property                          |Description                      |
|----------------------------------|---------------------------------|
|bool IsLoggedIn { get; }          |"User is logged in" flag         |
|bool IsConnected { get; }         |VPN connected flag               |
|VpnStatistics Statistics { get; } |Statistics of the VPN connection |

**Events**

|Event                                                               |Description                  |
|--------------------------------------------------------------------|-----------------------------|
|event EventHandler<VpnConnectedChangedEventArgs> ConnectedChanged   |VPN connected changed event  |
|event EventHandler<VpnStatisticsChangedEventArgs> StatisticsChanged |VPN statistics changed event |

### Class AuthMethod ###

Data class for OAuth token and provider name. There is no public constructor, you can use factory methods instead. For anonymous login use Anonymous(). For popular OAuth providers like Facebook, Twitter and other use named factories Facebook(token), Twitter(token) etc. For other providers use OAuth(token) or Custom(customMethodName, token).

**Static methods**

|Method                                                   |Description                                                      |
|---------------------------------------------------------|-----------------------------------------------------------------|
|AuthMethod Anonymous()                                   |Creates new instance of the "Anonymous" authentication method    |
|AuthMethod OAuth(string token)                           |Creates new instance of the "Custom OAuth" authentication method |
|AuthMethod Live(string token)                            |Creates new instance of the "Live" authentication method         |
|AuthMethod Google(string token)                          |Creates new instance of the "Google" authentication method       |
|AuthMethod Twitter(string token)                         |Creates new instance of the "Twitter" authentication method      |
|AuthMethod Facebook(string token)                        |Creates new instance of the "Facebook" authentication method     |
|AuthMethod Firebase(string token)                        |Creates new instance of the "Firebase" authentication method     |
|AuthMethod GitHub(string token)                          |Creates new instance of the "GitHub" authentication method       |
|AuthMethod Custom(string token, string customMethodName) |Creates new instance of your custom authentication method        |

**Properties**

|Property                             |Description                                                 |
|-------------------------------------|------------------------------------------------------------|
|AuthenticationMethod Method { get; } |Concrete authentication method                              |
|string Token { get; }                |Authentication token if necessary (most likely OAuth token) |

### Enum ResponseResult ###

Enumeration for backend API response results. 

**Values**

|Value               |Description                                                  |
|--------------------|-------------------------------------------------------------|
|None                |Default response result                                      |
|Ok                  |Successful response result                                   |
|NotAuthorized       |Request could not complete because authorization is required |
|DevicesExceeded     |Available devices count exceeded                             |
|SessionsExceeded    |Available sessions count exceeded                            |
|NotResponding       |Backend API server is not responding                         |
|ServerUnavailable   |Backend API server is unavailable                            |
|UserSuspended       |User was deleted during active session                       |
|TrafficExceeded     |Available traffic limit exceeded                             |
|InternalServerError |Internal server error occured while processing request       |
|OAuthError          |Authentication error occurred through OAuth-server           |
|Invalid             |Password and username doesn’t match records                  |

### Class BaseApiResult ###

Base abstract class for all backend API responses.

**Properties**

|Property                              |Description                             |
|--------------------------------------|----------------------------------------|
|bool IsSuccess { get; private set; }  |Successful request completion flag      |
|string Error { get; private set; }    |Request error (if present)              |
|string RawResult { get; set; }        |Raw request result                      |
|ResponseResult Result { get; set; }   |One of predefined response results enum |

### Class PostLoginResponse ###

Data class for `Login` method response, `BaseApiResult` derivative.

**Properties**

|Property                              |Description                             |
|--------------------------------------|----------------------------------------|
|string AccessToken { get; set; }      |Access token for other API methods      |
|string Name { get; set; }             |Logged in user name                     |
|string DevicesLimit { get; set; }     |Available devices limit                 |
|string ActivatedDevices { get; set; } |Activated devices count                 |
|string ActiveSessions { get; set; }   |Active sessions count                   |
|string SessionsLimit { get; set; }    |Available sessions limit                |
|string UserId { get; set; }           |Logged in user id                       |

### Class GetLogoutResponse ###

Data class for `Logout` method response, `BaseApiResult` derivative. Does not define any properties or methods.

### Class GetCountriesResponse ###

Data class for `GetCountries` method response, `BaseApiResult` derivative.

**Properties**

|Property                                          |Description                     |
|--------------------------------------------------|--------------------------------|
|List<VpnServerCountry> VpnCountries { get; set; } |Available server of countries list |

### Class GetCurrentUserResponse ###

Data class for `GetCurrentUser` method response, `BaseApiResult` derivative.

**Properties**

|Property                            |Description            |
|------------------------------------|-----------------------|
|Subscriber Subscriber { get; set; } |Subscriber information |

### Class GetCountersResponse ###

Data class for `GetTrafficCounters` method response, `BaseApiResult` derivative.

**Properties**

|Property              |Description             |
|----------------------|------------------------|
|long Rx { get; set; } |Received bytes count    |
|long Tx { get; set; } |Transmitted bytes count |

### Class GetRemainingTrafficResponse ###

Data class for `GetRemainingTraffic` method response, `BaseApiResult` derivative.

**Properties**

|Property                            |Description                     |
|------------------------------------|--------------------------------|
|bool IsUnlimited { get; set; }      |Unlimited traffic flag          |
|long TrafficStart { get; set; }     |Beginning session time          |
|long TrafficLimit { get; set; }     |Current traffic limit (bytes)   |
|long TrafficUsed { get; set; }      |Used traffic value (bytes)      |
|long TrafficRemaining { get; set; } |Remaining traffic value (bytes) |

### Class PostPurchaseResponse ###

Data class for `Purchase` method response, `BaseApiResult` derivative.

**Properties**

|Property                       |Description              |
|-------------------------------|-------------------------|
|bool IsPurchased { get; set; } |Successful purchase flag |

### Class VpnStatistics ###

Data class for `Purchase` method response, `BaseApiResult` derivative.

**Properties**

|Property                       |Description              |
|-------------------------------|-------------------------|
|bool IsPurchased { get; set; } |Successful purchase flag |

### Class VpnServerCountry ###

Data class to store information about available country and servers.

**Properties**

|Property                     |Description    |
|-----------------------------|---------------|
|string Country { get; set; } |Server country |
|int Servers { get; set; }    |Servers count  |

### Class Subscriber ###

Data class to store information about subscriber.

**Properties**

|Property                           |Description                                          |
|-----------------------------------|-----------------------------------------------------|
|long Id { get; }                   |User Id                                              |
|string Name { get; }               |User name                                            |
|Bundle Bundle { get; }             |License information                                  |
|int ActivatedDevices { get; }      |Number of activated devices                          |
|int ActiveSessions { get; }        |Number of active sessions                            |
|string Locale { get; }             |Code of system's language (can be null)              |
|string CarrierId { get; }          |Hardcoded value of carrier that is used by device    |
|int Condition { get; }             |Represent value 0/1 which enable/disable subscriber  |
|DateTime RegistrationTime { get; } |Registration time                                    |
|DateTime ConnectionTime { get; }   |Connection time                                      |
|Extra Extra { get; }               |Extra information                                    |
|Social Social { get; }             |Information about social profile                     |
|string ExtRef { get; }             |External reference                                   |

### Class Bundle ###

Data class to store information about license.

**Properties**

|Property                   |Description           |
|---------------------------|----------------------|
|long Id { get; }           |Bundle Id             |
|string Name { get; }       |Bundle name           |
|int DevicesLimit { get; }  |Bundle devices limit  |
|int SessionsLimit { get; } |Bundle sessions limit |

### Class Extra ###

Data class to store extra information about subscriber. Reserved for future use.

### Class Social ###

Data class to store information about subscriber's social profile.

**Properties**

|Property              |Description             |
|----------------------|------------------------|
|string Email { get; } |Email of the subscriber |

### Class VpnStatistics ###

Data class to store information about VPN connection statistics.

**Properties**

|Property                         |Description          |
|---------------------------------|---------------------|
|long BytesReceived { get; set; } |Received bytes count |
|long BytesSent { get; set; }     |Sent bytes count     |

### Class VpnStatisticsChangedEventArgs ###

Data class to store information about VPN connection statistics changed event.

**Properties**

|Property                    |Description                       |
|----------------------------|----------------------------------|
|VpnStatistics Data { get; } |Current VPN connection statistics |

### Class VpnConnectedChangedEventArgs ###

Data class to store information about VPN connection state changed event.

**Properties**

|Property                |Description        |
|------------------------|-------------------|
|bool Connected { get; } |VPN connected flag |

### Handling Errors ###

There are several types of exceptions which may be generated during API methods request. `HydraException` is a base class for other sdk exceptions:

|Type                           |Description                                   |
|-------------------------------|----------------------------------------------|
|AuthorizationRequiredException |"User must be logged in to proceed" exception |
|BackendApiException            |Backend API related exception                 |
|NotConnectedException          |"Not connected to the VPN" exception          |

### Class HydraException ###

Base exception class of SDK

### Class AuthorizationRequiredException ###

Exception is thrown while executing operation when user is required to be logged in, e.g., while executing `GetCountries` method before `Login` or after `Logout`.

### Class BackendApiException ###

Exception is thrown when backend API returns response result that differs from `ResponseResult.Ok`.

**Properties**

|Property                               |Description                 |
|---------------------------------------|----------------------------|
|ResponseResult ResponseResult { get; } |Backend API response result |

### Class NotConnectedException ###

Exception is thrown while executing `StopVpn` method when VPN is not connected (i.e., `StartVpn` method was not called).

# Setting up logging #
 
Key places of Hydra SDK are instrumented with logging for debug purposes. If you need to see those log messages, you have to provide your own `LoggerListener`.

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

 

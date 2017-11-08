# Hydra VPN Windows SDK demo application #

GitHub project: https://github.com/AnchorFreePartner/hydra-vpn-demo-windows
This repository contains demo application which demonstrates usage of Hydra VPN Windows SDK.

# Requirements #

This project is based on default Microsoft Visual Studio build process.
Hydra VPN Windows SDK requires **.Net Framework 4.5**. Demo application requires **Microsoft Visual Studio 2017**.
Demo application installs TAP driver and Windows service at startup. If something goes wrong run `tap\[32bit|64bit]\install-tap.bat` as Administrator to install TAP driver. Use `service\install.bat` and `service\uninstall.bat` when you need to manage Windows service manually.

# Adding SDK to project #

1. Put the SDK binaries in a suitable place
2. Reference SDK binaries
3. Add `Unity` NuGet package reference ([Unity at www.nuget.org](https://www.nuget.org/packages/Unity/))
4. Make sure your project is targeting at least .Net Framework 4.5 

Now you're all set.

# TAP driver installation #  

To install TAP driver you need to go to the `tap` folder and execute `install-tap.bat`  from `32bit` folder for Windows x86 or from `64bit` folder for Windows x64 as Administrator.
  
# Windows service installation #

To be able to use the hydra service you need to install it into the system once. It could be done by following command:

```
Hydra.Sdk.Windows.Service.exe -install <serviceName>
``` 

where `serviceName` is the name of your service.

# Usage and core interfaces #

SDK contains one core interface - `IHydraSdk`.

### IHydraSdk ###

To be able to work with hydra sdk, you need to bootstrap hydra by providing valid backend server configuration and hydra configuration. This could be done by using following code snippet (you need to reference `Hydra.Sdk.Windows`, `Hydra.Sdk.Common` and `Hydra.Sdk.Backend` assemblies):

```C#
var bootstrapper = new HydraWindowsBootstrapper();

var backendConfiguration = new BackendServerConfiguration(CarrierId, VpnServerUrl, DeviceId);
var hydraConfiguration = new HydraWindowsConfiguration(ServiceName);

bootstrapper.Bootstrap(backendConfiguration, hydraConfiguration);
``` 

If you need to specify bypass domains or disable automatic reconnection after coming back from sleep mode, set corresponding hydra configuration properties before bootstrapping:

```C#
var hydraConfiguration = new HydraWindowsConfiguration(ServiceName)
    .AddBypassDomains(BypassDomainsList)
    .SetReconnectOnWakeUp(ReconnectOnWakeUp);
```

After bootstrapping you can get the backend service instance by simply resolving `IHydraSdk` from IoC container:

```C#
var hydraSdk = HydraIoc.Container.Resolve<IHydraSdk>();
```

### Login example ###

Login process requires OAuth Access Token and Authentication Method. This example uses Anonymous and GitHub for demonstration. Do not forget to check whether the request was successful:

```C#
var authMethod = AuthMethod.Firebase(token);
var result = await hydraSdk.Login(authMethod);

if (!loginResponse.IsSuccess || loginResponse.Result != ResponseResult.Ok)
{
    // Handle login error
}
else
{
    // Handle successful login 
}
```

After successful login you can execute other methods which requires to be logged in. 

### Connect example ###

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

if (!loginResponse.IsSuccess || loginResponse.Result != ResponseResult.Ok)
{
    // Handle login error
}
else
{
    hydraSdk.ConnectedChanged += (sender, args) => Console.WriteLine($"Connected: {args.Connected}");
    hydraSdk.StartVpn();
}
```

Disconnect from VPN with:

```C#
await hydraSdk.StopVpn();
``` 

# Change country #

Getting available countries list:

```C#
// Get available countries
var countriesResponse = await hydraSdk.GetCountries();

// Check whether request was successful
if (!countriesResponse.IsSuccess || loginResponse.Result != ResponseResult.Ok)
{
    // Handle request error
}

// Get countries from response
var countries = countriesResponse.VpnCountries;

// Proceed with countries
```

`GetCountries` response contains list of `VpnServerCountry` items. Response must also be checked for OK status. `VpnServerCountry` contains information about available Country and Servers, and is used to specify the required country for VPN connection.

# Check remaining traffic limit #

User can check remaining traffic limit if it is set:

```C#
var remainingTrafficResponseResult = await hydraSdk.GetRemainingTraffic();
```

`GetRemainingTrafficResponse` contains information about:

* Is unlimited - connection to unlimited flag
* Traffic start - beginning session time
* Traffic limit - limit for traffic usage in bytes
* Traffic used - used traffic for subscriber
* Traffic remaining - remaining traffic in bytes

# OAuth or Anonymous authorization #

This example application uses two types of client authorization: with OAuth token and
Anonymous.

Usage:

```C#
var result = await hydraSdk.Login(authMethod);

if (!loginResponse.IsSuccess || loginResponse.Result != ResponseResult.Ok)
{
    // Handle login error
}
else
{
    // Handle successful login 
}
```

- `AuthMethod` - one of the valid authentication methods:
  * GitHub, Facebook, Twitter, Firebase, Live, Google - for public authentication servers
  * OAuth - for custom authentication server
  * Custom - for custom authentication scheme
  * Anonymous - for anonymous authentication
- `DeviceId` - unique device id
- `MachineName` - name of your machine
- `DeviceType` - Desktop or Mobile
- `OAuthToken` - valid token from OAuth server or `null` for Anonymous

Log out user with:

```C#
var logoutResponse = await hydraSdk.Logout();
```

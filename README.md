# Hydra VPN Windows SDK demo application #

This repository contains demo application which demonstrates usage of Hydra VPN Windows SDK.

# Requirements #

This project is based on default Microsoft Visual Studio build process.
Hydra VPN Windows SDK requires **.Net Framework 4.5**. Demo application requires **Microsoft Visual Studio 2017**.
Demo application installs TAP driver and Windows service at startup, if something went wrong run `tap\[32bit|64bit]\install-tap.bat` as Administrator to install TAP driver. Use `service\install.bat` and `service\uninstall.bat` when you need to manage Windows service manually.

# Adding SDK to project #

1. Put the SDK binaries in a suitable place
2. Reference SDK binaries
3. Add `Unity` NuGet package reference ([Unity at www.nuget.org](https://www.nuget.org/packages/Unity/))
4. Make sure your project is targeting at least .Net Framework 4.5 

Now you're all set.

# Usage and core interfaces #

SDK contains two core interfaces:

1. `IPartnerBackendService`
2. `IHydraVpnClient`

### IPartnerBackendService ###

This interface manages client user authentication, vpn credentials retrieval, user licensing info,
session management.
Session will be saved after first successful sign in and destroyed and cleaned up after logout.

To be able to work with backend service, you need to bootstrap backend module by providing valid ***Carrier ID*** and ***VPN server URL***. This could be done by using following code snippet (you need to reference `Hydra.Sdk.Common` and `Hydra.Sdk.Backend` assemblies):

```C#
var backendServerConfiguration = new BackendServerConfiguration(carrierId, vpnServerUrl);
var hydraBackendBootstrapper = new HydraBackendBootstrapper(backendServerConfiguration);
hydraBackendBootstrapper.Bootstrap();
``` 

After bootstrapping you can get the backend service instance by simply resolving `IPartnerBackendService` from IoC container:

```C#
var vpnServerService = HydraIoc.Container.Resolve<IPartnerBackendService>();
```

Login process requires OAuth Access Token and Authentication Method.
This example uses Anonymous and GitHub for demonstration.

```C#
var loginParam = new LoginParam(
            AuthenticationMethod.Anonymous,
            DeviceId, // Your device id
            Environment.MachineName,
            DeviceType.Desktop,
            string.Empty // Your OAuth Access Token if necessary
			);

var loginResponse = await vpnServerService.LoginAsync(loginParam);
```

Do not forget to check whether the request was successful:

```C#
if (!loginResponse.IsSuccess || loginResponse.Result != ResponseResult.Ok)
{
    // Handle unsuccessful request
}
```

After successful login you can execute other methods with `AccessToken` from the login response. For example, get credentials:

```C#
var credentialsParam = new GetCredentialsParam(accessToken);

var credentialsResponse = await vpnServerService.GetCredentialsAsync(credentialsParam);
```

### IHydraVpnClient ###

This interface manages VPN connection.

To be able to work with VPN client, first you need to bootstrap VPN module. It could be done by using following code snippet (you need to reference `Hydra.Sdk.Common` and `Hydra.Sdk.Vpn` assemblies):

```C#
var hydraVpnBootstrapper = new HydraVpnBootstrapper();
hydraVpnBootstrapper.Bootstrap();
``` 

If you want to use service-based approach, reference `Hydra.Sdk.Windows` assembly and use `HydraWindowsBootstrapper` instead of `HydraVpnBootstrapper`.

After bootstrapping you can get the VPN client instance by simply resolving `IHydraVpnClient` from IoC container:

```C#
var vpnClient = HydraIoc.Container.Resolve<IHydraVpnClient>();
```

Then you can connect to VPN with Connect() call:

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

Disconnect from VPN with:

```C#
await vpnClient.Disconnect();
``` 

# Change country #

Getting available countries list from `IPartnerBackendService`:

```C#
var accessToken = loginResponse.AccessToken;
var getCountriesResult = await vpnServerService.GetCountriesAsync(accessToken);
```

`GetCountriesAsync` response contains list of `VpnServerCountry` items. Response must also be checked for OK status. `VpnServerCountry` contains available information - Country and Servers count and is used to specify the required country when getting VPN credentials:

```C#
var vpnServerCountry = getCountriesResult.VpnCountries.First();
var country = vpnServerCountry.Country;
var credentialsParam = new GetCredentialsParam(accessToken, country);

var credentialsResponse = await vpnServerService.GetCredentialsAsync(credentialsParam);
```

# Check remaining traffic limit #

User can check remaining traffic limit if it is set:

```C#
var remainingTrafficResponseResult = await partnerBackendService.GetRemainingTrafficAsync(new GetRemaningTrafficParam (this.AccessToken));
```

`GetRemainingTrafficResponse` contains information about:

* Is unlimited - is connection unlimited flag
* Traffic start - beggining session time
* Traffic limit - limit for traffic usage in bytes
* Traffic used - used traffic for subscriber
* Traffic remaining - remaining traffic in bytes traffic

# OAuth or Anonymous authorization #

This example application uses two types of client authorization: with OAuth token and
Anonymous.

Usage:

```C#
var loginResponse = await backendService.LoginAsync(
	new LoginParam
	(
		AuthenticationMethod,
		DeviceId,
		MachineName,
		DeviceType,
		OAuthToken
	));
```

- `AuthenticationMethod` - one of the valid authentication methods:
  * GitHub, Facebook, Twitter, Firebase, Live, Google - for public authentication servers,
  * OAuth - for custom authentication server,
  * Anonymous - for anonymous authentication.
- `DeviceId` - unique device id.
- `MachineName` - name of your machine.
- `DeviceType` - Desktop or Mobile.
- `OAuthToken` - valid token from OAuth server or `null` for Anonymous.

Log out user with:

```C#
var logoutResponse = await backendService.LogoutAsync(new LogoutRequestParam(this.AccessToken));
```

namespace Hydra.Sdk.Wpf.ViewModel.Control
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Windows.IoC;
    using Common.Logger;
    using Hydra.Sdk.Backend.IoC;
    using Hydra.Sdk.Backend.Misc;
    using Hydra.Sdk.Backend.Parameter;
    using Hydra.Sdk.Backend.Service;
    using Hydra.Sdk.Common.IoC;
    using Hydra.Sdk.Vpn.Config;
    using Hydra.Sdk.Vpn.Service;
    using Hydra.Sdk.Vpn.Service.EventsHandling;
    using Hydra.Sdk.Wpf.Helper;
    using Logger;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Prism.Commands;
    using Prism.Mvvm;
    using Vpn.IoC;

    /// <summary>
    /// Main screen view model.
    /// </summary>
    public class MainScreenViewModel : BindableBase
    {
        /// <summary>
        /// Machine GUID from registry.
        /// </summary>
        private static readonly string MachineId;

        /// <summary>
        /// Hydra VPN client instance.
        /// </summary>
        private IHydraVpnClient vpnClient;

        /// <summary>
        /// Device id for backend login method.
        /// </summary>
        private string deviceId;

        /// <summary>
        /// Carrier id for backend service.
        /// </summary>
        private string carrierId;

        /// <summary>
        /// Backend url for backend service.
        /// </summary>
        private string backendUrl;

        /// <summary>
        /// Country for backend get credentials method.
        /// </summary>
        private string country;

        /// <summary>
        /// Message which is disoplayed in case of errors.
        /// </summary>
        private string errorText;

        /// <summary>
        /// Access token for backend methods.
        /// </summary>
        private string accessToken;

        /// <summary>
        /// User password for hydra.
        /// </summary>
        private string password;

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        private string vpnIpServerServer;

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        private string vpnIp;

        /// <summary>
        /// Received bytes count.
        /// </summary>
        private string bytesReceived;

        /// <summary>
        /// Sent bytes count.
        /// </summary>
        private string bytesSent;

        /// <summary>
        /// VPN connection status.
        /// </summary>
        private string status;

        /// <summary>
        /// Remaining traffic response.
        /// </summary>
        private string remainingTrafficResponse;

        /// <summary>
        /// Error visibility flag.
        /// </summary>
        private bool isErrorVisible;

        /// <summary>
        /// Connect button visibility flag.
        /// </summary>
        private bool isConnectButtonVisible;

        /// <summary>
        /// Disconnect button visibility flag.
        /// </summary>
        private bool isDisconnectButtonVisible;        

        /// <summary>
        /// Connect command.
        /// </summary>
        private ICommand connectCommand;

        /// <summary>
        /// Disconnect command.
        /// </summary>
        private ICommand disconnectCommand;

        /// <summary>
        /// Clear log command.
        /// </summary>
        private ICommand clearLogCommand;

        /// <summary>
        /// Current VPN credentials.
        /// </summary>
        private VpnCredentials vpnCredentials;

        /// <summary>
        /// Timer to update remaining traffic information.
        /// </summary>
        private DispatcherTimer dispatcherTimer;

        /// <summary>
        /// Use service flag.
        /// </summary>
        private bool useService;

        /// <summary>
        /// Name of windows service to use to establish hydra connection.
        /// </summary>
        private string serviceName;

        /// <summary>
        /// Hydra log contents.
        /// </summary>
        private string logContents;

        /// <summary>
        /// Countries list.
        /// </summary>
        private IEnumerable<string> countriesList;

        /// <summary>
        /// Use GitHub authorization flag.
        /// </summary>
        private bool useGithubAuthorization;

        /// <summary>
        /// Login command.
        /// </summary>
        private ICommand loginCommand;

        /// <summary>
        /// Logout command
        /// </summary>
        private ICommand logoutCommand;

        /// <summary>
        /// Login button visibility flag.
        /// </summary>
        private bool isLoginButtonVisible;

        /// <summary>
        /// Logout button visibility flag.
        /// </summary>
        private bool isLogoutButtonVisible;

        /// <summary>
        /// Logged in flag.
        /// </summary>
        private bool isLoggedIn;

        /// <summary>
        /// Requested country to connect.
        /// </summary>
        private string requestedCountry;

        /// <summary>
        /// GitHub login.
        /// </summary>
        private string gitHubLogin;

        /// <summary>
        /// GitHub password.
        /// </summary>
        private string gitHubPassword;

        /// <summary>
        /// <see cref="MainScreenViewModel"/> static constructor. Performs <see cref="MachineId"/> initialization.
        /// </summary>
        static MainScreenViewModel()
        {
            MachineId = RegistryHelper.GetMachineGuid();
        }

        /// <summary>
        /// <see cref="MainScreenViewModel"/> default constructor.
        /// </summary>
        public MainScreenViewModel()
        {
            // Bootstrap VPN and initialize events
            this.BootstrapVpn();

            // Init view model
            var dateTime = DateTime.Now;
            this.DeviceId = $"{MachineId}-{dateTime:dd-MM-yy}";
            this.CarrierId = "touchvpn";
            this.BackendUrl = "https://backend.northghost.com";
            this.IsConnectButtonVisible = false;
            this.SetStatusDisconnected();
            this.SetStatusLoggedOut();

            // Init remaining traffic timer
            this.InitializeTimer();

            // Init logging
            this.InitializeLogging();

            // Init predefined carriers and countries
            this.InitializeCarriers();
            this.InitializeCountriesList();
        }

        /// <summary>
        /// Performs predefined carriers initialization.
        /// </summary>
        private void InitializeCarriers()
        {
            this.CarrierId = "afdemo";
        }

        /// <summary>
        /// Performs countries list initialization.
        /// </summary>
        private void InitializeCountriesList()
        {
            this.CountriesList = new[] { "" };
        }

        /// <summary>
        /// Performs logging initialization.
        /// </summary>
        private void InitializeLogging()
        {
            var loggerListener = new EventLoggerListener();
            loggerListener.LogEntryArrived += (sender, args) => AddLogEntry(args.Entry);
            HydraLogger.AddHandler(loggerListener);
        }

        /// <summary>
        /// Adds new log entry to the log contents.
        /// </summary>
        /// <param name="logEntry">Log entry to add.</param>
        private void AddLogEntry(string logEntry)
        {
            if (string.IsNullOrWhiteSpace(this.LogContents))
            {
                this.LogContents = string.Empty;
            }

            this.LogContents += logEntry + Environment.NewLine;
        }

        /// <summary>
        /// Device id for backend login method.
        /// </summary>
        public string DeviceId
        {
            get => this.deviceId;
            set => this.SetProperty(ref this.deviceId, value);
        }

        /// <summary>
        /// Carrier id for backend service.
        /// </summary>
        public string CarrierId
        {
            get => this.carrierId;
            set => this.SetProperty(ref this.carrierId, value);
        }

        /// <summary>
        /// Backend url for backend service.
        /// </summary>
        public string BackendUrl
        {
            get => this.backendUrl;
            set => this.SetProperty(ref this.backendUrl, value);
        }

        /// <summary>
        /// Message which is disoplayed in case of errors.
        /// </summary>
        public string ErrorText
        {
            get => this.errorText;
            set => this.SetProperty(ref this.errorText, value);
        }

        /// <summary>
        /// Access token for backend methods.
        /// </summary>
        public string AccessToken
        {
            get => this.accessToken;
            set => this.SetProperty(ref this.accessToken, value);
        }

        /// <summary>
        /// User password for hydra.
        /// </summary>
        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        public string VpnIpServer
        {
            get => this.vpnIpServerServer;
            set => this.SetProperty(ref this.vpnIpServerServer, value);
        }

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        public string VpnIp
        {
            get => this.vpnIp;
            set => this.SetProperty(ref this.vpnIp, value);
        }

        /// <summary>
        /// Remaining traffic response.
        /// </summary>
        public string RemainingTrafficResponse
        {
            get => this.remainingTrafficResponse;
            set => this.SetProperty(ref this.remainingTrafficResponse, value);
        }

        /// <summary>
        /// Error visibility flag.
        /// </summary>
        public bool IsErrorVisible
        {
            get => this.isErrorVisible;
            set => this.SetProperty(ref this.isErrorVisible, value);
        }

        /// <summary>
        /// Connect button visibility flag.
        /// </summary>
        public bool IsConnectButtonVisible
        {
            get => this.isConnectButtonVisible;
            set => this.SetProperty(ref this.isConnectButtonVisible, value);
        }

        /// <summary>
        /// Disconnect button visibility flag.
        /// </summary>
        public bool IsDisconnectButtonVisible
        {
            get => this.isDisconnectButtonVisible;
            set => this.SetProperty(ref this.isDisconnectButtonVisible, value);
        }

        /// <summary>
        /// Received bytes count.
        /// </summary>
        public string BytesReceived
        {
            get => this.bytesReceived;
            set => this.SetProperty(ref this.bytesReceived, value);
        }

        /// <summary>
        /// Sent bytes count.
        /// </summary>
        public string BytesSent
        {
            get => this.bytesSent;
            set => this.SetProperty(ref this.bytesSent, value);
        }

        /// <summary>
        /// VPN connection status.
        /// </summary>
        public string Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        /// <summary>
        /// Country for backend get credentials method.
        /// </summary>
        public string Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        /// <summary>
        /// Use service flag.
        /// </summary>
        public bool UseService
        {
            get => this.useService;
            set => this.SetProperty(ref this.useService, value);
        }

        /// <summary>
        /// Name of windows service to use to establish hydra connection.
        /// </summary>
        public string ServiceName
        {
            get => this.serviceName;
            set => this.SetProperty(ref this.serviceName, value);
        }

        /// <summary>
        /// Logging enabled flag.
        /// </summary>
        public bool IsLoggingEnabled
        {
            get => HydraLogger.IsEnabled;
            set => HydraLogger.IsEnabled = value;
        }

        /// <summary>
        /// Hydra log contents.
        /// </summary>
        public string LogContents
        {
            get => this.logContents;
            set => this.SetProperty(ref this.logContents, value);
        }

        /// <summary>
        /// Countries list.
        /// </summary>
        public IEnumerable<string> CountriesList
        {
            get => this.countriesList;
            set => this.SetProperty(ref this.countriesList, value);
        }

        /// <summary>
        /// Use GitHub authorization flag.
        /// </summary>
        public bool UseGithubAuthorization
        {
            get => this.useGithubAuthorization;
            set
            {
                this.SetProperty(ref this.useGithubAuthorization, value);
                this.RaisePropertyChanged(nameof(IsGithubCredentialsEnabled));
            }
        }

        /// <summary>
        /// Login button visibility flag.
        /// </summary>
        public bool IsLoginButtonVisible
        {
            get => this.isLoginButtonVisible;
            set => this.SetProperty(ref this.isLoginButtonVisible, value);
        }

        /// <summary>
        /// Logout button visibility flag.
        /// </summary>
        public bool IsLogoutButtonVisible
        {
            get => this.isLogoutButtonVisible;
            set => this.SetProperty(ref this.isLogoutButtonVisible, value);
        }

        /// <summary>
        /// Logged in flag.
        /// </summary>
        public bool IsLoggedIn
        {
            get => this.isLoggedIn;
            set
            {
                this.SetProperty(ref this.isLoggedIn, value);
                this.RaisePropertyChanged(nameof(IsLoggedOut));
                this.RaisePropertyChanged(nameof(IsGithubCredentialsEnabled));
            }
        }

        /// <summary>
        /// Logged out flag.
        /// </summary>
        public bool IsLoggedOut => !this.isLoggedIn;

        /// <summary>
        /// GitHub credentials enabled flag.
        /// </summary>
        public bool IsGithubCredentialsEnabled => UseGithubAuthorization && IsLoggedOut;

        /// <summary>
        /// GitHub login.
        /// </summary>
        public string GitHubLogin
        {
            get => this.gitHubLogin;
            set => this.SetProperty(ref this.gitHubLogin, value);
        }

        /// <summary>
        /// GitHub password.
        /// </summary>
        public string GitHubPassword
        {
            get => this.gitHubPassword;
            set => this.SetProperty(ref this.gitHubPassword, value);
        }

        /// <summary>
        /// Connect command.
        /// </summary>
        public ICommand ConnectCommand => this.connectCommand ?? (this.connectCommand = new DelegateCommand(this.Connect));

        /// <summary>
        /// Disconnect command.
        /// </summary>
        public ICommand DisconnectCommand => this.disconnectCommand ?? (this.disconnectCommand = new DelegateCommand(this.Disconnect));

        /// <summary>
        /// Clear log command.
        /// </summary>
        public ICommand ClearLogCommand => this.clearLogCommand ??
                                           (this.clearLogCommand = new DelegateCommand(this.ClearLog));

        public ICommand LoginCommand => this.loginCommand ?? (this.loginCommand = new DelegateCommand(this.Login));

        public ICommand LogoutCommand => this.logoutCommand ?? (this.logoutCommand = new DelegateCommand(this.Logout));

        private async Task<string> GetGithubOAuthToken(string login, string password)
        {
            const string apiUrl = "https://api.github.com/authorizations";
            const string clientId = "70ed6ffd4b08b3119208";
            const string clientSecret = "fe02229ef77aa489f748f346e3e337490fd5b8ce";

            var authString = Convert.ToBase64String(Encoding.Default.GetBytes($"{login}:{password}"));
            var parameters = new
            {
                scopes = new string[]{ },
                client_id = clientId,
                client_secret = clientSecret
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", login);

                using (var message = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                {
                    message.Headers.Accept.ParseAdd("application/json");
                    message.Headers.Authorization = AuthenticationHeaderValue.Parse($"Basic {authString}");                    

                    var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
                    message.Content = content;

                    try
                    {
                        HydraLogger.Trace("Trying to get OAuth token from GitHub...");
                        var response = await client.SendAsync(message);
                        if (!response.IsSuccessStatusCode)
                        {
                            HydraLogger.Trace("Unable to get OAuth token from GitHub!");
                            return string.Empty;
                        }

                        HydraLogger.Trace("Got valid response from GitHub");
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseJson = JObject.Parse(responseString);

                        return responseJson["token"].ToObject<string>();
                    }
                    catch 
                    {
                        return string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Performs login to the backend server.
        /// </summary>
        private async void Login()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsConnectButtonVisible = false;
                this.IsLoginButtonVisible = false;
                this.vpnCredentials = null;

                // Perform logout
                await LogoutHelper.Logout();

                // Get GitHub OAuth token if necessary
                string oauthToken = string.Empty;
                if (UseGithubAuthorization)
                {
                    oauthToken = await GetGithubOAuthToken(GitHubLogin, GitHubPassword);
                    if (string.IsNullOrWhiteSpace(oauthToken))
                    {
                        MessageBox.Show("Could not perform GitHub authorization!", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }

                // Bootstrap hydra backend with provided CarrierId and BackendUrl
                var hydraBackendBootstrapper = new HydraBackendBootstrapper(new BackendServerConfiguration(this.CarrierId, this.BackendUrl));
                hydraBackendBootstrapper.Bootstrap();

                // Resolve backend service and perform login
                var backendService = HydraIoc.Container.Resolve<IPartnerBackendService>();
                var loginResponse = await backendService.LoginAsync(
                    new LoginParam
                    (
                        this.UseGithubAuthorization ? AuthenticationMethod.GitHub : AuthenticationMethod.Anonymous,
                        this.DeviceId,
                        Environment.MachineName,
                        DeviceType.Desktop,
                        this.UseGithubAuthorization ? oauthToken : string.Empty
                    ));

                // Check whether login was successful
                if (!loginResponse.IsSuccess)
                {
                    this.IsLoginButtonVisible = true;
                    this.ErrorText = loginResponse.Error ?? loginResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Remember access token for later usages
                LogoutHelper.AccessToken = loginResponse.AccessToken;
                this.AccessToken = loginResponse.AccessToken;

                this.UpdateCountries();

                // Work with UI
                this.IsConnectButtonVisible = true;
                this.SetStatusLoggedIn();

                // Update remaining traffic
                await this.UpdateRemainingTraffic();
            }
            catch (Exception e)
            {
                // Show error when exception occured
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
                this.IsLoginButtonVisible = true;
            }
        }

        /// <summary>
        /// Update available countries list.
        /// </summary>
        private async void UpdateCountries()
        {
            // Get backend service reference
            var backendService = HydraIoc.Container.Resolve<IPartnerBackendService>();
            var countriesResponse = await backendService.GetCountriesAsync(this.AccessToken);

            // Check whether request was successful
            if (!countriesResponse.IsSuccess)
            {
                this.IsLoginButtonVisible = true;
                this.ErrorText = countriesResponse.Error ?? countriesResponse.Result.ToString();
                this.IsErrorVisible = true;
                return;
            }

            // Get countries from response
            var countries = countriesResponse.VpnCountries.Select(x => x.Country).ToList();
            countries.Insert(0, "");

            // Remember countries
            this.CountriesList = countries;
        }

        private async void Logout()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsConnectButtonVisible = false;
                this.IsLogoutButtonVisible = false;
                this.vpnCredentials = null;
                this.requestedCountry = string.Empty;

                // Resolve backend service and perform logout
                var backendService = HydraIoc.Container.Resolve<IPartnerBackendService>();
                var logoutResponse = await backendService.LogoutAsync(new LogoutRequestParam(this.AccessToken));

                // Check whether logout was successful
                if (!logoutResponse.IsSuccess)
                {
                    this.IsLogoutButtonVisible = true;
                    this.ErrorText = logoutResponse.Error ?? logoutResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Erase access token and other related properties
                LogoutHelper.AccessToken = string.Empty;
                this.AccessToken = string.Empty;
                this.VpnIp = string.Empty;
                this.Password = string.Empty;
                this.RemainingTrafficResponse = String.Empty;

                // Work with UI
                this.IsConnectButtonVisible = true;
                this.InitializeCountriesList();
                this.SetStatusLoggedOut();
            }
            catch (Exception e)
            {
                // Show error when exception occured
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
                this.isLogoutButtonVisible = true;
            }
        }

        /// <summary>
        /// Clears log contents.
        /// </summary>
        private void ClearLog()
        {
            this.LogContents = string.Empty;
        }

        /// <summary>
        /// Performs remaining traffic timer initialization.
        /// </summary>
        private void InitializeTimer()
        {
            this.dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            this.dispatcherTimer.Tick += this.DispatcherTimerOnTick;
            this.dispatcherTimer.Start();
        }

        /// <summary>
        /// Subscribes to VPN client events.
        /// </summary>
        private void InitializeEvents()
        {
            this.vpnClient.Connected += this.VpnClientOnConnected;
            this.vpnClient.Disconnected += this.VpnClientOnDisconnected;
            this.vpnClient.StatisticsChanged += this.VpnClientOnStatisticsChanged;
        }

        /// <summary>
        /// VPN client statistics changed event handler.
        /// </summary>
        /// <param name="sender">Sender (VPN client).</param>
        /// <param name="vpnStatisticsChangedEventArgs">Event arguments (bytes sent/received).</param>
        private void VpnClientOnStatisticsChanged(object sender, VpnStatisticsChangedEventArgs vpnStatisticsChangedEventArgs)
        {
            this.BytesReceived = vpnStatisticsChangedEventArgs.Data.BytesReceived.ToString();
            this.BytesSent = vpnStatisticsChangedEventArgs.Data.BytesSent.ToString();
        }

        /// <summary>
        /// VPN client disconnected event handler.
        /// </summary>
        private void VpnClientOnDisconnected(object o, VpnDisconnectedEventArgs vpnDisconnectedEventArgs)
        {
            this.SetStatusDisconnected();
        }

        /// <summary>
        /// VPN client connected event handler.
        /// </summary>
        private void VpnClientOnConnected(object o, VpnConnectedEventArgs vpnConnectedEventArgs)
        {
            this.Status = "Connected";
            this.IsConnectButtonVisible = false;
            this.IsDisconnectButtonVisible = true;
            this.IsLogoutButtonVisible = false;
        }

        /// <summary>
        /// Performs actions related to setting backend status to "Logged out"
        /// </summary>
        private void SetStatusLoggedOut()
        {
            this.IsLoginButtonVisible = true;
            this.IsLogoutButtonVisible = false;
            this.IsLoggedIn = false;
        }

        /// <summary>
        /// Performs actions related to setting backend status to "Logged in"
        /// </summary>
        private void SetStatusLoggedIn()
        {
            this.IsLoginButtonVisible = false;
            this.IsLogoutButtonVisible = true;
            this.IsLoggedIn = true;
        }

        /// <summary>
        /// Performs actions related to setting VPN status to "Disconnected".
        /// </summary>
        private void SetStatusDisconnected()
        {
            this.Status = "Disconnected";
            this.BytesReceived = "0";
            this.BytesSent = "0";
            this.IsDisconnectButtonVisible = false;
            this.IsConnectButtonVisible = true;
            this.IsLogoutButtonVisible = true;
        }

        /// <summary>
        /// Gets VPN credentinals.
        /// </summary>
        private async Task GetVpnCredentials()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsConnectButtonVisible = false;
                this.vpnCredentials = null;

                // Get credentials with AccessToken from Login response and provided Country
                var backendService = HydraIoc.Container.Resolve<IPartnerBackendService>();
                var credentialsResponse = await backendService.GetCredentialsAsync(
                    new GetCredentialsParam
                        (
                            this.AccessToken,
                            this.Country
                        ));

                // Check whether request was successful
                if (!credentialsResponse.IsSuccess)
                {
                    this.IsConnectButtonVisible = true;
                    this.ErrorText = credentialsResponse.Error ?? credentialsResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Remember VPN credentials
                this.Password = credentialsResponse.VpnCredentials.Password;
                this.VpnIp = credentialsResponse.VpnCredentials.Ip;
                this.VpnIpServer = credentialsResponse.VpnCredentials.Ip;
                this.vpnCredentials = credentialsResponse.VpnCredentials;
                this.IsConnectButtonVisible = true;

                // Update remaining traffic
                await this.UpdateRemainingTraffic();
            }
            catch (Exception e)
            {
                // Show error when exception occured
                this.IsConnectButtonVisible = true;
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }
        }

        /// <summary>
        /// Checks if Windows service with supplied name exists.
        /// </summary>
        /// <param name="name">Name of Windows service</param>
        /// <returns>true if Windows service with supplied name exists, otherwise false.</returns>
        private bool IsServiceExists(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            using (var controller = new ServiceController(name))
            {
                try
                {
                    var controllerStatus = controller.Status;
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Bootstraps VPN according to the selected parameters and initializes VPN events.
        /// </summary>
        private void BootstrapVpn()
        {
            if (this.UseService)
            {
                var hydraVpnBootstrapper = new HydraWindowsBootstrapper();
                hydraVpnBootstrapper.Bootstrap(this.ServiceName);
            }
            else
            {
                var hydraBootstrapper = new HydraVpnBootstrapper();
                hydraBootstrapper.Bootstrap();
            }

            this.vpnClient = HydraIoc.Container.Resolve<IHydraVpnClient>();
            InitializeEvents();
        }

        /// <summary>
        /// Performs VPN connection.
        /// </summary>
        private async void Connect()
        {
            // Get credentials if we need to
            if (this.vpnCredentials == null || this.requestedCountry != this.Country)
            {
                this.requestedCountry = this.Country;
                await GetVpnCredentials();
            }

            if (this.vpnCredentials == null)
            {
                // Error occured while receiving the credentials
                return;
            }

            // Check if we want to use Windows service and if service exists
            if (this.UseService && !this.IsServiceExists(this.ServiceName))
            {
                MessageBox.Show($"Could not locate service '{this.ServiceName}', please check whether it is installed.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Bootstrap VPN
            this.BootstrapVpn();

            // Connect VPN using provided VPN server IP and user hash
            await this.vpnClient.Connect(
                new HydraConfigParams { DestinationIp = this.VpnIp, UserHash = this.vpnCredentials?.Password });
        }

        /// <summary>
        /// Disconnects from VPN server.
        /// </summary>
        private async void Disconnect()
        {
            // Disconnect VPN
            await this.vpnClient.Disconnect();

            // Update UI
            this.SetStatusDisconnected();
        }

        /// <summary>
        /// Remaining traffic timer tick event handler.
        /// </summary>
        private async void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            // Exit if AccessToken is empty
            if (string.IsNullOrEmpty(this.AccessToken))
            {
                return;
            }

            // Update remaining traffic
            await this.UpdateRemainingTraffic();
        }

        /// <summary>
        /// Performs update of remaining traffic
        /// </summary>
        private async Task UpdateRemainingTraffic()
        {
            // Check if access token is not empty
            if (string.IsNullOrWhiteSpace(this.AccessToken))
            {
                return;
            }

            // Resolve backend service
            var partnerBackendService = HydraIoc.Container.Resolve<IPartnerBackendService>();

            // Get remaining traffic
            var remainingTrafficResponseResult = await partnerBackendService.GetRemainingTrafficAsync(new GetRemaningTrafficParam (this.AccessToken));

            // Check whether request was successful
            if (!remainingTrafficResponseResult.IsSuccess)
            {
                return;
            }

            // Update UI with response data
            this.RemainingTrafficResponse
                = remainingTrafficResponseResult.IsUnlimited
                      ? "Unlimited"
                      : $"Bytes remaining: {remainingTrafficResponseResult.TrafficRemaining}\nBytes used: {remainingTrafficResponseResult.TrafficUsed}";
        }
    }
}
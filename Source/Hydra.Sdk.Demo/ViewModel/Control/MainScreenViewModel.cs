// <copyright file="MainScreenViewModel.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Demo.ViewModel.Control
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Hydra.Sdk.Demo.Countries;
    using Hydra.Sdk.Demo.Helper;
    using Hydra.Sdk.Demo.Logger;
    using Hydra.Sdk.Demo.Model;
    using Hydra.Sdk.Demo.View;
    using Hydra.Sdk.Windows;
    using Hydra.Sdk.Windows.Enum;
    using Hydra.Sdk.Windows.EventArgs;
    using Hydra.Sdk.Windows.IoC;
    using Hydra.Sdk.Windows.Logger;
    using Hydra.Sdk.Windows.Misc;
    using Hydra.Sdk.Windows.Network.Rules;
    using Microsoft.Practices.ServiceLocation;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PartnerApi.Model.Nodes;
    using Prism.Commands;
    using Prism.Mvvm;

    /// <summary>
    /// Main screen view model.
    /// </summary>
    public class MainScreenViewModel : BindableBase
    {
        private static readonly string MachineId = RegistryHelper.GetMachineGuid();
        private ICommand connectCommand;
        private ICommand disconnectCommand;
        private ICommand clearLogCommand;
        private ICommand loginCommand;
        private ICommand logoutCommand;
        private IHydraSdk vpnClient;
        private IReadOnlyCollection<VpnNodeModel> nodes;
        private VpnNodeModel selectedNodeModel;
        private Carrier carrier;
        private DispatcherTimer dispatcherTimer;
        private string deviceId;
        private string carrierId;
        private string backendAddress;
        private string country;
        private string errorText;
        private string accessToken;
        private string password;
        private string vpnIpServerServer;
        private string vpnIp;
        private string bytesReceived;
        private string bytesSent;
        private string status;
        private string remainingTrafficResponse;
        private string gitHubLogin;
        private string gitHubPassword;
        private string bypassDomains = "iplocation.net\r\n*.iplocation.net";
        private string serviceName = "Hydra Sdk Demo Vpn Service";
        private string logContents;
        private bool isErrorVisible;
        private bool isConnectButtonVisible;
        private bool isDisconnectButtonVisible;
        private bool reconnectOnWakeUp = true;
        private bool useService = true;
        private bool useGithubAuthorization;
        private bool isLoginButtonVisible;
        private bool isLogoutButtonVisible;
        private bool isLoggedIn;

        /// <summary>
        /// Initializes static members of the <see cref="MainScreenViewModel"/> class.
        /// <see cref="MainScreenViewModel"/> static constructor. Performs <see cref="MachineId"/> initialization.
        /// </summary>
        static MainScreenViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainScreenViewModel"/> class.
        /// <see cref="MainScreenViewModel"/> default constructor.
        /// </summary>
        public MainScreenViewModel()
        {
            // Init view model
            var dateTime = DateTime.Now;
            this.DeviceId = $"{MachineId}-{dateTime:dd-MM-yy}";

            // this.CarrierId = "touchvpn";
            this.CarrierId = "afdemo";
            this.BackendAddress = "https://backend.northghost.com/";
            this.IsConnectButtonVisible = false;
            this.SetStatusDisconnected();
            this.SetStatusLoggedOut();

            // Init remaining traffic timer
            this.InitializeTimer();

            // Init logging
            this.InitializeLogging();

            this.BootstrapVpn();
        }

        /// <summary>
        /// Gets or sets device id for backend login method.
        /// </summary>
        public string DeviceId
        {
            get => this.deviceId;
            set => this.SetProperty(ref this.deviceId, value);
        }

        /// <summary>
        /// Gets or sets carrier id for backend service.
        /// </summary>
        public string CarrierId
        {
            get => this.carrierId;
            set => this.SetProperty(ref this.carrierId, value);
        }

        /// <summary>
        /// Gets or sets backend url address for backend service.
        /// </summary>
        public string BackendAddress
        {
            get => this.backendAddress;
            set => this.SetProperty(ref this.backendAddress, value);
        }

        /// <summary>
        /// Gets or sets message which is disoplayed in case of errors.
        /// </summary>
        public string ErrorText
        {
            get => this.errorText;
            set => this.SetProperty(ref this.errorText, value);
        }

        /// <summary>
        /// Gets or sets access token for backend methods.
        /// </summary>
        public string AccessToken
        {
            get => this.accessToken;
            set => this.SetProperty(ref this.accessToken, value);
        }

        /// <summary>
        /// Gets or sets user password for hydra.
        /// </summary>
        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        /// <summary>
        /// Gets or sets vPN service IP address.
        /// </summary>
        public string VpnIpServer
        {
            get => this.vpnIpServerServer;
            set => this.SetProperty(ref this.vpnIpServerServer, value);
        }

        /// <summary>
        /// Gets or sets vPN service IP address.
        /// </summary>
        public string VpnIp
        {
            get => this.vpnIp;
            set => this.SetProperty(ref this.vpnIp, value);
        }

        /// <summary>
        /// Gets or sets remaining traffic response.
        /// </summary>
        public string RemainingTrafficResponse
        {
            get => this.remainingTrafficResponse;
            set => this.SetProperty(ref this.remainingTrafficResponse, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether error visibility flag.
        /// </summary>
        public bool IsErrorVisible
        {
            get => this.isErrorVisible;
            set => this.SetProperty(ref this.isErrorVisible, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether connect button visibility flag.
        /// </summary>
        public bool IsConnectButtonVisible
        {
            get => this.isConnectButtonVisible;
            set => this.SetProperty(ref this.isConnectButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether disconnect button visibility flag.
        /// </summary>
        public bool IsDisconnectButtonVisible
        {
            get => this.isDisconnectButtonVisible;
            set => this.SetProperty(ref this.isDisconnectButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets received bytes count.
        /// </summary>
        public string BytesReceived
        {
            get => this.bytesReceived;
            set => this.SetProperty(ref this.bytesReceived, value);
        }

        /// <summary>
        /// Gets or sets sent bytes count.
        /// </summary>
        public string BytesSent
        {
            get => this.bytesSent;
            set => this.SetProperty(ref this.bytesSent, value);
        }

        /// <summary>
        /// Gets or sets vPN connection status.
        /// </summary>
        public string Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        /// <summary>
        /// Gets or sets country for backend get credentials method.
        /// </summary>
        public string Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether use service flag.
        /// </summary>
        public bool UseService
        {
            get => this.useService;
            set => this.SetProperty(ref this.useService, value);
        }

        /// <summary>
        /// Gets or sets name of windows service to use to establish hydra connection.
        /// </summary>
        public string ServiceName
        {
            get => this.serviceName;
            set => this.SetProperty(ref this.serviceName, value);
        }

        /// <summary>
        /// Gets or sets hydra log contents.
        /// </summary>
        public string LogContents
        {
            get => this.logContents;
            set => this.SetProperty(ref this.logContents, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether use GitHub authorization flag.
        /// </summary>
        public bool UseGithubAuthorization
        {
            get => this.useGithubAuthorization;
            set
            {
                this.SetProperty(ref this.useGithubAuthorization, value);
                this.RaisePropertyChanged(nameof(this.IsGithubCredentialsEnabled));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether login button visibility flag.
        /// </summary>
        public bool IsLoginButtonVisible
        {
            get => this.isLoginButtonVisible;
            set => this.SetProperty(ref this.isLoginButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether logout button visibility flag.
        /// </summary>
        public bool IsLogoutButtonVisible
        {
            get => this.isLogoutButtonVisible;
            set => this.SetProperty(ref this.isLogoutButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether logged in flag.
        /// </summary>
        public bool IsLoggedIn
        {
            get => this.isLoggedIn;
            set
            {
                this.SetProperty(ref this.isLoggedIn, value);
                this.RaisePropertyChanged(nameof(this.IsLoggedOut));
                this.RaisePropertyChanged(nameof(this.IsGithubCredentialsEnabled));
            }
        }

        /// <summary>
        /// Gets or sets the nodes collection.
        /// </summary>
        public IReadOnlyCollection<VpnNodeModel> Nodes
        {
            get => this.nodes;
            set => this.SetProperty(ref this.nodes, value);
        }

        /// <summary>
        /// Gets or sets the selected selectedNodeModel.
        /// </summary>
        public VpnNodeModel SelectedNodeModel
        {
            get => this.selectedNodeModel;
            set => this.SetProperty(ref this.selectedNodeModel, value);
        }

        /// <summary>
        /// Gets a value indicating whether logged out flag.
        /// </summary>
        public bool IsLoggedOut => !this.isLoggedIn;

        /// <summary>
        /// Gets a value indicating whether gitHub credentials enabled flag.
        /// </summary>
        public bool IsGithubCredentialsEnabled => this.UseGithubAuthorization && this.IsLoggedOut;

        /// <summary>
        /// Gets or sets gitHub login.
        /// </summary>
        public string GitHubLogin
        {
            get => this.gitHubLogin;
            set => this.SetProperty(ref this.gitHubLogin, value);
        }

        /// <summary>
        /// Gets or sets gitHub password.
        /// </summary>
        public string GitHubPassword
        {
            get => this.gitHubPassword;
            set => this.SetProperty(ref this.gitHubPassword, value);
        }

        /// <summary>
        /// Gets or sets bypass domains raw.
        /// </summary>
        public string BypassDomains
        {
            get => this.bypassDomains;
            set => this.SetProperty(ref this.bypassDomains, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether reconnect on wakeup event.
        /// </summary>
        public bool ReconnectOnWakeUp
        {
            get => this.reconnectOnWakeUp;
            set => this.SetProperty(ref this.reconnectOnWakeUp, value);
        }

        /// <summary>
        /// Gets connect command.
        /// </summary>
        public ICommand ConnectCommand => this.connectCommand ?? (this.connectCommand = new DelegateCommand(this.Connect));

        /// <summary>
        /// Gets disconnect command.
        /// </summary>
        public ICommand DisconnectCommand => this.disconnectCommand ?? (this.disconnectCommand = new DelegateCommand(this.Disconnect));

        /// <summary>
        /// Gets clear log command.
        /// </summary>
        public ICommand ClearLogCommand => this.clearLogCommand ??
                                           (this.clearLogCommand = new DelegateCommand(this.ClearLog));

        /// <summary>
        /// Gets login command.
        /// </summary>
        public ICommand LoginCommand => this.loginCommand ?? (this.loginCommand = new DelegateCommand(this.Login));

        /// <summary>
        /// Gets logout command.
        /// </summary>
        public ICommand LogoutCommand => this.logoutCommand ?? (this.logoutCommand = new DelegateCommand(this.Logout));

        /// <summary>
        /// Gets GiHub OAuth token.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="pass">User password.</param>
        /// <returns>OAuth token or string.Empty in case of failure.</returns>
        private static async Task<string> GetGithubOAuthToken(string login, string pass)
        {
            const string githubOtpHeader = "X-GitHub-OTP";

            try
            {
                var response = await LoginGithub(login, pass).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        && response.Headers.Contains(githubOtpHeader))
                    {
                        HydraLogger.Trace("Two-factor authentication enabled");

                        var requestAuthCode = ServiceLocator.Current.GetInstance<RequestAuthCode>();

                        requestAuthCode.ShowDialog();
                        if (requestAuthCode.DialogResult != true)
                        {
                            HydraLogger.Trace("Cancel authorization!");
                            return string.Empty;
                        }

                        var authCode = requestAuthCode.RequestAuthCodeViewModel.AuthCode;
                        HydraLogger.Trace("Sending authentication code...");

                        response = await LoginGithub(login, pass, authCode).ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode)
                        {
                            HydraLogger.Trace("Two-factor authentication failed!");
                            return string.Empty;
                        }
                    }
                    else
                    {
                        HydraLogger.Trace("Unable to get OAuth token from GitHub!");
                        return string.Empty;
                    }
                }

                HydraLogger.Trace("Got valid response from GitHub");
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseJson = JObject.Parse(responseString);

                return responseJson["token"].ToObject<string>();
            }
            catch
            {
                // Something went wrong
                return string.Empty;
            }
        }

        /// <summary>
        /// Performs login to GitHub.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="pass">User password.</param>
        /// <param name="authCode">Optional authentication code for two-factor authentication.</param>
        /// <returns><see cref="HttpResponseMessage"/>.</returns>
        private static async Task<HttpResponseMessage> LoginGithub(string login, string pass, string authCode = null)
        {
            const string apiUrl = "https://api.github.com/authorizations";
            const string clientId = "70ed6ffd4b08b3119208";
            const string clientSecret = "fe02229ef77aa489f748f346e3e337490fd5b8ce";
            const string githubOtpHeader = "X-GitHub-OTP";

            var authString = Convert.ToBase64String(Encoding.Default.GetBytes($"{login}:{pass}"));
            var parameters = new
            {
                scopes = new string[] { },
                client_id = clientId,
                client_secret = clientSecret,
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", login);

                using (var message = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                {
                    message.Headers.Accept.ParseAdd("application/json");
                    message.Headers.Authorization = AuthenticationHeaderValue.Parse($"Basic {authString}");

                    // Two-factor authentication
                    if (authCode != null)
                    {
                        message.Headers.Add(githubOtpHeader, authCode);
                    }

                    var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
                    message.Content = content;

                    HydraLogger.Trace("Trying to get OAuth token from GitHub...");
                    return await client.SendAsync(message).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Performs logging initialization.
        /// </summary>
        private void InitializeLogging()
        {
            var loggerListener = new EventLoggerListener();
            loggerListener.LogEntryArrived += (sender, args) => this.AddLogEntry(args.Entry);
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
        /// Performs login to the backend server.
        /// </summary>
        private async void Login()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsLoginButtonVisible = false;

                // Perform logout
                await LogoutHelper.Logout().ConfigureAwait(false);

                // Get GitHub OAuth token if necessary
                string oauthToken = string.Empty;
                if (this.UseGithubAuthorization)
                {
                    oauthToken = await GetGithubOAuthToken(this.GitHubLogin, this.GitHubPassword).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(oauthToken))
                    {
                        MessageBox.Show("Could not perform GitHub authorization!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.IsLoginButtonVisible = true;
                        return;
                    }
                }

                // Bootstrap VPN
                this.BootstrapVpn();

                // Create AuthMethod
                var authMethod = this.UseGithubAuthorization
                    ? AuthMethod.GitHub(oauthToken)
                    : AuthMethod.Anonymous();

                // Perform login
                var loginResponse = await this.vpnClient.Login(authMethod).ConfigureAwait(false);

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
                this.carrier = new Carrier(this.CarrierId, string.Empty, this.AccessToken);
                this.UpdateCountries();

                // Work with UI
                this.SetStatusLoggedIn();

                // Update remaining traffic
                await this.UpdateRemainingTraffic().ConfigureAwait(false);
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
            try
            {
                // Get available countries
                var countriesResponse = await this.vpnClient.GetNodes(this.carrier).ConfigureAwait(false);

                // Check whether request was successful
                if (!countriesResponse.IsSuccess)
                {
                    this.IsLoginButtonVisible = true;
                    this.ErrorText = countriesResponse.Error ?? countriesResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Get countries from response
                var countries = countriesResponse.VpnNodes.Select(VpnCountriesParser.ToVpnNodeModel).ToList();

                // Remember countries
                this.Nodes = countries;
            }
            catch (Exception e)
            {
                // Show error when exception occured
                this.IsLoginButtonVisible = true;
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }
        }

        private async void Logout()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsLogoutButtonVisible = false;

                // Perform logout
                var logoutResponse = await this.vpnClient.Logout(this.carrier).ConfigureAwait(false);

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
                this.RemainingTrafficResponse = string.Empty;

                // Work with UI
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
            this.vpnClient.VpnConnectionStateChanged += (sender, args) =>
            {
                if (args.State == VpnConnectionState.Connected)
                {
                    this.VpnClientOnConnected();
                }
                else
                {
                    this.VpnClientOnDisconnected();
                }
            };

            this.vpnClient.StatisticsChanged += this.VpnClientOnStatisticsChanged;
        }

        /// <summary>
        /// VPN client statistics changed event handler.
        /// </summary>
        /// <param name="sender">Sender (VPN client).</param>
        /// <param name="vpnStatisticsChangedEventArgs">Event arguments (bytes sent/received).</param>
        private void VpnClientOnStatisticsChanged(object sender, VpnStatisticsChangedEventArgs vpnStatisticsChangedEventArgs)
        {
            this.BytesReceived = vpnStatisticsChangedEventArgs.Data.BytesReceived.ToString(CultureInfo.InvariantCulture);
            this.BytesSent = vpnStatisticsChangedEventArgs.Data.BytesSent.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// VPN client disconnected event handler.
        /// </summary>
        private void VpnClientOnDisconnected()
        {
            this.SetStatusDisconnected();
        }

        /// <summary>
        /// VPN client connected event handler.
        /// </summary>
        private void VpnClientOnConnected()
        {
            this.Status = "Connected";
            this.IsConnectButtonVisible = false;
            this.IsDisconnectButtonVisible = true;
            this.IsLogoutButtonVisible = false;
        }

        /// <summary>
        /// Performs actions related to setting backend status to "Logged out".
        /// </summary>
        private void SetStatusLoggedOut()
        {
            this.IsLoginButtonVisible = true;
            this.IsLogoutButtonVisible = false;
            this.IsLoggedIn = false;
        }

        /// <summary>
        /// Performs actions related to setting backend status to "Logged in".
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
        /// Bootstraps VPN according to the selected parameters and initializes VPN events.
        /// </summary>
        private void BootstrapVpn()
        {
            // Get bypass domains list
            var bypass = this.BypassDomains.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Bootstrap hydra backend with provided CarrierId and BackendAddress
            var backendConfiguration = new BackendServerConfiguration(this.CarrierId, this.BackendAddress, this.DeviceId);
            var hydraConfiguration = new HydraWindowsConfiguration(this.serviceName, new Dictionary<int, IConnectionRule>()).AddBypassDomains(bypass);

            // .SetReconnectOnWakeUp(this.ReconnectOnWakeUp);
            var hydraBootstrapper = new HydraWindowsBootstrapper();
            hydraBootstrapper.Bootstrap(backendConfiguration, hydraConfiguration);

            this.vpnClient = HydraIoc.Container.Resolve<IHydraSdk>();
            this.InitializeEvents();
        }

        /// <summary>
        /// Performs VPN connection.
        /// </summary>
        private async void Connect()
        {
            try
            {
                // Connect VPN using provided VPN server IP and user hash
                await this.vpnClient.StartVpn(this.SelectedNodeModel.ServerModel).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Show error when exception occuredR
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }
        }

        /// <summary>
        /// Disconnects from VPN server.
        /// </summary>
        private async void Disconnect()
        {
            try
            {
                // Disconnect VPN
                await this.vpnClient.StopVpn().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Show error when exception occured
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }

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
            await this.UpdateRemainingTraffic().ConfigureAwait(false);
        }

        /// <summary>
        /// Performs update of remaining traffic.
        /// </summary>
        private async Task UpdateRemainingTraffic()
        {
            try
            {
                // Check if access token is not empty
                if (string.IsNullOrWhiteSpace(this.AccessToken))
                {
                    return;
                }

                // Get remaining traffic
                var remainingTrafficResponseResult = await this.vpnClient.GetRemainingTraffic(this.carrier).ConfigureAwait(false);

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
            catch (Exception e)
            {
                HydraLogger.Trace(e.Message);
            }
        }
    }
}
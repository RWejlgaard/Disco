/*
ONLY FOR INTERNAL KMD USE
Author: ZRW
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Renci.SshNet;

namespace disco {
    public partial class MainWindow {
        private readonly SshClient _dansSsh = new SshClient("172.20.113.161", "weblogic", "password");
        //Settings _settingsWindow = new Settings();

        public MainWindow() {
            Disco.Default.Upgrade();
            InitializeComponent();
            ControlServerList.Items.Clear();
            DeploymentTargetCombo.Items.Clear();
            foreach (var server in Disco.Default.Servers) {
                var listItem = new CheckBox {
                    Content = server,
                    Margin = new Thickness(5, 2, 2, 2),
                    Foreground = Brushes.White
                };
                ControlServerList.Items.Add(listItem);
                var comboItem = new ComboBoxItem {Content = server};
                DeploymentTargetCombo.Items.Add(comboItem);
            }
        }

        private void AppendLog(string var) {
            Log.AppendText(var + "\n");
            Log.ScrollToEnd();
        }

        private IEnumerable<string> GetSelectedServers() {
            var servers = (from server in ControlServerList.Items.Cast<CheckBox>()
                where server.IsChecked.Value
                select server.Content.ToString()).ToList();
            return servers;
        }

        private void ControlStartBtn_Click(object sender, RoutedEventArgs e) {
            StartServer(GetSelectedServers());
        }

        private void ControlStopBtn_Click(object sender, RoutedEventArgs e) {
            StopServer(GetSelectedServers());
        }

        private void ControlRestartBtn_Click(object sender, RoutedEventArgs e) {
            RestartServer(GetSelectedServers());
        }

        private void TitleAboutBtn_Click(object sender, RoutedEventArgs e) {
            var aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        private void ServersSelectAllBtn_Click(object sender, RoutedEventArgs e) {
            foreach (var server in ControlServerList.Items.Cast<CheckBox>()) {
                server.IsChecked = true;
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e) {
            foreach (var server in Disco.Default.Servers) {
                AppendLog(server);
            }
        }

        private void TitleSettingsBtn_Click(object sender, RoutedEventArgs e) {
            var settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

        private void StartServer(IEnumerable<string> servers) {
            if (!servers.Any()) {
                AppendLog("Error: No server(s) selected!");
                return;
            }
            AppendLog("Connecting to DANS-APP...");
            try {
                _dansSsh.Connect();
                AppendLog("Success!");
            }
            catch (Exception e) {
                AppendLog("Connection Failed: " + e);
                return;
            }
            AppendLog("Starting server(s):");

            foreach (var server in servers) {
                AppendLog(server);
                _dansSsh.RunCommand("sh ~/startserver " + server);
            }
            _dansSsh.Disconnect();
        }

        private void StopServer(IEnumerable<string> servers) {
            if (!servers.Any()) {
                AppendLog("Error: No server(s) selected!");
                return;
            }
            AppendLog("Connecting to DANS-APP...");
            try {
                _dansSsh.Connect();
                AppendLog("Success!");
            }
            catch (Exception e) {
                AppendLog("Connection Failed: " + e);
                return;
            }
            AppendLog("Stopping server(s):");

            foreach (var server in servers) {
                AppendLog(server);
                _dansSsh.RunCommand("sh ~/stopserver " + server);
            }
            _dansSsh.Disconnect();
        }

        private async void RestartServer(IEnumerable<string> servers) {
            if (!servers.Any()) {
                AppendLog("Error: No server(s) selected!");
                return;
            }
            StopServer(servers);
            AppendLog("Waiting 10 seconds for server(s) to shutdown");
            await Task.Delay(10000);
            StartServer(servers);
        }

        private void TitleManageBtn_Click(object sender, RoutedEventArgs e) {
            var serverManagerWindow = new ServerManager();
            serverManagerWindow.ShowDialog();
        }

       
    }
}
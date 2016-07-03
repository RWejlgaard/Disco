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
using MahApps.Metro.Controls.Dialogs;
using Renci.SshNet;

namespace disco {
    public partial class MainWindow {
        readonly SshClient _dansSsh = new SshClient("10.0.0.33", "cosmo", "password");
        //Settings _settingsWindow = new Settings();

        public MainWindow() {
            InitializeComponent();
        }

        private void AppendLog(string var) {
            Log.AppendText(var + "\n");
            Log.ScrollToEnd();
        }

        private IEnumerable<string> GetSelectedServers() {
            var _servers =
                (from _server in ControlServerList.Items.Cast<CheckBox>()
                    where _server.IsChecked.Value
                    select _server.Content.ToString()).ToList();
            return _servers;
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
            catch (Exception _e) {
                AppendLog("Connection Failed: " + _e);
                return;
            }
            AppendLog("Starting server(s):");

            foreach (var _server in servers) {
                AppendLog(_server);
                _dansSsh.RunCommand("sh ~/startserver " + _server);
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
            catch (Exception _e) {
                AppendLog("Connection Failed: " + _e);
                return;
            }
            AppendLog("Stopping server(s):");

            foreach (var _server in servers) {
                AppendLog(_server);
                _dansSsh.RunCommand("sh ~/stopserver " + _server);
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

        private void TitleAboutBtn_Click(object sender, RoutedEventArgs e)
        {
            About _aboutWindow = new About();
            _aboutWindow.ShowDialog();
        }
    }
}
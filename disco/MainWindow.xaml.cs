/*
ONLY FOR INTERNAL KMD USE
Author: ZRW
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Renci.SshNet;

namespace disco {
    public partial class MainWindow {
        public static PrivateKeyFile KeyFile;
        private readonly SshClient _dansSsh;

        public MainWindow() {
            InitializeComponent();
            UpdateServerLists();
            var loadKeyWindow = new LoadKey();
            loadKeyWindow.ShowDialog();
            try {
                KeyFile = new PrivateKeyFile(loadKeyWindow.KeyFilePath, loadKeyWindow.Password);
                _dansSsh = new SshClient("dans-app", loadKeyWindow.UserName, KeyFile);
            }
            catch {
                MessageBox.Show("Invalid credentials");
                Close();
            }
        }

        private void AppendLog(string var) {
            Log.AppendText(var + "\n");
            Log.ScrollToEnd();
        }

        private void UpdateServerLists() {
            Disco.Default.Upgrade();
            ControlServerList.Children.Clear();
            DeploymentTargetCombo.Items.Clear();
            foreach (var server in Disco.Default.Servers) {
                var listItem = new CheckBox {
                    Content = server,
                    Margin = new Thickness(5, 5, 5, 2),
                    Foreground = Brushes.White
                };
                ControlServerList.Children.Add(listItem);
                var comboItem = new ComboBoxItem {Content = server};
                DeploymentTargetCombo.Items.Add(comboItem);
            }
        }

        private IEnumerable<string> GetSelectedServers() {
            var servers = (from server in ControlServerList.Children.Cast<CheckBox>()
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
            foreach (var server in ControlServerList.Children.Cast<CheckBox>()) {
                server.IsChecked = true;
            }
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
                _dansSsh.RunCommand("sh /appl/zrw/startserver.sh " + server);
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
                _dansSsh.RunCommand("sh /appl/zrw/stopserver.sh " + server);
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
            serverManagerWindow.Closing += Manager_Closing;
        }

        public void Manager_Closing(object sender, CancelEventArgs e) {
            UpdateServerLists();
        }

        private void ServersRefreshBtn_Click(object sender, RoutedEventArgs e) {
            UpdateServerLists();
        }
    }
}
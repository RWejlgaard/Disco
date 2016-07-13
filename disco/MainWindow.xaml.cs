/*
ONLY FOR INTERNAL KMD USE
Author: ZRW
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Renci.SshNet;
using SharpSvn;

namespace disco {
    public partial class MainWindow {
        private readonly SshClient _dansSsh;

        public MainWindow() {
            InitializeComponent();
            UpdateServerLists();
            var loadKeyWindow = new LoadKey();
            loadKeyWindow.ShowDialog();
            try {
                var keyFile = new PrivateKeyFile(loadKeyWindow.KeyFilePath, loadKeyWindow.Password);
                _dansSsh = new SshClient("dans-app", loadKeyWindow.UserName, keyFile);
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
            DeploymentBranchCombo.Items.Clear();
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
            foreach (var branch in Disco.Default.Branches) {
                var comboItem = new ComboBoxItem {Content = branch};
                DeploymentBranchCombo.Items.Add(comboItem);
            }
            DeploymentTargetCombo.SelectedIndex = 0;
            DeploymentBranchCombo.SelectedIndex = 0;
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

        private void TitleServersBtn_Click(object sender, RoutedEventArgs e) {
            var serverManagerWindow = new ServerManager();
            serverManagerWindow.ShowDialog();
            UpdateServerLists();
        }

        private void TitleBranchesBtn_Click(object sender, RoutedEventArgs e) {
            var branchManagerWindow = new BranchManager();
            branchManagerWindow.ShowDialog();
            UpdateServerLists();
        }

        private void DeploymentVersionInput_GotFocus(object sender, RoutedEventArgs e) {
            DeploymentVersionInput.Clear();
        }

        private void DeploymentCompileAndDeployBtn_Click(object sender, RoutedEventArgs e) {
            var target = DeploymentTargetCombo.Text;
            var branch = DeploymentBranchCombo.Text;
            var version = DeploymentVersionInput.Text;
            if (string.IsNullOrEmpty(version) || version.Any(char.IsLetter)) {
                DeploymentVersionInput.BorderBrush = Brushes.Red;
                return;
            }
            Checkout(branch);
            Compile(branch, version);
        }

        private async void Checkout(string url) {
            var branchName = url.Split('/').Last();
            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var checkoutPath = Path.Combine(appdataPath, "disco", "branches", branchName);
            var svnClient = new SvnClient();
            AppendLog("Checking out from SVN...");
            await Task.Run(() => { svnClient.CheckOut(new SvnUriTarget(url), checkoutPath); });
            AppendLog("Complete!");
        }

        private void Compile(string branch, string version) {
        }

        private void Deploy(string target, string branch) {
            AppendLog("Connecting to DANS-APP...");
            try {
                _dansSsh.Connect();
                AppendLog("Success!");
            }
            catch (Exception e) {
                AppendLog("Connection Failed: " + e);
                return;
            }
            var shortBranch = branch.Split('/').Last();
            AppendLog("Running deploy script on DANS-APP..");
            _dansSsh.RunCommand("cd /appl/zrw && sh deploy.sh -t " + target + " -b " + shortBranch);
        }

        private string GetDansBranchPath(string branch) {
            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdataPath, "disco", "branches", branch);
            return path;
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
    }
}
/*
ONLY FOR INTERNAL KMD USE
Author: ZRW
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Renci.SshNet;
using SharpSvn;

namespace disco {
    public partial class MainWindow {
        private readonly SshClient _dansSsh;

        public MainWindow() {
            InitializeComponent();
            // Updates UI elements with possible changes in Branch or Server lists
            UpdateServerLists();
            //Prepares for loading private key and shows window
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
            // This loads up previous changes and commits them to cold storage
            Disco.Default.Upgrade();
            // Clear all UI elements
            ControlServerList.Children.Clear();
            DeploymentTargetCombo.Items.Clear();
            DeploymentBranchCombo.Items.Clear();

            // Loads up Server list and appends it to UI elements
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

            // Loads up Branch list and appends it to UI elements
            foreach (var branch in Disco.Default.Branches) {
                var comboItem = new ComboBoxItem {Content = branch};
                DeploymentBranchCombo.Items.Add(comboItem);
            }
            DeploymentTargetCombo.SelectedIndex = 0;
            DeploymentBranchCombo.SelectedIndex = 0;
        }

        // Returns a string array with names of selected servers
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
            var branchName = branch.Split('/').Last();
            Checkout(branch);
            Compile(GetDansBranchPath(branchName), version);
            //Commit(GetDansBranchPath(branchName));
            Deploy(target, branchName);
            RestartServer(new List<string>{target});
        }

        private async void Checkout(string url) {
            var branchName = url.Split('/').Last();
            var checkoutPath = GetDansBranchPath(branchName);
            var svnClient = new SvnClient();
            AppendLog("Checking out from SVN...");
            // This is an async operator, it makes sure Checkout() runs in background without freezing the whole application
            await Task.Run(() => { svnClient.CheckOut(new SvnUriTarget(url), checkoutPath); });
            AppendLog("Complete!");
        }

        private async void Commit(string path) {
            var svnClient = new SvnClient();
            AppendLog("Committing to SVN...");
            await Task.Run(() => { svnClient.Commit(path); });
            AppendLog("Complete!");
        }

        private async void Compile(string path, string version) {
            AppendLog("Starting build...");
            AppendLog("This should take 30 min...");
            try {
                // Runs dans-ant-build.bat asynchronous, to not stall program during runtime
                await RunProcessAsync(Path.Combine(Environment.CurrentDirectory, "dans-ant-build.bat"), path + " " + version);
                AppendLog("Compile successful!");
            }
            catch (Exception) {
                AppendLog("Script failed to execute - file missing?");
                throw;
            }
        }

        /*
         * At the time of writing Process.Start() does not have an asynchronous counterpart
         * and therefore it has to be written manually
         */
        private static Task RunProcessAsync(string fileName, string arguments) {
            var completionSource = new TaskCompletionSource<bool>();

            var process = new Process {
                StartInfo = {
                    FileName = fileName,
                    Arguments = arguments
                },
                // This makes the process trigger the exited event
                EnableRaisingEvents = true
            };

            // When process is complete, cleanup afterwards
            process.Exited += (sender, args) => {
                completionSource.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return completionSource.Task;
        }

        private async void Deploy(string target, string branch) {
            AppendLog("Connecting to DANS-APP...");
            try {
                _dansSsh.Connect();
                AppendLog("Success!");
            }
            catch (Exception e) {
                AppendLog("Connection Failed: " + e);
                return;
            }

            // Takes the branch name from the url
            var shortBranch = branch.Split('/').Last();

            AppendLog("Running deploy script on DANS-APP..");
            AppendLog("This might take a while");

            // Runs deploy script
            var command = "sh autodeploy.sh -t " + target + " -b " + shortBranch;
            await Task.Run(() => { _dansSsh.RunCommand("cd /appl/zrw && " + command); });

            AppendLog("Successfully completed deploy script");
        }

        private static string GetDansBranchPath(string branch) {
            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdataPath, "disco", "branches", branch);
            return path;
        }

        private void StartServer(IEnumerable<string> servers) {
            // Checks if there are any servers selected
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

            /*
             * Runs startserver script for every server in list
             * this is a critical dependency and should be loaded from application 
             */
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
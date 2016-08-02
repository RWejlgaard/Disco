using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace disco {
    public partial class ServerManager {
        public ServerManager() {
            InitializeComponent();
            UpdateServerList();
        }

        private void UpdateServerList() {
            // Clears UI element
            ManagerServerList.Items.Clear();
            // Loads up servers from server list
            foreach (var server in Disco.Default.Servers) {
                ManagerServerList.Items.Add(new ListViewItem {Content = server});
            }
        }

        private void ManagerServerList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Disables buttons if no server is selected, this should not happen
            if (ManagerServerList.SelectedValue != null) {
                ManageRemoveServerBtn.IsEnabled = true;
                ManageMoveUpBtn.IsEnabled = true;
                ManageMoveDownBtn.IsEnabled = true;
            }
            else {
                ManageRemoveServerBtn.IsEnabled = false;
                ManageMoveUpBtn.IsEnabled = false;
                ManageMoveDownBtn.IsEnabled = false;
            }
        }

        private void ManageAddServerBtn_Click(object sender, RoutedEventArgs e) {
            // Shows addServer dialog and updates UI elements
            var addServerWindow = new AddServer();
            addServerWindow.ShowDialog();
            UpdateServerList();
        }

        private void ManageRemoveServerBtn_Click(object sender, RoutedEventArgs e) {
            var removeServer = (ListViewItem) ManagerServerList.SelectedItem;
            Disco.Default.Servers.Remove(removeServer.Content.ToString());
            Disco.Default.Save();
            UpdateServerList();
        }

        private void ManageMoveUpBtn_Click(object sender, RoutedEventArgs e) {
            var selection = ManagerServerList.SelectedIndex;
            var serverList = Disco.Default.Servers.Cast<string>().ToList();
            MoveUp(serverList, selection);
            Disco.Default.Servers.Clear();
            Disco.Default.Servers.AddRange(serverList.ToArray());
            Disco.Default.Save();
            UpdateServerList();
        }

        private void ManageMoveDownBtn_Click(object sender, RoutedEventArgs e) {
            var serverList = Disco.Default.Servers.Cast<string>().ToList();
            MoveDown(serverList, ManagerServerList.SelectedIndex);
            Disco.Default.Servers.Clear();
            Disco.Default.Servers.AddRange(serverList.ToArray());
            Disco.Default.Save();
            UpdateServerList();
        }

        // Moves specified index up one step in given list
        private static void MoveUp<T>(IList<T> list, int index) {
            if (index <= 0) return;
            var old = list[index - 1];
            list[index - 1] = list[index];
            list[index] = old;
        }

        // Moves specified index down one step in given list
        private static void MoveDown<T>(IList<T> list, int index) {
            if (index >= list.Count - 1) return;
            var old = list[index + 1];
            list[index + 1] = list[index];
            list[index] = old;
        }
    }
}
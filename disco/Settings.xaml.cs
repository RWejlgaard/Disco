using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace disco {
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : MetroWindow {
        public Settings() {
            InitializeComponent();
            foreach (var server in Disco.Default.Servers) {
                SettingsRemoveServerCombo.Items.Add(new ComboBoxItem() {Content = server});
            }
        }

        private async void SettingsAddServerBtn_Click(object sender, RoutedEventArgs e) {
            var newServer = await this.ShowInputAsync("Add Server", "Enter server name:");
            Disco.Default.Servers.Add(newServer);
            Disco.Default.Save();
        }

        private void SettingsRemoveServerBtn_Click(object sender, RoutedEventArgs e) {
            var removeServer = SettingsRemoveServerCombo.Text;
            if (removeServer == "") return;
            Disco.Default.Servers.Remove(removeServer);
            Disco.Default.Save();
        }
    }
}
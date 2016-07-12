using System.Windows;

namespace disco {
    public partial class AddServer {
        public AddServer() {
            InitializeComponent();
        }

        private void AddServerAddBtn_Click(object sender, RoutedEventArgs e) {
            var newServer = AddServerServerNameInput.Text;
            if (string.IsNullOrEmpty(newServer)) return;
            Disco.Default.Servers.Add(newServer);
            Disco.Default.Save();
            Close();
        }

        private void AddServerCancelBtn_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
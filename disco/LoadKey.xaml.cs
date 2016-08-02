/*
ONLY FOR INTERNAL KMD USE
Author: ZRW
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace disco {
    public partial class LoadKey {
        public string KeyFilePath;
        public string Password;
        public string UserName;

        public LoadKey() {
            InitializeComponent();
            LoadKeyUsernameInput.Text = Environment.UserName;
        }

        private void LoadKeyBrowseBtn_Click(object sender, RoutedEventArgs e) {
            var fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            KeyFilePath = fileDialog.FileName;
            LoadKeyFileLbl.Content = fileDialog.FileName.Split('\\').Last();
            LoadKeyUsernameInput.IsEnabled = true;
            LoadKeyPasswordInput.IsEnabled = true;
            LoadKeySubmitBtn.IsEnabled = true;
        }

        private void LoadKeySubmitBtn_Click(object sender, RoutedEventArgs e) {
            UserName = LoadKeyUsernameInput.Text;
            Password = LoadKeyPasswordInput.Password;
            Close();
        }

        private void LoadKeyPasswordInput_GotFocus(object sender, RoutedEventArgs e) {
            LoadKeyPasswordInput.Clear();
        }

        private void LoadKeyPasswordInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            UserName = LoadKeyUsernameInput.Text;
            Password = LoadKeyPasswordInput.Password;
            Close();
        }
    }
}
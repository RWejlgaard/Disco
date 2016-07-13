using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace disco {
    public partial class BranchManager {
        public BranchManager() {
            InitializeComponent();
            UpdateBranchList();
        }

        private void UpdateBranchList() {
            ManagerBranchList.Items.Clear();
            foreach (var server in Disco.Default.Branches) {
                ManagerBranchList.Items.Add(new ListViewItem {Content = server});
            }
        }

        private void ManagerBranchList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ManagerBranchList.SelectedValue != null) {
                ManageRemoveBranchBtn.IsEnabled = true;
                ManageMoveUpBtn.IsEnabled = true;
                ManageMoveDownBtn.IsEnabled = true;
            }
            else {
                ManageRemoveBranchBtn.IsEnabled = false;
                ManageMoveUpBtn.IsEnabled = false;
                ManageMoveDownBtn.IsEnabled = false;
            }
        }

        private void ManageAddBranchBtn_Click(object sender, RoutedEventArgs e) {
            var addBranchWindow = new AddBranch();
            addBranchWindow.ShowDialog();
            UpdateBranchList();
        }

        private void ManageRemoveBranchBtn_Click(object sender, RoutedEventArgs e) {
            var removeBranch = (ListViewItem) ManagerBranchList.SelectedItem;
            Disco.Default.Branches.Remove(removeBranch.Content.ToString());
            Disco.Default.Save();
            UpdateBranchList();
        }

        private void ManageMoveUpBtn_Click(object sender, RoutedEventArgs e) {
            var selection = ManagerBranchList.SelectedIndex;
            var serverList = Disco.Default.Branches.Cast<string>().ToList();
            MoveUp(serverList, selection);
            Disco.Default.Branches.Clear();
            Disco.Default.Branches.AddRange(serverList.ToArray());
            Disco.Default.Save();
            UpdateBranchList();
        }

        private void ManageMoveDownBtn_Click(object sender, RoutedEventArgs e) {
            var serverList = Disco.Default.Branches.Cast<string>().ToList();
            MoveDown(serverList, ManagerBranchList.SelectedIndex);
            Disco.Default.Branches.Clear();
            Disco.Default.Branches.AddRange(serverList.ToArray());
            Disco.Default.Save();
            UpdateBranchList();
        }

        private static void MoveUp<T>(IList<T> list, int index) {
            if (index <= 0) return;
            var old = list[index - 1];
            list[index - 1] = list[index];
            list[index] = old;
        }

        private static void MoveDown<T>(IList<T> list, int index) {
            if (index >= list.Count - 1) return;
            var old = list[index + 1];
            list[index + 1] = list[index];
            list[index] = old;
        }
    }
}
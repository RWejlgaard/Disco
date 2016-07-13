using System.Windows;

namespace disco {
    public partial class AddBranch {
        public AddBranch() {
            InitializeComponent();
        }

        private void AddBranchAddBtn_Click(object sender, RoutedEventArgs e) {
            var newBranch = AddBranchBranchNameInput.Text;
            if (string.IsNullOrEmpty(newBranch)) return;
            Disco.Default.Branches.Add(newBranch);
            Disco.Default.Save();
            Close();
        }

        private void AddBranchCancelBtn_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
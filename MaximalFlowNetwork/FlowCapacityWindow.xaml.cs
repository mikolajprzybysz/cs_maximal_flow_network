using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MaximalFlowNetwork {
    /// <summary>
    /// Interaction logic for FlowCapacityWindow.xaml
    /// </summary>
    public partial class FlowCapacityWindow : Window {
        public FlowCapacityWindow() {
            InitializeComponent();
        }

        private void textBoxFlow_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = Window1.IsTextAllowed(e.Text);
        }
    }
}

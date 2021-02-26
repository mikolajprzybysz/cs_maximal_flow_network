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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace MaximalFlowNetwork {
    /// <summary>
    /// Interaction logic for Vertex.xaml
    /// </summary>
    [ContentProperty("BodyText")]
    public partial class Vertex : UserControl {
        public Vertex() {
            InitializeComponent();
        }
        
        public string BodyText
		{
			get{
				return this.button.Content as string;
			}
			set{
				this.button.Content=value;
			}
		}
        
        private double _X;
        
        private double _Y;
        
        private double _XCenter { get;set; }
        
        private double _YCenter {  get;  set; }
        
        public double get_XCenter() {
            return _XCenter;
        }
        
        public double get_YCenter() {
            return _YCenter;
        }
        
        public double X {
            set{
                _X = value;
                _XCenter=_X+15d;
            }
            get {
                return _X;
            }
        }

        public double Y {
            set {
                _Y = value;
                _YCenter=_Y+15d;
            }
            get {
                return _Y;
            }
        }
        
        private Brush _bgColor;
        public Brush BgColor {
            set { _bgColor = value; button.Background = _bgColor; }
            get { return _bgColor; }
        }
      
        private bool _isHighlighted;
        public bool isHighlighted {
            set {
                _isHighlighted = value;
                if (value == true) {
                    BgColor = Brushes.Red;
                } else {
                    BgColor = Brushes.White;
                }
            }
            get {
                return _isHighlighted;
            }
        }

    }
}

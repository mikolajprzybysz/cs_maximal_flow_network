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
using QuickGraph;

namespace MaximalFlowNetwork {
    /// <summary>
    /// Interaction logic for Edge.xaml
    /// </summary>
    public partial class Edge : UserControl{

        private double _Length;
        public double Length {
            set { _Length = value; }
            get { return _Length; }
        }

        private double _Angle;
        public double Angle {
            set {
                _Angle = value; angleTransform.Angle = _Angle;
            }
            get {
                return _Angle;
            }
        }

        private int _Flow=0;
        public int Flow {
            set { 
                _Flow = value;
                label.Content = new StringBuilder(Convert.ToString(Flow) + "/" + Convert.ToString(Capacity)).ToString();
            }
            get { return _Flow; }
        }
        
        private int _Capacity = 0;
        public int Capacity {
            set { 
                _Capacity = value;
                label.Content = new StringBuilder(Convert.ToString(Flow) + "/" + Convert.ToString(Capacity)).ToString();
            }
            get { return _Capacity; }
        }
        
        private bool _showFC;
        public bool showFC {
            get { return _showFC; }
            set {
                _showFC = value;
                if (value == true) {
                    label.Visibility = Visibility.Visible;
                    label.Content = new StringBuilder(Convert.ToString(Flow) + "/" + Convert.ToString(Capacity)).ToString();
                    
                } else {
                    label.Visibility = Visibility.Hidden;
                }
            }
        }

        private double _L;
        public double L {
            get { return _L; }
            set { _L=value;}
        }

        private Brush _edgeColor;
        public Brush EdgeColor {
            set { _edgeColor = value; path.Stroke = value; }
            get { return _edgeColor; }
        }

        private bool _isHighlighted;
        public bool isHighlighted {
            set{
                _isHighlighted = value;
                if (value == true) {
                    path.Stroke = Brushes.Silver;
                } else {
                    path.Stroke = _edgeColor;
                }
            }
            get {
                return _isHighlighted;
            }
        }

        
        public Edge<object> graphEdge {
            get;
            set;
        }

        public Edge(double len) {
            InitializeComponent();
            Length = len;
            
            this.isHighlighted = false;
            this.EdgeColor = Brushes.Black;
            group.Children.Add(new LineGeometry(new Point(len-8d,1d),new Point(len,4d)));
            group.Children.Add(new LineGeometry(new Point(0,4d),new Point(len-8,4d)));
            group.Children.Add(new LineGeometry(new Point(len-8d,7d),new Point(len,4d)));
            group.Children.Add(new LineGeometry(new Point(len-8d,0),new Point(len-8d,8d)));
            showFC = true;
            angleTransform.Angle = 0;          
        }

       
    }
}

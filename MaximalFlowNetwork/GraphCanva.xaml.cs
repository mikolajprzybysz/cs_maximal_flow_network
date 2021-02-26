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
using System.Collections;
using System.ComponentModel;

namespace MaximalFlowNetwork {
    /// <summary>
    /// Interaction logic for GraphCanva.xaml
    /// </summary>
    public partial class GraphCanva : UserControl {
      
        #region Variables, constants etc.
        private double _width;
        private double _height;
        private Vertex[] _vertices = new Vertex[0];

        public String Source { get; set; }
        public String Sink { get; set; }

        public struct FlowCapacityPair {
            
            public int Flow;
            public int Capacity;
            
            public FlowCapacityPair(int flow, int capacity) {
                Flow = flow;
                Capacity = capacity;
            }
           
            public void setFlow(int flow){
                Flow = flow;
            }
            public void setCapacity(int capacity) {
                Capacity = capacity;
            }
        };

        private Dictionary<Edge<object>, FlowCapacityPair> fce = new Dictionary<Edge<object>, FlowCapacityPair>();
        private Dictionary<Edge<object>, bool> highlightedEdges = new Dictionary<Edge<object>, bool>();
        private Dictionary<Edge<object>, bool> signedEdges = new Dictionary<Edge<object>, bool>();

        public Dictionary<Edge<object>, FlowCapacityPair> getFCE() {
            return fce;
        }
        public void setFCE(Dictionary<Edge<object>, FlowCapacityPair> dict) {
            fce = dict;
        }

        public Dictionary<Edge<object>, bool> getHighlightedEdges() {
            return highlightedEdges;
        }
        public void setHighlightedEdges(Dictionary<Edge<object>, bool> dict) {
            highlightedEdges = dict;
        }

        public Dictionary<Edge<object>, bool> getSignedEdges() {
            return signedEdges;
        }
        public void setSignedEdges(Dictionary<Edge<object>, bool> dict) {
            signedEdges = dict;
        }

        private IBidirectionalGraph<object, IEdge<object>> _graph;
        public IBidirectionalGraph<object, IEdge<object>> Graph {
            get { return _graph; }
            set { _graph = value; }
        }

       
        public Vertex[] vertices { get { return _vertices; } set { this._vertices = value; } }
        public double myWidth { get { return _width; } set { this._width = value; } }
        public double myHeight { get { return _height; } set { this._height = value; } }
        public  enum MouseMode {Normal,AddVertex,AddEdge,SetSinkSource,Remove };

        DependencyPropertyDescriptor  dpdTop = DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(FrameworkElement));
        DependencyPropertyDescriptor dpdLeft = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(FrameworkElement));
        EventHandler vertexPositionHandler;
        private MouseMode _mouseMode;
        public MouseMode mouseMode {
            get { return _mouseMode;}
            set { 
                _mouseMode=value;
                selectedVertex = null;
            }
        }

        private VertexAdd VertexAddWindow = null;
        private FlowCapacityWindow _FlowCapacityWindow = null;
        private Vertex selectedVertex = null;
        private Edge selectedEdge = null;
        private string oldSource = null;
        public Label TipLabel {
            get;
            set;
        }
        private int counter = 0;
        private Brush edgCol = null;

        #endregion Variables, constants etc.

        public GraphCanva() {
            InitializeComponent();
            myHeight = 100;
            myWidth = 100;
            DependencyProperty.RegisterAttachedReadOnly("CanDragProperty", typeof(bool), typeof(DraggableExtender), new UIPropertyMetadata());
           vertexPositionHandler = new EventHandler(vertexPositionHandlerFunction);
           var g = new BidirectionalGraph<object, IEdge<object>>();
           Graph = g;
            mouseMode=MouseMode.Normal;
            updateLayoutEdges();
            this.MouseDown += new MouseButtonEventHandler(GraphCanva_MouseDown);
            this.Background = Brushes.White;

        }

        #region Layout utils
        public void vertexPositionHandlerFunction(object o, EventArgs e) {
            if ((double)((Vertex)o).GetValue(Canvas.LeftProperty) < 0) {
                ((Vertex)o).SetValue(Canvas.LeftProperty, 0d);
                ((Vertex)o).X = 0d;
            }
            if ((double)((Vertex)o).GetValue(Canvas.LeftProperty) > Width-37d) {
                ((Vertex)o).SetValue(Canvas.LeftProperty, Width - 38d);
                ((Vertex)o).X = Width-38d;
            }
            if ((double)((Vertex)o).GetValue(Canvas.TopProperty) < 0) {
                ((Vertex)o).SetValue(Canvas.TopProperty, 0d);
                ((Vertex)o).Y = 0d;
            }
            if ((double)((Vertex)o).GetValue(Canvas.TopProperty) > Height - 37d) {
                ((Vertex)o).SetValue(Canvas.TopProperty, Height - 38d);
                ((Vertex)o).Y = Height - 38d;
            }
            ((Vertex)o).X = (double)((Vertex)o).GetValue(Canvas.LeftProperty);
            ((Vertex)o).Y = (double)((Vertex)o).GetValue(Canvas.TopProperty);

            updateLayoutEdges();
        }

        public void addVertex(string name,double x, double y) {
            //dodać sprawdzanie czy dany vertex już istnieje
            Vertex vx = new Vertex();
            vx.BodyText = name;
            vx.X = x;
            vx.Y = y;
            vx.SetValue(Canvas.TopProperty, y);
            vx.SetValue(Canvas.LeftProperty, x);
            vx.SetValue(DraggableExtender.CanDragProperty, true);
            vx.SetValue(Canvas.ZIndexProperty, 10);
            vx.BgColor = Brushes.White;
           // vx.Background = Brushes.White;
            dpdLeft.AddValueChanged(vx, vertexPositionHandler);
            dpdTop.AddValueChanged(vx, vertexPositionHandler);
            vx.MouseDoubleClick += new MouseButtonEventHandler(vx_MouseDoubleClick);
            vx.button.Click += new RoutedEventHandler(vx_Click);
            Vertex[] tab= new Vertex[vertices.Length+1];
            for (int i = 0; i < vertices.Length;i++ ) {
                tab[i] = vertices[i];
            }
            tab[vertices.Length] = vx;
            vertices = new Vertex[tab.Length];
            vertices = tab;

            var g= new BidirectionalGraph<object, IEdge<object>>();
            g = (BidirectionalGraph<object, IEdge<object>>)Graph;
            g.AddVertex(vx);
            Graph = g;
            canva.Children.Add(((Vertex)vx));
            counter++;
        }

        public void removeVertex(string name) {
            if (getVertexByName(name) == null) return;
            var g = (BidirectionalGraph<object, IEdge<object>>)Graph;
            IEnumerator itr = g.Edges.GetEnumerator();
            while(itr.MoveNext()){
                if (((Vertex)(((Edge<object>)itr.Current).Source)).BodyText.CompareTo(name) == 0 ||
                    ((Vertex)(((Edge<object>)itr.Current).Target)).BodyText.CompareTo(name) == 0) {
                    removeEdge(((Vertex)(((Edge<object>)itr.Current).Source)).BodyText, ((Vertex)(((Edge<object>)itr.Current).Target)).BodyText);
                    itr = g.Edges.GetEnumerator();
                }
            }
            List<Vertex> temp = vertices.ToList<Vertex>();
            temp.Remove(getVertexByName(name));
            this.canva.Children.Remove(getVertexByName(name));
            this.canva.UpdateLayout();
            vertices = temp.ToArray();
           
            g.RemoveVertex(getVertexByName(name));
            Graph = g;
            if (Graph.VertexCount == 0) counter = 0;
        }

        /*public void updateLayoutVertices() {
           foreach( Vertex vx in Graph.Vertices){
               vertexPositionHandlerFunction(vx, new EventArgs());
           }
        }*/

        public Vertex getVertexByName(string name) {
            foreach (Vertex v in Graph.Vertices) {
                if (String.Compare(v.BodyText, name) == 0) {
                    return v;
                }
            }
            return null;
        }


        public void addEdge(string from, string to, int flow, int capacity) {
            var g = new BidirectionalGraph<object, IEdge<object>>();
            g = (BidirectionalGraph<object, IEdge<object>>)Graph;
            if (String.Compare(from, to) == 0) {
                throw new Exception("Names of vertices are the same");
            }
            Vertex va = getVertexByName(from);
            Vertex vb = getVertexByName(to);
            if (va != null && vb != null) {
                QuickGraph.Edge<object> edg = new Edge<object>(va, vb);
                fce.Add(edg, new FlowCapacityPair(flow, capacity));
                g.AddEdge(edg);
                Graph = g;
                updateLayoutEdges();
            } else {
                throw new Exception("No such vertices");
            }
        }

        public void removeEdge(string from, string to) {
            var g = (BidirectionalGraph<object, IEdge<object>>)Graph;
            Edge<object> edg = getEdgeByName(from, to);
            if (edg == null) {
                throw new Exception("No such edge");
            }
            g.RemoveEdge(edg);
            fce.Remove(edg);
            Graph = g;
            updateLayoutEdges();
        }

        public void updateLayoutEdges() {

            IEnumerator itr = canva.Children.GetEnumerator();
            FlowCapacityPair fc = new FlowCapacityPair();
            while (itr.MoveNext()) {
                if (itr.Current.GetType() == typeof(Edge)) {
                    canva.Children.RemoveAt(canva.Children.IndexOf((Edge)itr.Current));
                    canva.UpdateLayout();
                    itr = canva.Children.GetEnumerator();
                }
            }
            if (Graph.EdgeCount == 0) {
                return;
            }
            foreach (QuickGraph.Edge<object> e in Graph.Edges) {


                double x0 = ((Vertex)e.Source).get_XCenter();
                double y0 = ((Vertex)e.Source).get_YCenter();
                double x1 = ((Vertex)e.Target).get_XCenter();
                double y1 = ((Vertex)e.Target).get_YCenter();
                double alfa = (360d / (2d * Math.PI)) * Math.Atan2((y1 - y0), x1 - x0);
                double len = (Math.Sqrt(Math.Pow(y1 - y0, 2) + Math.Pow(x1 - x0, 2)) - 15d);

                Edge edg = new Edge(len);

                if (highlightedEdges.ContainsKey(e) == true) {
                    if (highlightedEdges[e] == true) {
                        edg.isHighlighted = true;
                    }
                }
                if (signedEdges.ContainsKey(e) == true) {
                    if (signedEdges[e] == true) {
                        edg.EdgeColor = Brushes.BlueViolet;
                    }
                }

                if (fce.ContainsKey(e)) {
                    fc = fce[e];
                }
                edg.Flow = fc.Flow;
                edg.Capacity = fc.Capacity;
                edg.Angle = alfa;
                edg.SetValue(Canvas.TopProperty, y0 - 4d);
                edg.SetValue(Canvas.LeftProperty, x0);
                edg.SetValue(Canvas.ZIndexProperty, 0);
                edg.label.SetValue(Canvas.LeftProperty, (x1 - x0) / 2d);
                edg.label.SetValue(Canvas.TopProperty, (y1 - y0) / 2d);
                edg.label.SetValue(Canvas.ZIndexProperty, 10);
                edg.graphEdge = e;
                edg.MouseDoubleClick += new MouseButtonEventHandler(edg_MouseDoubleClick);
                edg.MouseDown += new MouseButtonEventHandler(edg_MouseDown);
                edg.MouseEnter += new MouseEventHandler(edg_MouseEnter);
                edg.MouseLeave += new MouseEventHandler(edg_MouseLeave);
                canva.Children.Add(edg);
            }
        }

        public Edge<object> getEdgeByName(string from, string to) {
            foreach (Edge<object> edg in Graph.Edges) {
                if (((Vertex)(edg.Source)).BodyText.CompareTo(from) == 0 &&
                    ((Vertex)(edg.Target)).BodyText.CompareTo(to) == 0) {
                    return edg;
                }
            }
            return null;
        }
      
     
        public void clearGraph() {
            var g = (BidirectionalGraph<object, IEdge<object>>)Graph;
            resetSelectedVertex();
            canva.Children.Clear();
            //updateLayoutEdges();
            g.Clear();
            Graph = g;
            vertices = new Vertex[0];
            fce.Clear();
            Source = null;
            Sink = null;
            highlightedEdges = new Dictionary<Edge<object>, bool>();
            fce = new Dictionary<Edge<object>, FlowCapacityPair>();
            counter = 0;

        }
        #endregion Layout utils

        #region Mouse handlig

        void vx_Click(object sender, RoutedEventArgs e) {
            if (mouseMode == MouseMode.AddEdge) {
                if (selectedVertex == null) {
                    selectedVertex = getVertexByName((string)(((Button)sender).Content));
                    if (TipLabel != null) TipLabel.Content = "Select vertex to which you want direct edge";

                } else {
                    try {
                        if (Graph.ContainsEdge(selectedVertex, getVertexByName((string)(((Button)sender).Content))) == true) {
                            return;
                        }
                        if (Graph.ContainsEdge(getVertexByName((string)(((Button)sender).Content)), selectedVertex) == true) {
                            return;
                        }
                        addEdge(selectedVertex.BodyText, getVertexByName((string)(((Button)sender).Content)).BodyText, 0, 0);
                        selectedVertex = null;
                        if (TipLabel != null) TipLabel.Content = "Select vertex from which you want to direct edge";
                    } catch { }
                }
            } else if (mouseMode == MouseMode.SetSinkSource) {
                if (selectedVertex == null) {
                    if (this.Source != null) {
                        getVertexByName(Source).BgColor = Brushes.White;
                        oldSource = this.Source;
                    }
                    selectedVertex = getVertexByName((string)(((Button)sender).Content));
                    selectedVertex.BgColor = Brushes.Yellow;
                    this.Source = selectedVertex.BodyText;
                    if (TipLabel != null) TipLabel.Content = "Select sink";

                } else {

                    Vertex newSelectedVertex = getVertexByName((string)(((Button)sender).Content));
                    if (this.Sink != null && newSelectedVertex.BodyText.CompareTo(this.oldSource!=null?this.oldSource:"")!=0) getVertexByName(Sink).BgColor = Brushes.White;

                    if (newSelectedVertex == selectedVertex) {
                        return;
                    }
                    newSelectedVertex.BgColor = Brushes.SkyBlue;
                    this.Sink = newSelectedVertex.BodyText;
                    if (TipLabel != null) TipLabel.Content = "";
                    selectedVertex = null;
                    newSelectedVertex = null;
                    mouseMode = MouseMode.Normal;
                    this.Cursor = Cursors.Arrow;
                }
            } else if (mouseMode == MouseMode.Remove) {
                removeVertex((string)(((Button)sender).Content));
            }
        }

        void vx_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            selectedVertex = (Vertex)sender;
            VertexAddWindow = new VertexAdd();
            VertexAddWindow.okButton.Click += new RoutedEventHandler(okButtonVertexAdd_Click);
            VertexAddWindow.cancelButton.Click += new RoutedEventHandler(cancelButtonVertexAdd_Click);
            VertexAddWindow.Unloaded += new RoutedEventHandler(cancelButtonVertexAdd_Click);
            VertexAddWindow.Show();
            VertexAddWindow.nameTxt.Focus();
            //root.IsEnabled = false;

        }

        void edg_MouseLeave(object sender, MouseEventArgs e) {
            if (edgCol != null) {
                ((Edge)sender).EdgeColor = edgCol;
            }
        }

        void edg_MouseEnter(object sender, MouseEventArgs e) {
            
                edgCol = ((Edge)sender).EdgeColor;
            ((Edge)sender).EdgeColor = Brushes.CadetBlue;
        }

        void edg_MouseDown(object sender, MouseButtonEventArgs e) {
            if (mouseMode == MouseMode.Remove) {
                removeEdge(((Vertex)((Edge)sender).graphEdge.Source).BodyText, ((Vertex)((Edge)sender).graphEdge.Target).BodyText);
            }
        }

        void edg_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            //throw new NotImplementedException();
            selectedEdge = (Edge)sender;
            _FlowCapacityWindow = new FlowCapacityWindow();
            _FlowCapacityWindow.buttonOk.Click += new RoutedEventHandler(buttonOkFC_Click);
            _FlowCapacityWindow.buttonCancel.Click += new RoutedEventHandler(buttonCancelFC_Click);
            _FlowCapacityWindow.Unloaded += new RoutedEventHandler(buttonCancelFC_Click);
            _FlowCapacityWindow.textBoxFlow.Text = selectedEdge.Flow.ToString();
            _FlowCapacityWindow.textBoxCapacity.Text = selectedEdge.Capacity.ToString();
            _FlowCapacityWindow.Show();
            _FlowCapacityWindow.textBoxFlow.Focus();
        }

        void GraphCanva_MouseDown(object sender, MouseButtonEventArgs e) {
            Point position = new Point();
            // position = e.GetPosition(this);
            //addVertex(this.Graph.VertexCount.ToString(), position.X-16d, position.Y-16d);

            if (mouseMode == MouseMode.AddVertex) {
                position = e.GetPosition(this);
                addVertex(counter.ToString(), position.X - 16d, position.Y - 16d);
            } else if (mouseMode == MouseMode.AddEdge) {

            } else if (mouseMode == MouseMode.Normal) {

            }
        }

        #endregion Mouse handling

        #region Additional windows
        void buttonCancelFC_Click(object sender, RoutedEventArgs e) {
            if(_FlowCapacityWindow!=null) _FlowCapacityWindow.Close();
            selectedEdge = null;
            if (_FlowCapacityWindow != null) _FlowCapacityWindow = null;
        }

        void buttonOkFC_Click(object sender, RoutedEventArgs e) {
            String flowStr = _FlowCapacityWindow.textBoxFlow.Text;
            String capacityStr = _FlowCapacityWindow.textBoxCapacity.Text;
            

            if (flowStr == "" || capacityStr == "") {
                MessageBox.Show("All fields must be filled", "Empty fields warning");
                return;
            }
            
            if (Convert.ToInt16(flowStr) < 0) {
                MessageBox.Show("Flow can not be less than 0", "Incorrect flow");
                return;
            }
            if(Convert.ToInt16(capacityStr) <0){
                   MessageBox.Show("Capacity can not be less than 0", "Incorrect capacity");
                return;
            }
            if (Convert.ToInt16(flowStr) > Convert.ToInt16(capacityStr)) {
                MessageBox.Show("Flow can not be greater than capacity", "Incorrect flow or capacity");
                return;
            }
            selectedEdge.Flow = Convert.ToInt16(flowStr);
            selectedEdge.Capacity = Convert.ToInt16(capacityStr);
            fce[selectedEdge.graphEdge] = new FlowCapacityPair(Convert.ToInt16(flowStr), Convert.ToInt16(capacityStr));
            updateLayoutEdges();
            selectedEdge = null;
            _FlowCapacityWindow.Close();
        }

        void okButtonVertexAdd_Click(object sender, RoutedEventArgs e) {
            //root.IsEnabled = true;
            string txt = VertexAddWindow.nameTxt.Text;
            if (txt == "") {
                MessageBox.Show("You did not specify name");
                return;
            } else {
                foreach (Vertex v in Graph.Vertices) {
                    if (v.BodyText.CompareTo(txt) == 0) {
                        MessageBox.Show("Vertex with such name already exist", "Wrong vertex name");
                        return;
                    }
                }
                selectedVertex.BodyText = txt;
            }
            VertexAddWindow.Close();
            selectedVertex = null;
            //VertexAddWindow = null;
        }

        void cancelButtonVertexAdd_Click(object sender, RoutedEventArgs e) {
            VertexAddWindow.Close();
            selectedVertex = null;
            // VertexAddWindow = null;
        }
        #endregion Additional windows

        #region Functional methods
        public void importGraphFromNetwork(network net) {
            clearGraph();
            if(net.widthSpecified) this.Width = net.width;

            if (net.heightSpecified) this.Height = net.height;

            string source = null;
            string sink = null;
            if (net.node != null)
                foreach (networkNode node in net.node) {
                    addVertex(node.id, node.positionxSpecified ? (double)node.positionx : 0, node.positionySpecified ? (double)node.positiony : 0);
                    if (node.isSourceSpecified && node.isSource) {
                        source = node.id;
                    }
                    if (node.isSinkSpecified && node.isSink) {
                        sink = node.id;
                    }
                }
            this.setSinkAndSource(source, sink);
            if (net.edge != null)
                foreach (networkEdge edg in net.edge) {
                    addEdge(edg.from, edg.to, edg.flowSpecified ? edg.flow : 0, edg.capacitySpecified ? edg.capacity : 0);
                }
        }

        public void setSinkAndSource(String source, String sink) {
            if (this.Source != null) getVertexByName(this.Source).BgColor = Brushes.White;
            if (this.Sink != null) getVertexByName(this.Sink).BgColor = Brushes.White;
            if(getVertexByName(source)!=null)
            getVertexByName(source).BgColor = Brushes.Yellow;

            if(getVertexByName(sink)!=null)
            getVertexByName(sink).BgColor = Brushes.SkyBlue;
            this.Source = source;
            this.Sink = sink;

        }

        public void resetSelectedVertex() {
            selectedVertex = null;
            this.mouseMode = MouseMode.Normal;
            this.Cursor = Cursors.Arrow;
            TipLabel.Content = "";

        }

        public Vertex[] getVertices() {
            return vertices;
        }

        #endregion Functional methods


        //public void initializeGraph() {


        //    var g = new BidirectionalGraph<object, IEdge<object>>();

        //    Vertex[] vtab = new Vertex[4];
        //    for (int i = 0; i < 4; i++) {
        //        vtab[i] = new Vertex();
        //        vtab[i].BodyText = Convert.ToString(i);
        //        vtab[i].SetValue(DraggableExtender.CanDragProperty, true);
        //        vtab[i].SetValue(Canvas.ZIndexProperty, 10);
        //    }
        //    vtab[0].X = 20;
        //    vtab[0].Y = 20;

        //    vtab[1].X = 70;
        //    vtab[1].Y = 20;

        //    vtab[2].X = 80;
        //    vtab[2].Y = 80;

        //    vtab[3].X = 20;
        //    vtab[3].Y = 100;

        //    for (int i = 0; i < 4; i++) {
        //        vtab[i].SetValue(Canvas.LeftProperty, vtab[i].X);
        //        vtab[i].SetValue(Canvas.TopProperty, vtab[i].Y);
        //        g.AddVertex(vtab[i]);
        //        //canva.Children.Add(vtab[i]);

        //        dpdTop.AddValueChanged(vtab[i], vertexPositionHandler);
        //        dpdLeft.AddValueChanged(vtab[i], vertexPositionHandler);

        //    }
        //    vertices = vtab;
        //    g.AddEdge(new Edge<object>(vtab[0],vtab[1]));
        //    g.AddEdge(new Edge<object>(vtab[1], vtab[2]));
        //    g.AddEdge(new Edge<object>(vtab[2], vtab[3]));
        //    g.AddEdge(new Edge<object>(vtab[3], vtab[0]));
        //    g.AddEdge(new Edge<object>(vtab[0], vtab[2]));
        //    g.AddEdge(new Edge<object>(vtab[3], vtab[1]));
        //    Graph = g;
        //    foreach (object v in Graph.Vertices) {
        //        ((Vertex)v).SetValue(Canvas.TopProperty, ((Vertex)v).Y);
        //        ((Vertex)v).SetValue(Canvas.LeftProperty, ((Vertex)v).X);
        //        canva.Children.Add(((Vertex)v));
        //    }

        //}

    }
}

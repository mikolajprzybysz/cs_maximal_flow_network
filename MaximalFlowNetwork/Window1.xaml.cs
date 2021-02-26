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
using GraphSharp.Controls;
using GraphSharp.Controls.Animations;
using QuickGraph.Algorithms.Observers;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace MaximalFlowNetwork {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window {

        private bool isStepByStep = false;
        private ManualResetEvent mer=new ManualResetEvent(false);
        private ManualResetEvent endOfCalc = new ManualResetEvent(false);
        ThreadStart ths; 
        Thread th; 
        ThreadStart ths1;
        Thread th1;
        private string tempGraph = null;

        public Window1() {
            
            InitializeComponent();
            graphCanva.Background = Brushes.White;
            graphCanva.TipLabel = this.TipLabel;

        }

        #region Functionality methods
        
        private Edge<object> getKey(object source , object target,Dictionary<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> dict) {
            foreach (Edge<object> edg in dict.Keys) {
                if (edg.Source == source && edg.Target == target) {
                    return edg;
                }
            }
            return null;
        }
        
        private void saveToTemp() {
            tempGraph = XML_Util.ToXML(graphCanva.Graph, graphCanva.getFCE(),graphCanva.Source,graphCanva.Sink, Convert.ToInt32(graphCanva.Width), Convert.ToInt32(graphCanva.Height));

        }

        public static bool IsTextAllowed(string text) {
            Regex regex = new Regex("[^0-9]"); //regex that matches disallowed text
            return regex.IsMatch(text);
        }

        #endregion Functionality methods

        #region Calculation part of application
        /// <summary>
        /// Function is checking correctness of flow in graph
        /// </summary>
        /// <param name="graph">Graph in which flow is going to be checked</param>
        /// <param name="source">Source in graph</param>
        /// <param name="sink">Sink in graph</param>
        /// <param name="fce">Dictionary where keys are edges and values flow and capacity in edges</param>
        /// <returns>List of vertices where input flow does not fit to outputflow</returns>
        private List<Vertex> checkFlow(BidirectionalGraph<object, IEdge<object>> graph, string source, string sink, Dictionary<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> fce) {
            bool isCorrect=true;
            IEnumerable< KeyValuePair<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> >kvpIn;
            IEnumerable< KeyValuePair<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> >kvpOut;
            List<Vertex> temp = new List<Vertex>();
            int flowIn = 0;
            int flowOut = 0;

            foreach (Vertex vc in graph.Vertices) {
                if (vc == graphCanva.getVertexByName(source) || vc == graphCanva.getVertexByName(sink)) {
                    continue;
                }
                kvpIn =fce.Where(edg => edg.Key.Target == vc);
                kvpOut = fce.Where(edg =>edg.Key.Source == vc);
                foreach (KeyValuePair<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> kvp in kvpIn) {
                    flowIn += kvp.Value.Flow;
                }
                foreach (KeyValuePair<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> kvp in kvpOut) {
                    flowOut+=kvp.Value.Flow;
                }
                if (flowIn != flowOut) {
                    isCorrect = false;
                    temp.Add(vc);
                }
                flowIn = 0;
                flowOut = 0;
            }
            if (isCorrect) {
                return null;
            } else {
                return temp;
            }
        }

        private void highlightAugmentingPath(Dictionary<Edge<object>, int> path, BidirectionalGraph<object, IEdge<object>> graph) {
            graphCanva.setHighlightedEdges(new Dictionary<Edge<object>, bool>());
            foreach (Edge<object> edg in graph.Edges) {
                graphCanva.getHighlightedEdges().Add(edg, false);
            }
            foreach(Edge<object> edg in path.Keys){
                if (graphCanva.getFCE().ContainsKey(edg)) {
                    graphCanva.getHighlightedEdges()[edg] = true;
                } else {
                    graphCanva.getHighlightedEdges()[getKey(edg.Target, edg.Source, graphCanva.getFCE())] = true;
                }
            }
        }
        
        private Dictionary<Edge<object>, int> getPath(BidirectionalGraph<object, IEdge<object>> rgraph, BidirectionalGraph<object, IEdge<object>> graph, string source, string sink, Dictionary<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> fce) {
            BidirectionalGraph<object, IEdge<object>> g = rgraph;
            //var g = new BidirectionalGraph<object,IEdge<object>>();

            var bfs = new QuickGraph.Algorithms.Search.BreadthFirstSearchAlgorithm<object, IEdge<object>>(g);
            var observer = new VertexPredecessorRecorderObserver<object, IEdge<object>>();
            Dictionary<Edge<object>, int> dict = new Dictionary<Edge<object>, int>();

            Vertex v = null;
            graphCanva.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
          new Action(
            delegate() {
                v = graphCanva.getVertexByName(sink);
            }
            )
                );

            IEnumerable<IEdge<object>> edges = null;
            bool isEnded = false;
            int c;
            observer.Attach(bfs);
            Vertex s = null;
            while (!isEnded) {
                graphCanva.Dispatcher.Invoke(
               System.Windows.Threading.DispatcherPriority.Normal,
         new Action(
           delegate() {
               bfs.Compute(graphCanva.getVertexByName(source));
           }));
                if (observer.TryGetPath(v, out edges)) {

                    foreach (Edge<object> edg in edges) {

                        if (graph.ContainsEdge(edg)) {    //zgodnie z kierunkiem
                            c = (fce[edg].Capacity - fce[edg].Flow);
                        } else {                          //przeciwnie do kierunku
                            c = fce[getKey(edg.Target, edg.Source, fce)].Flow;
                        }

                        if (c <= 0) {

                            //g.RemoveEdge(edg);
                            foreach (Edge<object> e in g.Edges) {
                                if (e.Source == edg.Source && e.Target == edg.Target) {
                                    g.RemoveEdge(e);
                                    break;
                                }
                            }
                            //observer.Detach(bfs);
                            bfs = new QuickGraph.Algorithms.Search.BreadthFirstSearchAlgorithm<object, IEdge<object>>(g);
                            observer = new VertexPredecessorRecorderObserver<object, IEdge<object>>();
                            observer.Attach(bfs);
                            //observer.Attach(bfs);
                            isEnded = false;
                            dict.Clear();
                            edges = null;
                            break;
                        } else {
                            dict.Add(edg, c);
                            isEnded = true;
                        }
                    }

                } else {
                    return null;
                }
            }

            return dict;
        }

        #region Description of algorithm
        //Edmonds-Karp algorithm:
        //Inputs Graph G with flow capacity c, a source node s, and a sink
        //node t
        //Output A maximum flow f from s to t

        //WHILE there is a path p from s to t (found by Breadth First
        //Search) in Gf
        //(where: Gf is the network with capacity cf(u,v) and no flow,
        //where cf(u,v) equals:
        //• cf(u,v) := c(u,v) − f(u,v) if edge (u,v) is a forward arc on p
        //• f(u,v) if edge (u,v) is a backward arc on p
        //(where c(u,v) is a capacity between u and v in G and f(u,v)
        //is a flow between u and v in G)),
        //such that cf(u,v)>0
        //DO
        //1.Find cf(p) = min {cf(u,v)|(u,v) is in p}
        //2.FOR EACH edge (u,v) in p DO
        //IF(edge is on forward arc) THEN
        //f(u,v) :=f(u,v) + cf(p)
        //ELSE
        //f(u,v) :=f(u,v) - cf(p)
        #endregion Description of algorithm

        private int calculateMaximalFlow(BidirectionalGraph<object, IEdge<object>> graph,string source, string sink) {
            var rgraph = new BidirectionalGraph<object, IEdge<object>>(true);
            //IBidirectionalGraph<object, IEdge<object>> g =gr;
            Dictionary<Edge<object>,MaximalFlowNetwork.GraphCanva.FlowCapacityPair> dict = graphCanva.getFCE();
            Dictionary<Edge<object>, int> path = null ;


            //tworzenie rgraph
            rgraph.AddVerticesAndEdgeRange(graph.Edges);
            foreach (Edge<object> edg in graph.Edges) {
                rgraph.AddEdge(new Edge<object>(edg.Target,edg.Source));
            }

            while ((path = getPath(rgraph, graph, source, sink, dict)) != null) {
                graphCanva.setSignedEdges(new Dictionary<Edge<object>, bool>());
                if (isStepByStep) {
                    highlightAugmentingPath(path, graph);
                    this.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            button9.Focus();
                        }));

                    mer.WaitOne();
                    graphCanva.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate() {
                        graphCanva.updateLayoutEdges();
                    }));
                    mer.Reset();
                }
                int cMin = int.MaxValue;
                foreach (Edge<object> edg in path.Keys) {
                    cMin = Math.Min(path[edg], cMin);
                }
                if (isStepByStep) {
                    this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate() {
                        this.flowInfoLabel.Content = cMin.ToString();
                    }));
                }
                foreach (Edge<object> edg in path.Keys) {
                    if (graph.ContainsEdge(edg)) {//zgodnie z kierunkiem
                        //dict[edg].setFlow(dict[edg].Flow + cMin);
                        dict[edg] = new GraphCanva.FlowCapacityPair(dict[edg].Flow + cMin, dict[edg].Capacity);
                    } else {//pod prąd

                        //dict[new Edge<object>(edg.Target, edg.Source)].setFlow(dict[edg].Flow + cMin);
                        //dict[getKey(edg.Target, edg.Source, dict)].setFlow(dict[getKey(edg.Target, edg.Source, dict)].Flow - cMin);
                        dict[getKey(edg.Target, edg.Source, dict)] = new GraphCanva.FlowCapacityPair(dict[getKey(edg.Target, edg.Source, dict)].Flow-cMin,dict[getKey(edg.Target, edg.Source, dict)].Capacity);
                    }
                    graphCanva.setFCE(dict);
                    
                    
                    if (!isStepByStep) {
                       /* graphCanva.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            graphCanva.updateLayoutEdges();
                            
                        }));*/
                     //   Thread.Sleep(500);
                    } else {
                        if (graphCanva.getFCE().ContainsKey(edg)) {
                            graphCanva.getSignedEdges()[edg] = true;
                        } else {
                            graphCanva.getSignedEdges()[getKey(edg.Target, edg.Source, graphCanva.getFCE())] = true;
                        }

                        this.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            button9.Focus();
                        }));

                        mer.WaitOne();
                        graphCanva.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            graphCanva.updateLayoutEdges();
                        }));
                        mer.Reset();
                    }
                }
                
            }
            // MessageBox.Show("Maximal flow equals:" + 
           // endOfCalc.Set();

            graphCanva.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            graphCanva.setSignedEdges(new Dictionary<Edge<object>, bool>());
                            graphCanva.updateLayoutEdges();

                        }));

            endOfCalc.Set();
            return 0;
        }
      
        private void startCalculation() {
            calculateMaximalFlow((BidirectionalGraph<object, IEdge<object>>)graphCanva.Graph, graphCanva.Source, graphCanva.Sink);
        }
       
        private void endCalculation() {
            endOfCalc.WaitOne();
            if(th.IsAlive)
            th.Join();
            IEnumerator itr = graphCanva.getHighlightedEdges().Keys.GetEnumerator();
            foreach (Edge<object> edg in graphCanva.Graph.Edges) {
                graphCanva.getHighlightedEdges()[edg] = false;
            }
            int maxflow = 0;
            graphCanva.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            graphCanva.updateLayoutEdges();
                            if (this.button9.Visibility == Visibility.Visible) this.button9.Visibility = Visibility.Collapsed;
                            var g = (BidirectionalGraph<object, IEdge<object>>)graphCanva.Graph;
                            IEnumerator itr1 = g.Edges.GetEnumerator();

                            while (itr1.MoveNext()) {
                                if (((Vertex)(((Edge<object>)itr1.Current).Target)).BodyText.CompareTo(graphCanva.Sink) == 0) {
                                    maxflow += graphCanva.getFCE()[(Edge<object>)itr1.Current].Flow;
                                    itr = g.Edges.GetEnumerator();
                                }
                            }
                        }));

            MessageBox.Show("Maximal flow equals:" + maxflow.ToString(), "End of calculation");
            graphCanva.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate() {
                            graphCanva.IsEnabled = true ;
                            button11.Visibility = Visibility.Visible;
                            button4.Visibility = Visibility.Visible;
                            this.FlowInfo_stackPanel.Visibility = Visibility.Hidden;
                            this.flowInfoLabel.Content = "";
                        }));
        }

        #endregion Calculation part of application

        #region GUI

        private void radioButtonResult_Checked(object sender, RoutedEventArgs e) {
            isStepByStep = false;
        }

        private void radioButtonStep_Checked(object sender, RoutedEventArgs e) {
            isStepByStep = true;
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e) {
            textBoxHeight.Text = "500";
            textBoxWidth.Text = "500";
            button11.Visibility = Visibility.Hidden;
            graphCanva.clearGraph();
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e) {
            button11.Visibility = Visibility.Hidden;
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true) {
                // Open document
                string filename = dlg.FileName;
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                string str = file.ReadToEnd();
                file.Close();
                network n = XML_Util.ToNetwork(str);
                textBoxWidth.Text = n.widthSpecified ? Convert.ToString( n.width) : "";
                textBoxHeight.Text = n.heightSpecified ? Convert.ToString(n.height) : "";
                graphCanva.importGraphFromNetwork(n);
            }


        }

        private void buttonSave_Click(object sender, RoutedEventArgs e) {
            button11.Visibility = Visibility.Hidden;           
            graphCanva.resetSelectedVertex();
            string s = XML_Util.ToXML(graphCanva.Graph, graphCanva.getFCE(),graphCanva.Source,graphCanva.Sink, Convert.ToInt32(graphCanva.Width), Convert.ToInt32(graphCanva.Height));
            // Write the string to a file.


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true) {
                // Save document
                string filename = dlg.FileName;
                System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
                file.Write(s);
                file.Close();
            }




        }

        private void buttonStart_Click(object sender, RoutedEventArgs e) {
           
            this.saveToTemp();
            graphCanva.resetSelectedVertex();

            if (graphCanva.Source == null || graphCanva.Sink == null) {
                MessageBox.Show("You must first specify source and sink in settings");
                return;
            }
            ths = new ThreadStart(startCalculation);
            th = new Thread(ths);
            ths1 = new ThreadStart(endCalculation);
            th1 = new Thread(ths1);

            //th.Start();
            foreach (Vertex vx in graphCanva.Graph.Vertices) {
                if (vx != graphCanva.getVertexByName(graphCanva.Source) && vx != graphCanva.getVertexByName(graphCanva.Sink)) {
                    vx.isHighlighted = false;
                }
            }
            List<Vertex> vc = checkFlow((BidirectionalGraph<object, IEdge<object>>)graphCanva.Graph, graphCanva.Source, graphCanva.Sink, graphCanva.getFCE());
            if (vc == null) {
                button11.Visibility = Visibility.Hidden;
                button4.Visibility = Visibility.Hidden;
                //startCalculation();
                if (radioButton1.IsChecked == true) {
                    this.button9.Visibility = Visibility.Visible;
                    this.FlowInfo_stackPanel.Visibility = Visibility.Visible;
                }
                graphCanva.IsEnabled = false;
                th1.Start();
                th.Start();

            } else {
                foreach (Vertex v in vc) {

                    v.isHighlighted = true;

                }
                MessageBoxResult res = MessageBox.Show("Flow in vertices with red colour is incorrect. Please correct flows to perform calculation of maximal flow", "Incorrect flow");

            }
        }

        private void buttonRestart_Click(object sender, RoutedEventArgs e) {
            if (tempGraph != null) {
                network n = XML_Util.ToNetwork(tempGraph);
                graphCanva.importGraphFromNetwork(n);
            }
            button11.Visibility = Visibility.Hidden;
        }

        private void buttonNormal_Click(object sender, RoutedEventArgs e) {
            graphCanva.resetSelectedVertex();
        }

        private void buttonAddVertex_Click(object sender, RoutedEventArgs e) {
            button11.Visibility = Visibility.Hidden;
            graphCanva.resetSelectedVertex();
            TipLabel.Content = "Click at the place you want to put vertex. Double click on vertex to change name";
            graphCanva.mouseMode = GraphCanva.MouseMode.AddVertex;
            graphCanva.Cursor = Cursors.Cross;
        }

        private void buttonAddEdge_Click(object sender, RoutedEventArgs e) {
            button11.Visibility = Visibility.Hidden;
            graphCanva.resetSelectedVertex();
            TipLabel.Content = "Select vertex from which you want to direct edge. Double click on edge to change flow and capacity";
            graphCanva.mouseMode = GraphCanva.MouseMode.AddEdge;
            graphCanva.Cursor = Cursors.UpArrow;
        }

        private void buttonSelectSinkAndSource_Click(object sender, RoutedEventArgs e) {
            button11.Visibility = Visibility.Hidden;
            graphCanva.resetSelectedVertex();
            graphCanva.mouseMode = GraphCanva.MouseMode.SetSinkSource;
            graphCanva.Cursor = Cursors.Cross;
            this.TipLabel.Content = "Select source";
        }

        private void buttonStep_Click(object sender, RoutedEventArgs e) {
            mer.Set();
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e) {
            button11.Visibility = Visibility.Hidden;
            graphCanva.resetSelectedVertex();
            this.TipLabel.Content = "Select element you want to remove";
            graphCanva.mouseMode = GraphCanva.MouseMode.Remove;
            graphCanva.Cursor = Cursors.Cross;
        }

        private void root_Unloaded(object sender, RoutedEventArgs e) {
            Process.GetCurrentProcess().Close();
            root.Close();
            
        }
     
        private void root_Closed(object sender, EventArgs e) {
            if (th!=null && th.IsAlive) {
                th.Abort();
            }
            if (th1!=null && th1.IsAlive) {
                th1.Abort();
            }
        }
        
        private void root_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                graphCanva.resetSelectedVertex();
            }
        }

        private void textBoxNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = Window1.IsTextAllowed(e.Text);
        }

        #endregion GUI

        private void textBoxWidth_TextChanged(object sender, TextChangedEventArgs e) {
            if (graphCanva != null && textBoxWidth != null)
                if (textBoxWidth.Text == "") {
                    graphCanva.Width = 500;
                    TipLabel.Content = "Width can not be smaller than 500";
                } else {
                    int w = Convert.ToInt32(this.textBoxWidth.Text);
                    if (w < 500) {
                        graphCanva.Width = 500;
                        TipLabel.Content = "Width can not be smaller than 500";
                    } else {
                        graphCanva.Width = Convert.ToInt32(this.textBoxWidth.Text);
                        TipLabel.Content = "";
                    }
                }
        }

        private void textBoxHeight_TextChanged(object sender, TextChangedEventArgs e) {
            if (graphCanva != null && textBoxHeight != null)
                if (textBoxHeight.Text == "") {
                    graphCanva.Height = 500;
                    TipLabel.Content = "Height can not be smaller than 500";
                } else {
                    int w = Convert.ToInt32(this.textBoxHeight.Text);
                    if (w < 500) {
                        graphCanva.Height = 500;
                        TipLabel.Content = "Height can not be smaller than 500";
                    } else {
                        graphCanva.Height = Convert.ToInt32(this.textBoxHeight.Text);
                        TipLabel.Content = "";
                    }
                }
        }

       

        


        //void SettingsButtonOk_Click(object sender, RoutedEventArgs e) {
        //    graphCanva.resetSelectedVertex();
        //    String source = SettingsWindow.comboBoxSource.Text;
        //    String sink = SettingsWindow.comboBoxSink.Text;
        //    if (sink == "" || source == "") {
        //        MessageBox.Show("Please specify source and sink first");
        //        return;
        //    }
        //    if (sink.CompareTo(source) == 0) {
        //        MessageBox.Show("Sink and source can not be the same vertices");
        //        return;
        //    }
        //    graphCanva.setSinkAndSource(source, sink);

        //}

        //void SettingsWindow_Unloaded(object sender, RoutedEventArgs e) {
        //    SettingsWindow.Close();
        //    root.IsEnabled = true;
        //}
        /* 
          private void buttonAddEdge_Click(object sender, RoutedEventArgs e) {
              String fromStr = EdgeAddWindow.comboBoxFrom.Text;
              String toStr = EdgeAddWindow.comboBoxTo.Text;
              String flowStr = EdgeAddWindow.textBoxFlow.Text;
              String capacityStr = EdgeAddWindow.textBoxCapacity.Text;

              if (fromStr == "" || toStr == "" || flowStr == "" || capacityStr == "") {
                  MessageBox.Show("All fields must be filled", "Empty fields warning");
                  return;
              }
              if (Convert.ToInt16(flowStr) < 0) {
                  MessageBox.Show("Flow can not be less than 0", "Incorrect flow");
                  return;
              }
              if (fromStr.CompareTo(toStr) == 0) {
                  MessageBox.Show("Can not route from one vertex to the same one", "Incorrect edge");
                  return;
              }
              if (Convert.ToInt16(flowStr) > Convert.ToInt16(capacityStr)) {
                  MessageBox.Show("Flow can not be greater than capacity", "Incorrect flow or capacity");
                  return;
              }
              graphCanva.addEdge(fromStr, toStr,Convert.ToInt32(flowStr),Convert.ToInt32(capacityStr));
            
          }
         */

    }
}

using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using QuickGraph;
using System.Windows;

namespace MaximalFlowNetwork {
    public class XML_Util {
        public static string ToXML(
            IBidirectionalGraph<object, IEdge<object>> graph, Dictionary<Edge<object>, MaximalFlowNetwork.GraphCanva.FlowCapacityPair> fce,string source, string sink, int width, int height) {

            MaximalFlowNetwork.GraphCanva.FlowCapacityPair fc = new GraphCanva.FlowCapacityPair();
            var g = new BidirectionalGraph<object, IEdge<object>>();
            g = (BidirectionalGraph<object, IEdge<object>>)graph;
            QuickGraph.Edge<object> edg;
            Vertex vx;

            network net = new network();
            net.edge = new networkEdge[g.EdgeCount];
           
            for (int i = 0; i < g.EdgeCount; i++) {
                edg = (QuickGraph.Edge<object>)g.Edges.ElementAt(i);
                net.edge[i] = new networkEdge();
                net.edge[i].from = ((Vertex)edg.Source).BodyText;
                net.edge[i].to = ((Vertex)edg.Target).BodyText;
                if(fce.ContainsKey((Edge<object>)edg))
                    fc = fce[edg];
                net.edge[i].capacity = fc.Capacity;
                net.edge[i].flow = fc.Flow;
                net.edge[i].capacitySpecified = true;
                net.edge[i].flowSpecified = true;
                
                net.edge[i].id = Convert.ToString(i);
            }
           
            net.node = new networkNode[graph.VertexCount];
            for (int i = 0; i < g.VertexCount; i++) {
                vx = (Vertex)g.Vertices.ElementAt(i);
                net.node[i] = new networkNode();
                net.node[i].positionx = Convert.ToInt32(vx.X);
                net.node[i].positiony = Convert.ToInt32(vx.Y);
                net.node[i].positionxSpecified = true;
                net.node[i].positionySpecified = true;
                net.node[i].id = vx.BodyText;
                if (vx.BodyText.CompareTo(source) == 0) {
                    net.node[i].isSource = true;
                    net.node[i].isSourceSpecified = true;
                } else {
                    net.node[i].isSource = false;
                }

                if (vx.BodyText.CompareTo(sink) == 0) {
                    net.node[i].isSink = true;
                    net.node[i].isSinkSpecified = true;
                } else {
                    net.node[i].isSink = false;
                }


            }
            
            net.id = DateTime.Now.ToString();
            net.width = width;
            net.height = height;
            net.heightSpecified = true;
            net.widthSpecified = true;


            XmlSerializer serializer = null;
            FileStream stream = null;

            try {
                StringBuilder sb = new StringBuilder();
                StringWriter output = new StringWriter(sb);

                output.NewLine = String.Empty;
                serializer = new XmlSerializer(net.GetType());

                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                serializer.Serialize(output, net, namespaces);
                return output.ToString();
            } catch {
                return ""; // return indication for error
            } finally {
                if (stream != null) {
                    stream.Close();
                }
            }
           
        }

        public static network ToNetwork(string msg) {
            network net = null;
            try {
                TextReader reader = new StringReader(msg);
                XmlSerializer serializer = new XmlSerializer(typeof(network));
                net = (network)serializer.Deserialize(reader);

                reader.Close();
        
            } catch (System.Xml.XmlException xe) {
                MessageBox.Show(xe.Message, "XML Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (InvalidOperationException ioe) {
                MessageBox.Show(ioe.InnerException.Message, "XML Serialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return net;
        }
    }
}

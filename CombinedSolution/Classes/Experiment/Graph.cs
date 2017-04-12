using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;

namespace CombinedSolution.Classes.Experiment
{
    public class Node
    {
        private int _id;
        private int _x;
        private int _y;
        private int _cost;

        public Node(int id, int x, int y)
        {
            _id = id;
            _x = x;
            _y = y;
            _cost = Int32.MaxValue;
        }
        public int X
        {
            get
            {
                return _x;
            }
        }
        public int Y
        {
            get
            {
                return _y;
            }
        }
        public int ID
        {
            get
            {
                return _id;
            }
        }
        public int Cost
        {
            get
            {
                return _cost;
            }
            set
            {
                _cost = value;
            }
        }
    }
    public class Edge
    {
        private int _id;

        private Node _A;
        private Node _B;

        private int _aX;
        private int _aY;
        private int _bX;
        private int _bY;

        private int _cost;

        public Edge(Node a, Node b)
        {
            _A = a;
            _B = b;
            _aX = _A.X;
            _aY = _A.Y;
            _bX = _B.X;
            _bY = _B.Y;
            _cost = (int)Math.Abs(Math.Sqrt(Math.Pow((_bX - _aX), 2) + Math.Pow((_bY - _aY), 2)));
        }
        public int AX
        {
            get
            {
                return _aX;
            }
        }
        public int AY
        {
            get
            {
                return _aY;
            }
        }
        public int BX
        {
            get
            {
                return _bX;
            }
        }
        public int BY
        {
            get
            {
                return _bY;
            }
        }
        public int Cost
        {
            get
            {
                return _cost;
            }
        }
        public Node A
        {
            get
            {
                return _A;
            }
        }
        public Node B
        {
            get
            {
                return _B;
            }
        }
    }
    public class Graph
    {
        private List<Node> _nodeList;
        private List<Edge> _edgeList;
        private string _inFile;
        private string _outFile;
        private int _start;
        private int _end;
        private List<Edge> _route;
        private Font _font;
        private Brush _brush;
        private Brush _textBrush;
        private Pen _pen;
        public Graph(int inp, int start, int end)
        {
            _nodeList = new List<Node>();
            _edgeList = new List<Edge>();
            _inFile = inp.ToString() + ".txt";
            _outFile = "Images/" + inp.ToString() + "-" + start.ToString() + "&" + end.ToString() + ".bmp";
            _font = new Font("Times New Roman", 12.0f);
            _start = start;
            _end = end;
        }


        public void LoadGraph()
        {
            ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
            bool divider = false;
            int count = 0;
            cacheLock.EnterReadLock();
            try
            {
                string[] fOut = File.ReadAllLines(_inFile);
                foreach (string s in fOut)
                {
                    if (s == "#")
                    {
                        divider = true;
                    }
                    else if (!divider)
                    {
                        loadNode(s);
                        ++count;
                    }
                    else if (divider)
                    {
                        loadEdge(s);
                    }
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        private void loadNode(string input)
        {
            string[] split = input.Split('|');
            if (split.Length == 3)
            {
                int id, x, y;
                Int32.TryParse(split[0], out id);
                Int32.TryParse(split[1], out x);
                Int32.TryParse(split[2], out y);
                Node temp = new Node(id, x, y);
                _nodeList.Add(temp);
            }
        }
        private void loadEdge(string input)
        {
            string[] split = input.Split('|');
            if (split.Length == 2)
            {
                int idA, idB;
                Node a = null;
                Node b = null;
                Int32.TryParse(split[0], out idA);
                Int32.TryParse(split[1], out idB);
                foreach (Node n in _nodeList)
                {
                    if (n.ID == idA)
                    {
                        a = n;
                    }
                    else if (n.ID == idB)
                    {
                        b = n;
                    }
                    if (a != null && b != null)
                    {
                        Edge e = new Edge(a, b);
                        _edgeList.Add(e);
                        break;
                    }
                }
            }
        }
        private Node getNode(int id)
        {
            Node retVal = null;
            foreach (Node n in _nodeList)
            {
                if (n.ID == id)
                {
                    retVal = n;
                    break;
                }
            }
            return retVal;
        }
        public void Djikstra()
        {
            Node st = getNode(_start);
            Node en = getNode(_end);
            if (st != null && en != null)
            {
                List<Node> nList = new List<Node>(_nodeList);
                List<Edge> eList = new List<Edge>(_edgeList);
                _route = new List<Edge>();
                Node current = null;
                Node next = null;
                Edge tempEdge = null;
                int cost = 0;
                int tentative = 0;
                current = st;
                current.Cost = 0;
                while (nList.Count > 0)
                {
                    if (current == en)
                    {
                        break;
                    }
                    foreach (Edge e in eList)
                    {
                        if (e.A == current && nList.Contains(e.B))
                        {
                            cost += e.Cost;
                            if (cost < e.B.Cost)
                            {
                                e.B.Cost = cost;
                            }
                            if (next == null)
                            {
                                tentative = cost;
                                tempEdge = e;
                                next = e.B;
                            }
                            else if (cost < tentative)
                            {
                                tentative = cost;
                                tempEdge = e;
                                next = e.B;
                            }
                            cost -= e.Cost;
                        }
                        else if (e.B == current && nList.Contains(e.A))
                        {
                            cost += e.Cost;
                            if (cost < e.A.Cost)
                            {
                                e.A.Cost = cost;
                            }
                            if (next == null)
                            {
                                tentative = cost;
                                tempEdge = e;
                                next = e.A;
                            }
                            else if (cost < tentative)
                            {
                                tentative = cost;
                                tempEdge = e;
                                next = e.A;
                            }
                            cost -= e.Cost;
                        }
                    }
                    if (tempEdge != null)
                    {
                        eList.Remove(tempEdge);
                        _route.Add(tempEdge);
                        tempEdge = null;
                        nList.Remove(current);
                        current = next;
                        next = null;
                        cost = 0;
                        tentative = 0;
                    }
                    else
                    {
                        _route.Clear();
                        break;
                    }
                }
            }
        }
        public void GraphToImage()
        {
            _brush = new SolidBrush(Color.Red);
            _textBrush = new SolidBrush(Color.Black);
            _pen = new Pen(Color.Red);
            int xBorder = 1000;
            int yBorder = 1000;
            Bitmap bmp = new Bitmap(xBorder, yBorder);
            Graphics bmpGraphics = Graphics.FromImage(bmp);
            bmpGraphics.FillRectangle(Brushes.White, 0, 0, xBorder, yBorder);
            Color col = Color.Black;
            foreach (Edge e in _edgeList)
            {
                if(_route.Contains(e))
                {
                    col = Color.DarkGoldenrod;
                }
                drawEdge(e, col, bmpGraphics);
                col = Color.Black;
            }
            foreach (Node n in _nodeList)
            {
                drawNode(n, bmpGraphics);
            }
            bmp.Save(_outFile);
            _brush.Dispose();
            _textBrush.Dispose();
            _pen.Dispose();
            bmpGraphics.Dispose();
        }
        private void drawNode(Node a, Graphics g)
        {
            int rad = 10;
            int hw = rad + rad;
            int x = a.X;
            int y = a.Y;
            g.DrawEllipse(_pen, x - rad, y - rad, hw, hw);
            g.FillEllipse(_brush, x - rad, y - rad, hw, hw);
            string ds = a.ID.ToString();
            SizeF dsH = g.MeasureString(ds, _font);
            Point centreB = new Point(x - (int)(dsH.Width / 2), y - (int)(dsH.Height / 2));
            g.DrawString(ds, _font, _textBrush, centreB);
        }
        private void drawEdge(Edge e, Color c, Graphics g)
        {
            _pen.Color = c;
            _pen.Width = 2.0f;
            g.DrawLine(_pen, e.AX, e.AY, e.BX, e.BY);
            _pen.Width = 1.0f;
            _pen.Color = Color.Red;
        }
    }
}

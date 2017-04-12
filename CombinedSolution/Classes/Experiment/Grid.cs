using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;

namespace CombinedSolution.Classes.Experiment
{
    public class Square
    {
        private int _x;
        private int _y;
        private int _hw;
        private List<Square> _neighbours;

        public Square(int x, int y, int hw)
        {
            _x = x + (hw / 2);
            _y = y + (hw / 2);
            _hw = hw;
            _neighbours = new List<Square>();
        }
        public void addNeighbour(Square b)
        {
            _neighbours.Add(b);
        }
        public List<Square> GetNeighbours()
        {
            return _neighbours;
        }
        public void Draw(Graphics graphic, Pen p)
        {
            int x = _x - (_hw / 2);
            int y = _y - (_hw / 2);
            graphic.DrawRectangle(p, x, y, _hw, _hw);
        }
        public void Fill(Graphics graphic, Brush b)
        {
            int x = _x - (_hw / 2);
            int y = _y - (_hw / 2);
            graphic.FillRectangle(b, x, y, _hw, _hw);
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

    }
    public class PriorityQueue<T>
    {
        private List<Tuple<T, int>> elements = new List<Tuple<T, int>>();

        public int Count()
        {
            return elements.Count;
        }
        public void Enqueue(T item, int priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }
        public T Dequeue()
        {
            int index = 0;

            for (int i = 0; i < elements.Count; ++i)
            {
                if (elements[i].Item2 < elements[index].Item2)
                {
                    index = i;
                }
            }

            T retVal = elements[index].Item1;
            elements.RemoveAt(index);
            return retVal;
        }
    }
    public class Grid
    {
        private List<Square> _grid;
        private List<Square> _blocked;
        private Square _start;
        private Square _end;
        private string _fileIn;
        private string _fileOut;
        private int _gridX;
        private int _gridY;
        private int _hw;

        private List<Square> _aStarRoute;
        private List<Square> _djikstraRoute;

        public Grid(string fIn, string fOut,int x, int y, int hw)
        {
            _grid = new List<Square>();
            _blocked = new List<Square>();
            _start = null;
            _end = null;
            _fileIn = fIn;
            _fileOut = fOut;
            _gridX = x;
            _gridY = y;
            _hw = hw;
        }
        public void CreateSub()
        {
            createGrid();
            Scheduler.Instance.AddNewTask(new Task(LGSub));
        }
        public void LGSub()
        {
            
            LoadGrid();
            Scheduler.Instance.AddNewTask(new Task(DjikSub));
        }
        public void DjikSub()
        {
            Dijkstra();
            Scheduler.Instance.AddNewTask(new Task(AStarSub));
        }
        public void AStarSub()
        {
            AStar();
            Scheduler.Instance.AddNewTask(new Task(DrawSub));
        }
        public void DrawSub()
        {
            PrintToFile();
            Scheduler.Instance.Close();
        }


        public void createGrid()
        {
            int x = 10;
            int y = 10;
            int tX = x;
            for (int i = 0; i < _gridY; ++i)
            {
                for (int i2 = 0; i2 < _gridX; ++i2)
                {
                    _grid.Add(new Square(tX, y, _hw));
                    tX += _hw;
                }
                y += _hw;
                tX = x;
            }
            int len = _grid.Count;
            double euclid = 0;
            for (int i = 0; i < len; ++i)
            {
                x = _grid[i].X;
                y = _grid[i].Y;
                foreach (Square s in _grid)
                {
                    if (s != _grid[i])
                    {
                        euclid = euclideanDistance(x, y, s.X, s.Y);
                        if (euclid < (_hw*2))
                        {
                            _grid[i].addNeighbour(s);
                        }
                    }
                }
            }
        }
        private double euclideanDistance(int x1, int y1, int x2, int y2)
        {
            double retVal = 0;
            retVal = Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2);
            retVal = Math.Sqrt(retVal);
            return retVal;
        }        
        private Square getSquare(string data)
        {
            string[] split = data.Split(',');
            int x, y;
            Int32.TryParse(split[0], out x);
            Int32.TryParse(split[1], out y);
            Square retVal = null;
            foreach (Square s in _grid)
            {
                if (s.X == x && s.Y == y)
                {
                    retVal = s;
                    break;
                }
            }
            return retVal;
        }
        public void LoadGrid()
        {
            ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
            int count = 0;
            string[] data;
            Square sq = null;
            cacheLock.EnterReadLock();
            try
            {
                data = File.ReadAllLines(_fileIn);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
            foreach (string s in data)
            {
                if (s == "#")
                {
                    ++count;
                }
                else
                {
                    sq = getSquare(s);
                    if (sq != null)
                    {
                        switch (count)
                        {
                            case (0):
                                _start = sq;
                                break;
                            case (1):
                                _end = sq;
                                break;
                            default:
                                _blocked.Add(sq);
                                break;
                        }
                    }
                    else
                    {
                        throw new Exception("No section exists with coordinates provided.");
                    }
                }
            }
        }
        private int Cost(Square id)
        {
            return _blocked.Contains(id) ? 1000 : 1;
        }
        private List<Square> Route(Dictionary<Square, Square> cameFrom)
        {
            List<Square> route = new List<Square>();
            Square current = _end;
            route.Add(current);
            while (current != _start)
            {
                current = cameFrom[current];
                route.Add(current);
            }
            return route;
        }
        private int Heuristic(Square a, Square b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
        public void AStar()
        {
            PriorityQueue<Square> frontier = new PriorityQueue<Square>();
            Dictionary<Square, Square> comeFrom = new Dictionary<Square, Square>();
            Dictionary<Square, int> costSoFar = new Dictionary<Square, int>();
            List<Square> neighbours = null;
            Square current = null;
            int newCost = 0;
            int priority = 0;

            frontier.Enqueue(_start, 0);
            comeFrom[_start] = _start;
            costSoFar[_start] = 0;

            while (frontier.Count() != 0)
            {
                current = frontier.Dequeue();
                if (current == _end)
                {
                    break;
                }
                neighbours = current.GetNeighbours();
                foreach (Square sq in neighbours)
                {
                    newCost = costSoFar[current] + Cost(sq);
                    if (!costSoFar.ContainsKey(sq) || newCost < costSoFar[sq])
                    {
                        costSoFar[sq] = newCost;
                        priority = newCost + Heuristic(_end, sq);
                        frontier.Enqueue(sq, priority);
                        comeFrom[sq] = current;
                    }
                }
            }

            _aStarRoute = Route(comeFrom);
        }
        public void Dijkstra()
        {
            PriorityQueue<Square> frontier = new PriorityQueue<Square>();
            Dictionary<Square, Square> comeFrom = new Dictionary<Square, Square>();
            Dictionary<Square, int> costSoFar = new Dictionary<Square, int>();
            List<Square> neighbours = null;
            Square current = null;
            int newCost = 0;

            frontier.Enqueue(_start, 0);
            comeFrom[_start] = _start;
            costSoFar[_start] = 0;

            while (frontier.Count() != 0)
            {
                current = frontier.Dequeue();
                if (current == _end)
                {
                    break;
                }
                neighbours = current.GetNeighbours();
                foreach (Square b in neighbours)
                {
                    newCost = costSoFar[current] + Cost(b);
                    if (!costSoFar.ContainsKey(b) || newCost < costSoFar[b])
                    {
                        costSoFar[b] = newCost;
                        frontier.Enqueue(b, newCost);
                        comeFrom[b] = current;
                    }
                }
            }

            _djikstraRoute = Route(comeFrom);
        }
        public void PrintToFile()
        {
            if (_aStarRoute != null && _djikstraRoute != null)
            {
                Bitmap bmp = new Bitmap(1250, 1020);
                Graphics g = Graphics.FromImage(bmp);
                Font f = new Font("Times New Roman", 12.0f);
                g.FillRectangle(Brushes.White, 0, 0, 1200, 1020);
                Pen p = new Pen(Color.Black);
                foreach (Square s in _grid)
                {
                    s.Draw(g, p);
                }
                foreach (Square s in _blocked)
                {
                    s.Fill(g, Brushes.Black);
                }
                foreach (Square s in _djikstraRoute)
                {
                    if (_aStarRoute.Contains(s))
                    {
                        s.Fill(g, Brushes.Purple);
                        _aStarRoute.Remove(s);
                    }
                    else
                    {
                        s.Fill(g, Brushes.Blue);
                    }
                }
                _djikstraRoute.Clear();
                foreach (Square s in _aStarRoute)
                {
                    s.Fill(g, Brushes.Orange);
                }
                _aStarRoute.Clear();
                _start.Fill(g, Brushes.Green);
                _end.Fill(g, Brushes.Red);

                g.DrawRectangle(p, 1030, 700, 150, 310);
                
                g.FillRectangle(Brushes.Green, 1040, 735, 40, 40);
                g.FillRectangle(Brushes.Red, 1040, 790, 40, 40);
                g.FillRectangle(Brushes.Blue, 1040, 845, 40, 40);
                g.FillRectangle(Brushes.Orange, 1040, 900, 40, 40);
                g.FillRectangle(Brushes.Purple, 1040, 955, 40, 40);

                g.DrawString("Legend", f, Brushes.Black, 1077.5f, 700);
                g.DrawString("Start", f, Brushes.Black, 1102.5f, 745);
                g.DrawString("End", f, Brushes.Black, 1102.5f, 800);
                g.DrawString("Djikstra", f, Brushes.Black, 1102.5f, 855);
                g.DrawString("A*", f, Brushes.Black, 1102.5f, 910);
                g.DrawString("Both", f, Brushes.Black, 1102.5f, 965);

                bmp.Save(_fileOut);
                bmp.Dispose();
                p.Dispose();
                g.Dispose();
            }
        }
    }
}

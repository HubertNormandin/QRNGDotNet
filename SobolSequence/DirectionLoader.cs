using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;


namespace QRNGDotNet.SobolSequence
{
    [Serializable]
    public class Direction: IEqualityComparer<Direction>
    {
        public int d;
        public uint s;
        public uint a;
        public uint[] v;
        public Direction(int d, uint s, uint a, uint[] v)
        {
            this.d = d;
            this.s = s;
            this.a = a;
            this.v = v;
        }
        public int D()
        {
            return this.d;
        }
        public uint S()
        {
            return this.s;
        }
        public uint A()
        {
            return this.a;
        }
        public uint[] V()
        {
            return this.v;
        }
        public bool Equals(Direction x, Direction y)
        {
            if (y == null || x == null)
                return false;
            
            if (x.v.Length != y.v.Length)
                return false;
            
            if (x.v.Where((t, i) => t != y.v[(uint)i]).Any())
            {
                return false;
            }

            return x.d == y.d && x.s == y.s && x.a == y.a;
        } 

        public int GetHashCode(Direction obj)
        {
            return this.d;
        }
    }
    public sealed partial class DirectionLoader
    {
        // Load directions number for a specific criteria
        public class DirectionNumberLoader
        {
            public const byte L = 32;
            private int last_inserted = 0;
            private StreamReader direction_reader;
            private Dictionary<int, Direction> directions;

            public DirectionNumberLoader(string criteria)
            {
                this.directions = new Dictionary<int, Direction>();
                OpenDirectionFile(DirectionLoader.config.GetPath(criteria));
            }

            public Direction Next()
            {
                this.last_inserted++;
                if (this.directions.ContainsKey(this.last_inserted)) {
                    
                    return this.directions[this.last_inserted - 1];
                }
                else if (this.last_inserted == 1)
                {
                    uint[] v = new uint[L + 1];
                    v[0] = (uint)Math.Pow(2, 32);
                    for (int i = 1; i <= L; i++)
                    {
                        // for the first dimension every m_i = 1;
                        v[i] = (uint)(1 << (32 - i));
                    }
                    Direction direction = new Direction(this.last_inserted, 0, 0, v);
                    this.directions.Add(this.last_inserted, direction);
                    return direction;
                }
                else if(!this.direction_reader.EndOfStream)
                {                    
                    string line = direction_reader.ReadLine();
                    if (line != null)
                    {
                        // Parse value in direction file
                        string[] values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        uint s = UInt32.Parse(values[1]);
                        uint a = UInt32.Parse(values[2]);
                        uint[] m_i = new uint[values.Length - 3];

                        for (int i = 0; i < values.Length - 3; i++)
                        {
                            m_i[i] = UInt32.Parse(values[i + 3]);
                        }

                        uint[] v = new uint[L + 1];
                        if (L <= s)
                        {
                            for (int i = 1; i <= L; i++)
                            {
                                v[i] = m_i[i] << (32 - i);
                            }
                        }
                        else
                        {
                            for (int i = 1; i <= s; i++)
                            {
                                v[i] = m_i[i - 1] << (32 - i);
                            }
                            for (uint i = s + 1; i <= L; i++)
                            {
                                v[i] = v[i - s] ^ (v[i - s] >> (int)s);
                                for (uint k = 1; k <= s - 1; k++)
                                {
                                    v[i] ^= (((a >> (int)(s - 1 - k)) & 1) * v[i - k]);
                                }
                            }
                        }
                        Direction direction = new Direction(this.last_inserted, s, a, v);
                        this.directions.Add(this.last_inserted, direction);
                        return direction;
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }

            private void OpenDirectionFile(Stream path)
            {
                try
                {
                    try
                    {

                        // Open and read the file containing the direction number
                        direction_reader = new StreamReader(path, true);

                        //skip the header
                        direction_reader.ReadLine();
                    }
                    catch (FileNotFoundException e)
                    {
                        // TODO: throw right exception + Logging
                        throw e;
                    }
                }
                catch (ArgumentNullException e)
                {
                    // add exception
                    //TODO: add LOGGING
                    throw e;
                }
            }
            public void Reset()
            {
                this.last_inserted = 0;
            }

            ~DirectionNumberLoader()
            {
                // close the file if it is open
                direction_reader?.Close();
            }
        }
        // Multiton
        private static Dictionary<string, DirectionNumberLoader> _instances = new Dictionary<string, DirectionNumberLoader>();
        private static Config config = Config.Instance;
        private static object _lock = new object();
        
        private DirectionLoader()
        {
           
        }

        public static DirectionNumberLoader GetDirectionNumberLoader(string criteria)
        {
            lock (_lock)
            {
                if (!_instances.ContainsKey(criteria)) _instances.Add(criteria, new DirectionNumberLoader(criteria));
            }
            return _instances[criteria];
        }
    }
}
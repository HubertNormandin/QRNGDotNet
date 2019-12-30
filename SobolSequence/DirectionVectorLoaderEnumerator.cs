
using System;
using System.Collections;
using System.Collections.Generic;


namespace CsQRNG.SobolSequence
{
    public sealed partial class DirectionVectorLoader : IEnumerator<Direction>
    {

        public Direction Current
        {
            get
            {
                this.directions.TryGetValue(DirectionVectorLoader.last_inserted, out Direction value);
                return value;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                this.directions.TryGetValue(DirectionVectorLoader.last_inserted, out Direction value);
                return value;
            }
        }

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            uint[] v = new uint[L + 1];
            DirectionVectorLoader.last_inserted++;
            if (this.directions.ContainsKey(DirectionVectorLoader.last_inserted))
            {
                return true;
            }
            else if(DirectionVectorLoader.last_inserted == 0)
            {
                v[0] = (uint)Math.Pow(2, 32)-1;
                for (int i = 1; i <= L; i++)
                {
                    // for the first dimension every m_i = 1;
                    v[i] = (uint)(1 << (32 - i));
                }
                Direction dir = new Direction(DirectionVectorLoader.last_inserted,0, 0,v);
                this.directions.GetOrAdd(DirectionVectorLoader.last_inserted, dir);
            }
            else if(!this.direction_reader.EndOfStream)
            {
                string line = this.direction_reader.ReadLine();
                if (line != null)
                {
                    string[] values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    uint s = UInt32.Parse(values[1]);
                    uint a = UInt32.Parse(values[2]);
                    uint[] m_i = new uint[values.Length-3];
                    for (int i = 0; i < values.Length-3; i++)
                    {
                        m_i[i] = UInt32.Parse(values[i+3]);
                    }

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
                    this.directions.GetOrAdd(DirectionVectorLoader.last_inserted, new Direction(DirectionVectorLoader.last_inserted, s, a, v));
                }
                else
                    return false;
            }
            else
                return false;
            
            return true;
        }

        public void Reset()
        {
            DirectionVectorLoader.last_inserted = 1;
        }

    }
}

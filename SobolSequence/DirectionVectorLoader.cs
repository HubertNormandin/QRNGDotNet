using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
// ReSharper disable PossibleIntendedRethrow

namespace CsQRNG.SobolSequence
{
    [Serializable]
    public class Direction: IEqualityComparer<Direction>
    {
        public int D;
        public uint S;
        public uint A;
        public uint[] V;
        public Direction(int d, uint s, uint a, uint[] v)
        {
            this.D = d;
            this.S = s;
            this.A = a;
            this.V = v;
            
        }

        public bool Equals(Direction x, Direction y)
        {
            Console.WriteLine();
            if (y == null || x == null)
                return false;
            
            if (x.V.Length != y.V.Length)
                return false;
            
            if (x.V.Where((t, i) => t != y.V[(uint)i]).Any())
            {
                return false;
            }

            return x.D == y.D && x.S == y.S && x.A == y.A;
        } 

        public int GetHashCode(Direction obj)
        {
            return this.D;
        }
    }

    public sealed partial class DirectionVectorLoader
    {

        public const byte L = 32;
        private ConcurrentDictionary<int, Direction> directions;
        // the criteria that has been use to generate the direction vector
        private readonly char criteria;
        private readonly IFormatter formatter;
        [NonSerialized] private static int last_inserted=-1;
        [NonSerialized] private StreamReader direction_reader;

        public DirectionVectorLoader(char criteria)
        {
            this.criteria = criteria;
            this.formatter = new BinaryFormatter();
            this.OpenDirectionFile();
            this.Deserialize();
        }

        private void OpenDirectionFile()
        {
            //string direction_dir = Configuration.Default.DirectionVector;
            string direction_dir = "..\\..\\..\\CsQRNG\\DirectionVector";
            try
            {
                DirectoryInfo dir = new DirectoryInfo(direction_dir);

                //fetching the serialized direction number
                //List<string> direction_files = dir.GetFiles().Where(file => file.Name.Contains(criteria.ToString())).Select(file => file.Name).ToList();
                //string filename = direction_files.FirstOrDefault();
                string filename = "joe-kuo-6.21201";
                try
                {
                    this.direction_reader = new StreamReader(File.OpenRead(direction_dir + "\\" + filename), true);
                    //skip the header
                    this.direction_reader.ReadLine();
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

        private void Deserialize()
        {
            // Open The file where the direction number will be deserialize
            string number_dir = "..\\..\\..\\CsQRNG\\DirectionNumber";
            FileStream number_reader = null;
            string filename = "dir-number-" + this.criteria + ".dat";
            try
            {
                number_reader = new FileStream(number_dir + "\\" + filename, FileMode.Open, FileAccess.ReadWrite);
                // Deserialize the file 
                this.directions = (ConcurrentDictionary<int, Direction>) this.formatter.Deserialize(number_reader);
            }
            catch (FileNotFoundException)
            {
                // Instantiate an empty ConcurrentDictionary if file doesn't exist
                this.directions = new ConcurrentDictionary<int, Direction>();
                number_reader = File.Create(number_dir + "\\" + filename);
            }
            finally
            {
                number_reader?.Close();
            }
            //TODO Verify that the exception cover everything
        }

        public void Serialize()
        {
            FileStream number_reader = null;
            //string number_dir = Configuration.Default.DirectionNumber;
            string number_dir = "..\\..\\..\\CsQRNG\\DirectionNumber";

            string filename = "dir-number-"+this.criteria+".dat";
            // close the file if it is open
            number_reader?.Close();
            // Write On The File
            number_reader = File.Create(number_dir + "\\" + filename );
            this.formatter.Serialize(number_reader, this.directions);
            number_reader.Close();
        }

        ~DirectionVectorLoader()
        {
            // close the file if it is open
            direction_reader?.Close();
        }

    }
}
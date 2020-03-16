using System;
using System.IO;
using System.Reflection;

namespace QRNGDotNet
{
    public sealed class Config
    {
        //Instance of the singleton
        private static readonly Config _instance = new Config();

        static Config()
        {

        }

        private Config()
        {

        }

        public static Config Instance
        {
            get
            {
                return Config._instance;
            }
        }

        public Stream GetPath(string criteria)
        {
            switch (criteria){
                case "5":
                    return new MemoryStream(Properties.Resources.joe_kuo_5);

                case "6":
                    return new MemoryStream(Properties.Resources.joe_kuo_6);

                case "7":
                    return new MemoryStream(Properties.Resources.joe_kuo_7);

                default:
                    throw new ArgumentOutOfRangeException("criteria");

            }
        }
    }
}

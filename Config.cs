using System;
using System.IO;
using System.Reflection;

namespace QRNGDotNet
{
    public sealed class Config
    {
        //Instance of the singleton
        private static readonly Config _instance = new Config();
        private const string DirectionNumber5 = "\\DirectionNumber\\joe-kuo-5.21201";
        private const string DirectionNumber6 = "\\DirectionNumber\\joe-kuo-6.21201";
        private const string DirectionNumber7 = "\\DirectionNumber\\joe-kuo-7.21201";
        

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

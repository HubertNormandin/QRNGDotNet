using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRNGDotNet
{
    public sealed class Config
    {
        private static readonly Config _instance = new Config();
        private const string DirectionNumber5 = "..\\..\\..\\..\\QRNGDotNet\\DirectionNumber\\joe-kuo-5.21201";
        private const string DirectionNumber6 = "..\\..\\..\\..\\QRNGDotNet\\DirectionNumber\\joe-kuo-6.21201";
        private const string DirectionNumber7 = "..\\..\\..\\..\\QRNGDotNet\\DirectionNumber\\joe-kuo-7.21201";


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

        public string GetPath(string criteria)
        {
            switch(criteria){
                case "5":
                    return DirectionNumber5;
               
                case "6":
                    return DirectionNumber6;
              
                case "7":
                    return DirectionNumber7;
    
                default:
                    throw new ArgumentOutOfRangeException("criteria");

            }
        }
    }
}

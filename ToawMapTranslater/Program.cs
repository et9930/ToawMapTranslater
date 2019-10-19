using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToawMapTranslater
{
    class Program
    {

        static void Main(string[] args)
        {
            var gameData = System.IO.File.ReadAllText(@"D:\81507\文档\Directive 21 1941-1945.gam");
            dynamic gameDataXml = new DynamicXml(gameData);
            
            Console.ReadLine();
        }

    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace NETCoreDI
{
    public interface ISayHello
    {
        string Hi(string message);
    }
    public class SayHello : ISayHello
    {
        int HashCode;

        public SayHello()
        {
            // (jasper) 透過 HashCode 來區分注入物件是否為同一個物件
            HashCode = this.GetHashCode();
            Console.WriteLine($"SayHello ({HashCode}) 已經被建立了");
        }

        public string Hi(string message)
        {
            Console.WriteLine($"({HashCode}) {message}");
            return $"Hi ({HashCode}) {message}";
        }

        // (jasper) 了解 DI Container 與 GC 之間的關係，是否會自動釋放
        ~SayHello()
        {
            Console.WriteLine($"SayHello ({HashCode}) 已經被釋放了");
        }
    }


    public class Program
    {
        // static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            Console.WriteLine("====== TestTransient ======");
            TestTransient();
            Console.WriteLine("====== TestScoped ======");
            TestScoped();
            Console.WriteLine("====== TestSingleton ======");
            TestSingleton();

            //Console.WriteLine("Press any key for continuing...");
            //Console.ReadKey();

        }

        private static void TestTransient()
        {
            IServiceCollection serviceCollection;
            IServiceProvider serviceProvider1;
            ISayHello sayHello1;
            ISayHello sayHello2;
            ISayHello sayHello3;

            serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<ISayHello, SayHello>();
            serviceProvider1 = serviceCollection.BuildServiceProvider();

            #region 暫時性 Transient

            sayHello1 = serviceProvider1.GetService<ISayHello>();
            sayHello1.Hi("M1 - Will");
            sayHello2 = serviceProvider1.GetService<ISayHello>();
            sayHello2.Hi("M2 - Lee");
            sayHello3 = serviceProvider1.GetService<ISayHello>();
            sayHello1 = null;
            sayHello2 = null;
            GC.Collect(2);
            Thread.Sleep(1000);
            sayHello3.Hi("M3 - Will");

            #endregion
        }

        private static void TestScoped()
        {
            IServiceCollection serviceCollection;
            IServiceProvider serviceProvider1;
            IServiceProvider serviceProvider2;
            IServiceProvider serviceProvider3;
            IServiceScope serviceScope2;
            IServiceScope serviceScope3;
            ISayHello sayHello1;
            ISayHello sayHello2;
            ISayHello sayHello3;
            ISayHello sayHello1_1;
            ISayHello sayHello2_1;
            ISayHello sayHello3_1;

            serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ISayHello, SayHello>();
            serviceProvider1 = serviceCollection.BuildServiceProvider();

            #region 使用預設 Scope
            sayHello1 = serviceProvider1.GetService<ISayHello>();
            sayHello1.Hi("M1 - Will");
            sayHello2 = serviceProvider1.GetService<ISayHello>();
            sayHello2.Hi("M2 - Lee");
            sayHello1 = null;
            sayHello2 = null;
            GC.Collect(2);
            Thread.Sleep(1000);
            sayHello3 = serviceProvider1.GetService<ISayHello>();
            sayHello3.Hi("M9 - Will");
            #endregion

            #region 使用兩個 Scope

            //serviceScope2 = serviceProvider1.CreateScope();
            //serviceProvider2 = serviceScope2.ServiceProvider;
            //sayHello1 = serviceProvider2.GetService<ISayHello>();
            //sayHello1.Hi("M1 - Will");
            //sayHello2 = serviceProvider2.GetService<ISayHello>();
            //sayHello2.Hi("M2 - Lee");
            //sayHello1 = null;
            //sayHello2 = null;
            //GC.Collect(2);
            //Thread.Sleep(1000);
            //sayHello3 = serviceProvider2.GetService<ISayHello>();
            //sayHello3.Hi("M9 - Will");

            //serviceScope3 = serviceProvider1.CreateScope();
            //serviceProvider3 = serviceScope3.ServiceProvider;
            //sayHello1_1 = serviceProvider3.GetService<ISayHello>();
            //sayHello1_1.Hi("M1_1 - Ada");
            //sayHello2_1 = serviceProvider3.GetService<ISayHello>();
            //sayHello2_1.Hi("M2_1 - Chan");
            //sayHello1_1 = null;
            //sayHello2_1 = null;
            //GC.Collect(2);
            //Thread.Sleep(1000);
            //sayHello3_1 = serviceProvider3.GetService<ISayHello>();
            //sayHello3_1.Hi("M3_1 - Ada Chan");
            //// 若將底下的程式碼註解起來(在 AddScoped 模式)，則 
            //// sayHello1, sayHello2 指向到 ConsoleMessage 會被釋放掉
            ////sayHello3.Hi("M3 - Will");

            #endregion

        }


        private static void TestSingleton()
        {
            IServiceCollection serviceCollection;
            IServiceProvider serviceProvider1;
            IServiceProvider serviceProvider2;
            IServiceProvider serviceProvider3;
            IServiceScope serviceScope2;
            IServiceScope serviceScope3;
            ISayHello sayHello1;
            ISayHello sayHello2;
            ISayHello sayHello3;
            ISayHello sayHello1_1;
            ISayHello sayHello2_1;
            ISayHello sayHello3_1;

            serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ISayHello, SayHello>();
            serviceProvider1 = serviceCollection.BuildServiceProvider();

            #region 使用單一 Singleton 

            serviceScope2 = serviceProvider1.CreateScope();
            serviceProvider2 = serviceScope2.ServiceProvider;
            sayHello1 = serviceProvider2.GetService<ISayHello>();
            sayHello1.Hi("M1 - Will");
            sayHello2 = serviceProvider2.GetService<ISayHello>();
            sayHello2.Hi("M2 - Lee");
            sayHello1 = null;
            sayHello2 = null;
            GC.Collect(2);
            Thread.Sleep(1000);
            sayHello3 = serviceProvider2.GetService<ISayHello>();
            sayHello3.Hi("M9 - Will");

            serviceScope3 = serviceProvider1.CreateScope();
            serviceProvider3 = serviceScope3.ServiceProvider;
            sayHello1_1 = serviceProvider3.GetService<ISayHello>();
            sayHello1_1.Hi("M1_1 - Ada");
            sayHello2_1 = serviceProvider3.GetService<ISayHello>();
            sayHello2_1.Hi("M2_1 - Chan");
            sayHello1_1 = null;
            sayHello2_1 = null;
            GC.Collect(2);
            Thread.Sleep(1000);
            sayHello3_1 = serviceProvider3.GetService<ISayHello>();
            sayHello3_1.Hi("M3_1 - Ada Chan");
            // 若將底下的程式碼註解起來(在 AddScoped 模式)，則 
            // sayHello1, sayHello2 指向到 ConsoleMessage 會被釋放掉
            sayHello3.Hi("M3 - Will");

            #endregion
        }
    }
}

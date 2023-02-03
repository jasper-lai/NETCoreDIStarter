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
            //sayHello1 = serviceProvider1.GetService<ISayHello>();
            //sayHello1.Hi("M1 - Will");
            //sayHello2 = serviceProvider1.GetService<ISayHello>();
            //sayHello2.Hi("M2 - Lee");
            //sayHello1 = null;
            //sayHello2 = null;
            //GC.Collect(2);
            //Thread.Sleep(1000);
            //sayHello3 = serviceProvider1.GetService<ISayHello>();
            //sayHello3.Hi("M3 - Will");
            #endregion

            // -----------------------------------------------------
            // (jasper)
            // 1.. 因為是 AddScoped, 建立不同的 scope 的狀況下, 會有不同的物件實體
            // 2.. sayHello1, sayHello2 被設成 null, 因為還在 scope 中, 所以不會被釋放
            // 3.. sayHello1_1, sayHello2_1 被設成 null, 因為還在 scope 中, 所以不會被釋放;
            //     但此時因為前一個 scope 已經結束, 所以 sayHello1 (與 sayHello2 相同 instance), 
            //     應該要被解構掉.
            //     --> 但實測結果是完全沒有被釋放, 不論 sayHello3.Hi("M3 - Will"); 是否有被註解掉
            // -----------------------------------------------------

            #region 使用兩個 Scope

            // ----------------- scope2 ------------------------
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
            sayHello3.Hi("M3 - Will");

            // ----------------- scope3 ------------------------
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

            // (jasper) 箇要: 這裡用的是 scope2 的物件 ....
            //
            // 若將底下的程式碼註解起來(在 AddScoped 模式)，則 
            // sayHello1, sayHello2 指向到 ConsoleMessage 會被釋放掉
            sayHello3.Hi("M3 - Will");

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

            // -----------------------------------------------------
            // (jasper)
            // 1.. 因為是 AddSingleton, 所以, 即使是在不同的 scope, 仍然應該取得相同的物件
            // 2.. sayHello1, sayHello2, sayHello1_1, sayHello2_1 雖然被設為 null,
            //     但因為是 singleton, 仍然有 DI container 在使用中, 不會被釋放
            // -----------------------------------------------------

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
            sayHello3.Hi("M3 - Will");

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

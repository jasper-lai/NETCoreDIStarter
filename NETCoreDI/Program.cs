using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
            // (jasper): 經實測, 看來要在前一個函式結束後, 要再作一次 GC.Collect(), 才會真的回收
            // 讓資源釋放的訊息有空檔可以輸出至螢幕的方式
            // (1) 同步呼叫 MyDelay();   <== 是用 Task.Delay(2000) 去實作, 不會卡現行的 UI
            // (2) 非同步呼叫 DelayAsync() <== 是用 Task.Delay(2000) 去實作, 不會卡現行的 UI
            // (3) Thread.Sleep(2000);  <== 依 Google 查到的資料, 會卡現行的 UI (即 main thread)
            GC.Collect();
            //// (1)
            MyDelay();
            //// (2)
            //Task delay1 = DelayAsync();
            //delay1.Wait();
            //// (3)
            //Thread.Sleep(2 * 1000);

            Console.WriteLine("====== TestScoped ======");
            TestScoped();
            GC.Collect();
            MyDelay();
            //Task delay2 = DelayAsync();
            //delay2.Wait();

            Console.WriteLine("====== TestSingleton ======");
            TestSingleton();
            GC.Collect();
            MyDelay();
            //Task delay3 = DelayAsync();
            //delay3.Wait();

            //Console.WriteLine("====== TestDummy ======");
            //TestDummy();

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
            GC.Collect();
            Thread.Sleep(1000);
            sayHello3.Hi("M3 - Will");
            //Console.WriteLine($"DI Container Generation: {GC.GetGeneration(serviceProvider1)}");
            //Console.WriteLine($"object Generation: {GC.GetGeneration(sayHello3)}");

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
            //     --> 前一個 scope 並未結束, 故可以呼叫 sayHello3.Hi("M3 - Jasper"); 
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
            GC.Collect();
            Thread.Sleep(1000);
            sayHello3 = serviceProvider2.GetService<ISayHello>();
            sayHello3.Hi("M3 - Will");
            //Console.WriteLine($"DI Container II Generation: {GC.GetGeneration(serviceProvider2)}");
            //Console.WriteLine($"object Generation (sayHello3): {GC.GetGeneration(sayHello3)}");

            // ----------------- scope3 ------------------------
            serviceScope3 = serviceProvider1.CreateScope();
            serviceProvider3 = serviceScope3.ServiceProvider;
            sayHello1_1 = serviceProvider3.GetService<ISayHello>();
            sayHello1_1.Hi("M1_1 - Ada");
            sayHello2_1 = serviceProvider3.GetService<ISayHello>();
            sayHello2_1.Hi("M2_1 - Chan");
            sayHello1_1 = null;
            sayHello2_1 = null;
            GC.Collect();
            Thread.Sleep(1000);
            sayHello3_1 = serviceProvider3.GetService<ISayHello>();
            sayHello3_1.Hi("M3_1 - Ada Chan");
            //Console.WriteLine($"DI Container III Generation: {GC.GetGeneration(serviceProvider3)}");
            //Console.WriteLine($"object Generation (sayHello3_1): {GC.GetGeneration(sayHello3_1)}");

            // (jasper) 箇要: 這裡用的是 scope2 的物件 ....
            //
            // 若將底下的程式碼註解起來(在 AddScoped 模式)，則 
            // sayHello1, sayHello2 指向到 ConsoleMessage 會被釋放掉
            sayHello3.Hi("M3 - Jasper");

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
            GC.Collect();
            Thread.Sleep(1000);
            sayHello3 = serviceProvider2.GetService<ISayHello>();
            sayHello3.Hi("M3 - Will");
            //Console.WriteLine($"DI Container II Generation: {GC.GetGeneration(serviceProvider2)}");
            //Console.WriteLine($"object Generation (sayHello3): {GC.GetGeneration(sayHello3)}");

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
            //Console.WriteLine($"DI Container III Generation: {GC.GetGeneration(serviceProvider3)}");
            //Console.WriteLine($"object Generation (sayHello3_1): {GC.GetGeneration(sayHello3_1)}");

            // 若將底下的程式碼註解起來(在 AddScoped 模式)，則 
            // sayHello1, sayHello2 指向到 ConsoleMessage 會被釋放掉
            sayHello3.Hi("M3 - Will");

            #endregion
        }

        private static void TestDummy()
        {
            // -----------------------------------------------------
            // (jasper)
            // 1.. 看來會在 GC.Collect(2) 的空檔, 才會回收前述 Singleton 的物件
            // -----------------------------------------------------
            Console.WriteLine("Hello, World !");
            Console.WriteLine("Hello, World !");
            Console.WriteLine("Hello, World !");
            GC.Collect(2);
            Thread.Sleep(1000);
            Console.WriteLine("Hello, Jasper !");
            Console.WriteLine("Hello, Jasper !");
            Console.WriteLine("Hello, Jasper !");
        }


        private static async Task DelayAsync()
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"thread: {Thread.CurrentThread.ManagedThreadId} async: Starting *");
            Task delay = Task.Delay(2 * 1000);
            //Console.WriteLine($"thread: {Thread.CurrentThread.ManagedThreadId} async: Running for {sw.Elapsed.TotalSeconds} seconds **");
            await delay;
            Console.WriteLine($"thread: {Thread.CurrentThread.ManagedThreadId} async: Running for {sw.Elapsed.TotalSeconds} seconds ***");
            Console.WriteLine($"thread: {Thread.CurrentThread.ManagedThreadId} async: Done ****");
        }

        private static void MyDelay()
        {
            var delay = Task.Run(async () =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                await Task.Delay(2 * 1000);
                sw.Stop();
                return sw.Elapsed.TotalSeconds;
            });

            Console.WriteLine($"Elapsed {delay.Result} seconds " );
        }
    }
}

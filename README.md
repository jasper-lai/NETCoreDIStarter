## 目的:
用以驗證 .NET Core 3.1 (非 ASP.NET Core 3.1) 在 Transient, Scoped, Singleton 下, .NET Core DI 生成的物件實體是否相同, 及其釋放時機.

## 問題:
物件解構子執行的時間似乎不對.

## 細節:
1.. SolutionTransient, SolutionScoped, SolutionSingleton 都沒有看到物件釋放的訊息, 所以不確定是否有執行 ~SayHello() 這個解構子.

2.. 後來, 把 1.. 的程式合併到一支, 改用 TestTransient(), TestScoped(), TestSingleton() 方法作測試, 結果發現:  
(1) Transient 建立的物件, 會在 TestScoped() 執行的時候, 出現訊息.  
(2) Scoped 建立的物件, 會在 TestSingleton() 執行的時候, 出現訊息.  

3.. 要如何才能正確看到解構子執行的訊息?


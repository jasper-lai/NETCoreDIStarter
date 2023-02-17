## 目的:
用以驗證 .NET Core 3.1 (非 ASP.NET Core 3.1) 在 Transient, Scoped, Singleton 下, .NET Core DI 生成的物件實體是否相同, 及其釋放時機.

## 問題: (已經暫時 Work Around)
物件解構子執行的時間似乎不對.
Work Around 的方式, 請查看 細節.3 的描述.

## 預期行為:  (不知描述是否正確?)

### TestScoped(): 2 個 scope
1.. 因為是 AddScoped, 建立不同的 scope 的狀況下, 會有不同的物件實體  
2.. sayHello1, sayHello2 被設成 null, 因為還在 scope2 中, 所以不會被釋放.  
3.. sayHello1_1, sayHello2_1 被設成 null, 因為還在 scope3 中, 所以不會被釋放.  
    --> 前一個 scope 並未結束, 故可以呼叫 sayHello3.Hi("M3 - Jasper"); 
    
### TestSingleton:
1.. 因為是 AddSingleton, 所以, 即使是在不同的 scope, 仍然應該取得相同的物件.  
2.. sayHello1, sayHello2, sayHello1_1, sayHello2_1 雖然被設為 null, 但因為是 singleton, 仍然有 DI container 在使用中, 不會被釋放.  


## 細節:
1.. SolutionTransient, SolutionScoped, SolutionSingleton 都沒有看到物件釋放的訊息, 所以不確定是否有執行 ~SayHello() 這個解構子.

![Transient](https://github.com/jasper-lai/netcoredistarter/blob/master/pictures/transient.png?raw=true)

![Scoped](https://github.com/jasper-lai/netcoredistarter/blob/master/pictures/scoped.png?raw=true)

![Singleton](https://github.com/jasper-lai/netcoredistarter/blob/master/pictures/singleton.png?raw=true)

2.. 後來, 把 1.. 的程式合併到一支, 改用 TestTransient(), TestScoped(), TestSingleton() 方法作測試, 結果發現:  
(1) Transient 建立的物件, 會在 TestScoped() 執行的時候, 出現訊息.  
(2) Scoped 建立的物件, 會在 TestSingleton() 執行的時候, 出現訊息.  

![All_In_One](https://github.com/jasper-lai/netcoredistarter/blob/master/pictures/all_in_one.png?raw=true)

**3.. 要如何才能正確看到解構子執行的訊息? (Work Around)** 

經實測, 看來要在前一個函式結束後, 要再作一次 GC.Collect(), 才會真的回收.  
讓資源釋放的訊息有空檔可以輸出至螢幕的方式: (可以一併參考最後一次 commit 的程式段)    
* (1) 同步呼叫 MyDelay();   <== 是用 Task.Delay(2000) 去實作, 不會卡現行的 UI  
* (2) 非同步呼叫 DelayAsync() <== 是用 Task.Delay(2000) 去實作, 不會卡現行的 UI  
* (3) Thread.Sleep(2000);  <== 依 Google 查到的資料, 會卡現行的 UI (即 main thread)  
![All_In_One_Work_Around](https://github.com/jasper-lai/netcoredistarter/blob/master/pictures/all_in_one_work_around.png?raw=true)

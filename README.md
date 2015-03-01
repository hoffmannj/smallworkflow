# smallworkflow
Small fluent workflow framework

This is just a small framework that helps keeping responsibilities separated.

You can define a simple workflow like this:
```C#
	WFTask.A(MainTask)
	.OnSuccess(WFTask.A(MainTaskSuccess).OnSuccess(MainTaskSuccess2))
	.OnFailure(MainTaskFailure)
	.OnException<NullReferenceException>(MainTaskNullReferenceException)
	.OnException<Exception>(MainTaskException)
	.Run();
```
// Take a look at the Console application for the exact code.

You can even define a loop:
```C#
	var loop = WFTask.F(() => 10);
	var loopCore = WFTask.F<int, int>(LoopCore).OnException<Exception>(() => { });
	loopCore.OnSuccess(loopCore);
	loop.OnSuccess(loopCore).Run();
```
// Note: too many iterations can fill up the stack (it's like a recursive loop)

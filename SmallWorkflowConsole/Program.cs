using SmallWorkflow;
using System;

namespace SmallWorkflowConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Example1();
            Example2();

            Console.WriteLine("done");
            Console.ReadLine();
        }

        public static void Example1()
        {
            Console.WriteLine("Example 1:");
            WFTask.A(MainTask)
            .OnSuccess(WFTask.A(MainTaskSuccess).OnSuccess(MainTaskSuccess2))
            .OnFailure(MainTaskFailure)
            .OnException<NullReferenceException>(MainTaskNullReferenceException)
            .OnException<Exception>(MainTaskException)
            .Run();
            Console.WriteLine();
        }

        public static void Example2()
        {
            Console.WriteLine("Example 2:");
            var loop = WFTask.F(() => 10);
            var loopCore = WFTask.F<int, int>(LoopCore).OnException<Exception>(() => { });
            loopCore.OnSuccess(loopCore);
            loop.OnSuccess(loopCore).Run();
            Console.WriteLine();
        }

        private static void MainTask()
        {
            Console.WriteLine("Main task started.");
        }

        private static void MainTaskSuccess()
        {
            Console.WriteLine("Main task was successful. This is task 2.");
        }

        private static void MainTaskSuccess2()
        {
            Console.WriteLine("Task 2 was successful.");
        }

        private static void MainTaskFailure()
        {
            Console.WriteLine("There was a failure in the Main task.");
        }

        private static void MainTaskNullReferenceException(NullReferenceException ex)
        {
            Console.WriteLine("NullReferenceException occured in Main task: " + ex.Message);
        }

        private static void MainTaskException(Exception ex)
        {
            Console.WriteLine("Exception occured in Main task: " + ex.Message);
        }

        private static int LoopCore(int number)
        {
            if (number < 0) throw new Exception();
            Console.WriteLine("number: " + number);
            return number - 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallWorkflow
{
    public static class WFTask
    {
        public static WFTask<TPrev, TResult> F<TPrev, TResult>(Func<TPrev, TResult> func)
        {
            return new WFTask<TPrev, TResult>(func);
        }

        public static WFTask<object, TResult> F<TResult>(Func<TResult> func)
        {
            return new WFTask<object, TResult>(func);
        }

        public static WFTask<TPrev, object> A<TPrev>(Action<TPrev> action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            return new WFTask<TPrev, object>(prev =>
            {
                action(prev);
                return null;
            });
        }

        public static WFTask<object, object> A(Action action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            return new WFTask<object, object>(() =>
            {
                action();
                return null;
            });
        }
    }

    public sealed class WFTask<TPrev, TResult> : IRunnableTask<TPrev>
    {
        private Action<TPrev> TaskAction { get; set; }
        private TResult _result = default(TResult);

        private IRunnableTask<TResult> SuccessTask { get; set; }
        private IRunnableTask<TResult> FailureTask { get; set; }
        private IRunnableTask<TResult> FinallyTask { get; set; }
        private Dictionary<Type, dynamic> OnExceptionTask { get; set; }

        private WFTask()
        {
            OnExceptionTask = new Dictionary<Type, dynamic>();
        }

        internal WFTask(Func<TPrev, TResult> func)
            : this()
        {
            AssertHelper.ThrowIfNull(func, "func");
            TaskAction = prev =>
            {
                try
                {
                    _result = func(prev);
                    CallSuccess();
                }
                catch (Exception ex)
                {
                    CallFailure();
                    if (!CallException(ex)) throw;
                }
                finally
                {
                    CallFinally();
                }
            };
        }

        internal WFTask(Func<TResult> func)
            : this()
        {
            AssertHelper.ThrowIfNull(func, "func");
            TaskAction = prev =>
            {
                try
                {
                    _result = func();
                    CallSuccess();
                }
                catch (Exception ex)
                {
                    CallFailure();
                    if (!CallException(ex)) throw;
                }
                finally
                {
                    CallFinally();
                }
            };
        }

        public void Run(TPrev prev)
        {
            TaskAction(prev);
        }

        public void Run()
        {
            TaskAction(default(TPrev));
        }


        public WFTask<TPrev, TResult> OnSuccess<TNext>(Func<TResult, TNext> func)
        {
            AssertHelper.ThrowIfNull(func, "func");
            SuccessTask = WFTask.F(func);
            return this;
        }

        public WFTask<TPrev, TResult> OnSuccess(Action<TResult> action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            SuccessTask = WFTask.A(action);
            return this;
        }

        public WFTask<TPrev, TResult> OnSuccess(Action action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            SuccessTask = WFTask.A((TResult prev) => action());
            return this;
        }

        public WFTask<TPrev, TResult> OnSuccess<TNext>(WFTask<TResult, TNext> nextTask)
        {
            AssertHelper.ThrowIfNull(nextTask, "nextTask");
            SuccessTask = nextTask;
            return this;
        }

        public WFTask<TPrev, TResult> OnFailure<TNext>(Func<TResult, TNext> func)
        {
            AssertHelper.ThrowIfNull(func, "func");
            FailureTask = new WFTask<TResult, TNext>(func);
            return this;
        }

        public WFTask<TPrev, TResult> OnFailure(Action<TResult> action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            FailureTask = WFTask.A(action);
            return this;
        }

        public WFTask<TPrev, TResult> OnFailure(Action action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            FailureTask = WFTask.A((TResult prev) => action());
            return this;
        }

        public WFTask<TPrev, TResult> OnFailure<TNext>(WFTask<TResult, TNext> nextTask)
        {
            AssertHelper.ThrowIfNull(nextTask, "nextTask");
            FailureTask = nextTask;
            return this;
        }

        public WFTask<TPrev, TResult> OnFinally<TNext>(Func<TResult, TNext> func)
        {
            AssertHelper.ThrowIfNull(func, "func");
            FinallyTask = new WFTask<TResult, TNext>(func);
            return this;
        }

        public WFTask<TPrev, TResult> OnFinally(Action<TResult> action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            FinallyTask = WFTask.A(action);
            return this;
        }

        public WFTask<TPrev, TResult> OnFinally(Action action)
        {
            AssertHelper.ThrowIfNull(action, "action");
            FinallyTask = WFTask.A((TResult prev) => action());
            return this;
        }

        public WFTask<TPrev, TResult> OnFinally<TNext>(WFTask<TResult, TNext> nextTask)
        {
            AssertHelper.ThrowIfNull(nextTask, "nextTask");
            FinallyTask = nextTask;
            return this;
        }

        public WFTask<TPrev, TResult> OnException<TEx, TNext>(Func<TEx, TNext> func) where TEx : Exception
        {
            AssertHelper.ThrowIfNull(func, "func");
            OnExceptionTask[typeof(TEx)] = new WFTask<TEx, TNext>(func);
            return this;
        }

        public WFTask<TPrev, TResult> OnException<TEx>(Action<TEx> action) where TEx : Exception
        {
            AssertHelper.ThrowIfNull(action, "action");
            OnExceptionTask[typeof(TEx)] = WFTask.A(action);
            return this;
        }

        public WFTask<TPrev, TResult> OnException<TEx>(Action action) where TEx : Exception
        {
            AssertHelper.ThrowIfNull(action, "action");
            OnExceptionTask[typeof(TEx)] = WFTask.A((TEx ex) => action());
            return this;
        }

        public WFTask<TPrev, TResult> OnException<TEx, TNext>(WFTask<TEx, TNext> nextTask) where TEx : Exception
        {
            AssertHelper.ThrowIfNull(nextTask, "nextTask");
            OnExceptionTask[typeof(TEx)] = nextTask;
            return this;
        }


        private void CallSuccess()
        {
            if (SuccessTask == null) return;
            SuccessTask.Run(_result);
        }

        private void CallFailure()
        {
            if (FailureTask == null) return;
            FailureTask.Run(_result);
        }

        private void CallFinally()
        {
            if (FinallyTask == null) return;
            FinallyTask.Run(_result);
        }

        private bool CallException(dynamic ex)
        {
            var first = GetAssignableException(ex);
            if (first == null) return false;
            OnExceptionTask[first].Run(ex);
            return true;
        }

        private Type GetAssignableException(dynamic ex)
        {
            var exType = ex.GetType();
            return OnExceptionTask.Keys.FirstOrDefault(e => e.IsAssignableFrom(exType));
        }
    }
}

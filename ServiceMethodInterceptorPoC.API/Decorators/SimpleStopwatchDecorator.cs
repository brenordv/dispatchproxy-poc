using System.Diagnostics;
using System.Reflection;

namespace ServiceMethodInterceptorPoC.API.Decorators;

    public class SimpleStopwatchDecorator<TDecorated> : DispatchProxy
    {
        private const string ExecDoneMessage = "[{0}] Done executing '{1}'! ElapsedTime: {2}";
        private TDecorated _decorated;
        private ILogger<SimpleStopwatchDecorator<TDecorated>> _logger;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var timer = Stopwatch.StartNew();
            var type = typeof(TDecorated).Name;
            var methodName = $"{type}.{targetMethod.Name}";
            try
            {
                _logger.LogInformation("Invoking '{MethodName}' now!", methodName);

                var result = targetMethod.Invoke(_decorated, args);

                if (result is Task resultTask)
                {
                    resultTask.ContinueWith(task =>
                    {
                        timer.Stop();
                        if (task.IsFaulted)
                        {
                            _logger.LogInformation(ExecDoneMessage, "ERROR", methodName, timer.Elapsed);
                            _logger.LogError(task.Exception,
                                $"An unhandled exception was raised during execution of {methodName}", methodName);
                        }
                        else
                        {
                            _logger.LogInformation(ExecDoneMessage, "SUCCESS", methodName, timer.Elapsed);
                        }
                    });
                }
                else
                {
                    timer.Stop();
                    _logger.LogInformation(ExecDoneMessage, "SUCCESS", methodName, timer.Elapsed);
                }

                return result;
            }
            catch (TargetInvocationException ex)
            {
                if (timer.IsRunning) timer.Stop();
                _logger.LogInformation(ExecDoneMessage, "ERROR", methodName, timer.Elapsed);
                _logger.LogError(exception: ex.InnerException ?? ex,
                                 message:string.Format(ExecDoneMessage, "ERROR", methodName, timer.Elapsed));
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// Used dynamically during runtime.
        /// </summary>
        /// <remarks>Seems like dead code, but it's not.</remarks>
        /// <param name="decorated"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static TDecorated Create(TDecorated decorated, ILogger<SimpleStopwatchDecorator<TDecorated>> logger)
        {
            object proxy = Create<TDecorated, SimpleStopwatchDecorator<TDecorated>>();
            ((SimpleStopwatchDecorator<TDecorated>) proxy).SetParameters(decorated, logger);

            return (TDecorated) proxy;
        }

        private void SetParameters(TDecorated decorated, ILogger<SimpleStopwatchDecorator<TDecorated>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        }
    }
﻿using System;
using Polly;
using System.Threading.Tasks;

namespace RedisRetry
{
    public class RetryTask
    {
        private readonly Func<Task> _func;
        private static readonly Func<int, TimeSpan> RetryAttemptWaitProvider =
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

        private Policy _retryPolicy;

        public RetryTask(Func<Task> func, int retryCount = 3, Func<int, TimeSpan> waitProvider = null)
        {
            _func = func;
            _retryPolicy = Policy
               .Handle<Exception>()
               .WaitAndRetryAsync(retryCount, waitProvider ?? RetryAttemptWaitProvider);
        }

        public async Task RunAsync()
        {
            var res = await _retryPolicy.ExecuteAndCaptureAsync(() => _func())
                .ConfigureAwait(false);
            if (res.Outcome == OutcomeType.Failure)
            {
                throw res.FinalException;
            }
        }
    }
}

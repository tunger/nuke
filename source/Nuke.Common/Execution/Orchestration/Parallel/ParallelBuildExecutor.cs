﻿using JetBrains.Annotations;
using Nuke.Common.Execution.Orchestration.Sequential;
using Nuke.Common.Logging;
using Nuke.Common.Utilities.Collections;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nuke.Common.Execution.Orchestration.Parallel
{
    public static class ParallelBuildExecutor
    {
        public static async Task Execute(NukeBuild build)
        {
            var previouslyExecutedTargets = TargetExecutor.UpdateInvocationHash();
            BuildManager.CancellationHandler += () => SequentialBuildExecutor.ExecuteAssuredTargets(build, previouslyExecutedTargets);

            var workSets = build.ExecutionPlan.ExecutionItems;

            var cts = new CancellationTokenSource();
            
            try
            {
                // create the countdowns for all items which have dependents
                workSets.ForEach(x => x.InitCountdown());

                var invokedTargets = workSets.Where(x => x.IsInvoked).ToList();
                await ExecuteItems(build, invokedTargets, cts);
            }
            catch
            {
                SequentialBuildExecutor.ExecuteAssuredTargets(build, previouslyExecutedTargets);
                throw;
            }
        }

        private static async Task ExecuteItems(NukeBuild build, IEnumerable<ExecutionItem> items, CancellationTokenSource cts)
        {
            var tasks = items
                .Select(async x =>
                {
                    try
                    {
                        await Task.Run(() => ExecuteParallel(build, x, cts), cts.Token);
                    }
                    catch
                    {
                        cts.Cancel();
                        throw;
                    }
                })
                .ToList();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Recursively executes execution items and waits on dependencies.
        /// </summary>
        private static async Task ExecuteParallel(NukeBuild build, ExecutionItem executionItem, CancellationTokenSource cts)
        {
            // Only the first one to arrive will do the work, all others simply wait.
            var executeWorkload = executionItem.WillExecuteWork();

            // but first execute all dependencies
            if (executionItem.Dependencies.Any())
            {
                await ExecuteItems(build, executionItem.Dependencies, cts);
            }

            if (executeWorkload)
            {
                executionItem.Logger = LoggerProvider.GetCurrentLogger();
                TargetExecutor.ExecuteItem(build, executionItem, new string[] { }, cts.Token);

                // TargetExecutor raises the signal on the countdown
            }
            else
            {
                // Raise the signal, even if doing nothing
                executionItem.DidNothing();
            }

            // Now wait for all its iterations to arrive and/or finish work
            executionItem.Join(cts.Token);
        }
    }
}

﻿// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nuke.Common.Execution
{
    internal class TargetDefinition : ITargetDefinition
    {
        public TargetDefinition(NukeBuild build)
        {
            Build = build;
        }

        public NukeBuild Build { get; }

        internal string Description { get; set; }
        internal ExecutionStatus Status { get; set; }
        internal List<Expression<Func<bool>>> DynamicConditions { get; } = new List<Expression<Func<bool>>>();
        internal List<Expression<Func<bool>>> StaticConditions { get; } = new List<Expression<Func<bool>>>();
        internal List<LambdaExpression> Requirements { get; } = new List<LambdaExpression>();
        internal List<Target> DependsOnTargets { get; } = new List<Target>();
        internal List<Target> DependentForTargets { get; } = new List<Target>();
        internal List<Action> Actions { get; } = new List<Action>();
        internal DependencyBehavior DependencyBehavior { get; private set; }
        internal bool IsProceedAfterFailure { get; private set; }
        internal bool IsAssuredAfterFailure { get; private set; }
        internal bool IsInternal { get; private set; }
        internal List<Target> BeforeTargets { get; } = new List<Target>();
        internal List<Target> AfterTargets { get; } = new List<Target>();
        internal List<Target> TriggersTargets { get; } = new List<Target>();
        internal List<Target> TriggeredByTargets { get; } = new List<Target>();

        ITargetDefinition ITargetDefinition.Description(string description)
        {
            Description = description;
            return this;
        }

        public ITargetDefinition Executes(params Action[] actions)
        {
            Actions.AddRange(actions);
            return this;
        }

        public ITargetDefinition Executes<T>(Func<T> action)
        {
            return Executes(new Action(() => action()));
        }

        public ITargetDefinition Executes(Func<Task> action)
        {
            return Executes(() => action().GetAwaiter().GetResult());
        }

        public ITargetDefinition DependsOn(params Target[] targets)
        {
            DependsOnTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition DependsOn<T>(params Func<T, Target>[] targets)
        {
            return DependsOn(targets.Select(x => x((T) (object) Build)).ToArray());
        }

        public ITargetDefinition DependentFor(params Target[] targets)
        {
            DependentForTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition DependentFor<T>(params Func<T, Target>[] targets)
        {
            return DependentFor(targets.Select(x => x((T) (object) Build)).ToArray());
        }

        public ITargetDefinition OnlyWhenDynamic(params Expression<Func<bool>>[] conditions)
        {
            DynamicConditions.AddRange(conditions);
            return this;
        }

        public ITargetDefinition OnlyWhenStatic(params Expression<Func<bool>>[] conditions)
        {
            StaticConditions.AddRange(conditions);
            return this;
        }

        public ITargetDefinition Requires<T>(params Expression<Func<T>>[] parameterRequirement)
            where T : class
        {
            Requirements.AddRange(parameterRequirement);
            return this;
        }

        public ITargetDefinition Requires<T>(params Expression<Func<T?>>[] parameterRequirement)
            where T : struct
        {
            Requirements.AddRange(parameterRequirement);
            return this;
        }

        public ITargetDefinition Requires<TBuild, T>(params Expression<Func<TBuild, T>>[] parameterRequirement)
            where T : class
        {
            Requirements.AddRange(parameterRequirement);
            return this;
        }

        public ITargetDefinition Requires<TBuild, T>(params Expression<Func<TBuild, T?>>[] parameterRequirement)
            where T : struct
        {
            Requirements.AddRange(parameterRequirement);
            return this;
        }

        public ITargetDefinition Requires(params Expression<Func<bool>>[] requirement)
        {
            Requirements.AddRange(requirement);
            return this;
        }

        public ITargetDefinition WhenSkipped(DependencyBehavior dependencyBehavior)
        {
            DependencyBehavior = dependencyBehavior;
            return this;
        }

        public ITargetDefinition Before(params Target[] targets)
        {
            BeforeTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition Before<T>(params Func<T, Target>[] targets)
        {
            return Before(targets.Select(x => x((T) (object) Build)).ToArray());
        }

        public ITargetDefinition After(params Target[] targets)
        {
            AfterTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition After<T>(params Func<T, Target>[] targets)
        {
            return After(targets.Select(x => x((T) (object) Build)).ToArray());
        }

        public ITargetDefinition Triggers(params Target[] targets)
        {
            TriggersTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition Triggers<T>(params Func<T, Target>[] targets)
        {
            return Triggers(targets.Select(x => x((T) (object) Build)).ToArray());
        }

        public ITargetDefinition TriggeredBy(params Target[] targets)
        {
            TriggeredByTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition TriggeredBy<T>(params Func<T, Target>[] targets)
        {
            return TriggeredBy(targets.Select(x => x((T) (object) Build)).ToArray());
        }

        public ITargetDefinition AssuredAfterFailure()
        {
            IsAssuredAfterFailure = true;
            return this;
        }

        public ITargetDefinition ProceedAfterFailure()
        {
            IsProceedAfterFailure = true;
            return this;
        }

        public ITargetDefinition Unlisted()
        {
            IsInternal = true;
            return this;
        }
    }
}

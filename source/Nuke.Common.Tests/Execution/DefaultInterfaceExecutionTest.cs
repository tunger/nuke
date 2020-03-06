using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Nuke.Common.Execution;
using Xunit;

namespace Nuke.Common.Tests.Execution
{
    public class DefaultInterfaceExecutionTest
    {
        public static string Description = "description";
        public static Action Action = () => { };
        public static Expression<Func<bool>> Requirement = () => true;
        public static Expression<Func<bool>> StaticCondition = () => true;
        public static Expression<Func<bool>> DynamicCondition = () => false;

        [Fact]
        public void Test()
        {
            var build = new TestBuild();
            var ibuild = (ITestBuild)build;
            var targets = ExecutableTargetFactory.CreateAll(build, x => NukeBuild.FromInterface<ITestBuild>(i => i.A, x));

            var a = targets.Single(x => x.Name == nameof(ITestBuild.A));
            var b = targets.Single(x => x.Name == nameof(ITestBuild.B));
            var c = targets.Single(x => x.Name == nameof(ITestBuild.C));
            var d = targets.Single(x => x.Name == nameof(ITestBuild.D));

            targets.Single(x => x.IsDefault).Should().Be(a);

            a.Factory.Should().Be(NukeBuild.FromInterface<ITestBuild>(i => i.A, build));
            a.Description.Should().Be(DefaultInterfaceExecutionTest.Description);
            a.Requirements.Should().Equal(DefaultInterfaceExecutionTest.Requirement);
            a.Actions.Should().Equal(DefaultInterfaceExecutionTest.Action);
            a.AllDependencies.Should().BeEmpty();

            b.DependencyBehavior.Should().Be(DependencyBehavior.Execute);
            b.StaticConditions.Should().Equal(DefaultInterfaceExecutionTest.StaticCondition);
            b.ExecutionDependencies.Should().Equal(d);
            b.TriggerDependencies.Should().Equal(c);
            b.AllDependencies.Should().NotBeEmpty();

            c.Triggers.Should().Equal(b);
            c.TriggerDependencies.Should().Equal(d);
            c.ExecutionDependencies.Should().Equal(b);
            c.OrderDependencies.Should().Equal(d);
            c.AllDependencies.Should().NotBeEmpty();

            d.DependencyBehavior.Should().Be(DependencyBehavior.Skip);
            d.DynamicConditions.Should().Equal(DefaultInterfaceExecutionTest.DynamicCondition);
            d.OrderDependencies.Should().Equal(b);
            d.Triggers.Should().Equal(c);
            d.AllDependencies.Should().NotBeEmpty();
        }

        private class TestBuild : NukeBuild, ITestBuild { }

        private interface ITestBuild
        {
            public string Description => DefaultInterfaceExecutionTest.Description;
            public Action Action => DefaultInterfaceExecutionTest.Action;
            public Expression<Func<bool>> Requirement => DefaultInterfaceExecutionTest.Requirement;
            public Expression<Func<bool>> StaticCondition => DefaultInterfaceExecutionTest.StaticCondition;
            public Expression<Func<bool>> DynamicCondition => DefaultInterfaceExecutionTest.DynamicCondition;

            public Target A => _ => _
                .Description(Description)
                .Requires(Requirement)
                .Executes(Action);

            public Target B => _ => _
                .WhenSkipped(DependencyBehavior.Execute)
                .OnlyWhenStatic(StaticCondition)
                .DependsOn(D)
                .DependentFor(C);

            public Target C => _ => _
                .Triggers(B)
                .TriggeredBy(D);

            public Target D => _ => _
                .WhenSkipped(DependencyBehavior.Skip)
                .OnlyWhenDynamic(DynamicCondition)
                .After(B)
                .Before(C);
        }
    }
}

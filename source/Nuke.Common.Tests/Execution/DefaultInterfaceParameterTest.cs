using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;
using Xunit;

namespace Nuke.Common.Tests.Execution
{
    public class DefaultInterfaceParameterTest
    {
        [Fact]
        public void TestWithMissingArgument()
        {
            var build = new TestBuild();
            var targets = ExecutableTargetFactory.CreateAll(build, x => x.E);

            var e = targets.Single(x => x.Name == nameof(TestBuild.E));
            Assert.Throws<Exception>(() => RequirementService.ValidateRequirements(build, new[] { e }));
        }

        [Fact]
        public void TestWithProvidedArgument()
        {
            EnvironmentInfo.SetVariable("interface-parameter", "test");

            var build = new TestBuild();
            var targets = ExecutableTargetFactory.CreateAll(build, x => x.E);

            var e = targets.Single(x => x.Name == nameof(TestBuild.E));
            RequirementService.ValidateRequirements(build, new[] { e });
        }

        [Fact]
        public void TestWithEnvironmentInfoArgument()
        {
            EnvironmentInfo.SetVariable("buildserver.interfaceParameter", "test");

            var build = new TestBuild();
            var targets = ExecutableTargetFactory.CreateAll(build, x => x.E);

            var e = targets.Single(x => x.Name == nameof(TestBuild.E));
            RequirementService.ValidateRequirements(build, new[] { e });
        }

        private class TestBuild : NukeBuild, ITestBuild
        {
            public Target E => _ => _
                .Requires(() => ((ITestBuild)this).InterfaceParameter)
                .DependsOn<ITestBuild>(x => x.A)
                .Executes(() => { });
        }
        
        private interface ITestBuild
        {
            [Parameter] string InterfaceParameter => InjectionUtility.GetInjectionValue(() => InterfaceParameter)
                ?? EnvironmentInfo.GetParameter<string>("buildserver.interfaceParameter");

            public Target A => _ => _
                .Requires(() => InterfaceParameter)
                .Executes(() => { });
        }
    }
}

﻿using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace UnitTests
{
    public sealed class ScenarioTests
    {
        [Fact]
        public void Ctor_Initializes_Properties()
        {
            //arrange.
            var featureInstance = new FeatureForCtorTest();
            var stepMethods = new List<StepMethod>
            {
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.Then_Something)), featureInstance), "text1"),
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.When_Something)), featureInstance), "text2"),
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.Given_Something)), featureInstance), "text3")
            };

            //act.
            var sut = new Scenario(stepMethods);

            //assert.
            Assert.NotNull(sut.Steps);
            Assert.Equal(3, sut.Steps.Count);
            Assert.Equal(stepMethods, sut.Steps);
        }

        private sealed class FeatureForCtorTest : Feature
        {
            [Then("something")]
            public void Then_Something()
            {

            }

            [When("something")]
            public void When_Something()
            {

            }

            [Given("something")]
            public void Given_Something()
            {

            }
        }

        [Fact]
        public async Task Execute_Invokes_All_StepMethods()
        {
            //arrange.
            var featureInstance = new FeatureWithStepMethodsToInvoke();

            var sut = new Scenario(new List<StepMethod>
            {
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep1)), featureInstance), FeatureWithStepMethodsToInvoke.ScenarioStep1Text),
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep2)), featureInstance), FeatureWithStepMethodsToInvoke.ScenarioStep2Text),
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep3)), featureInstance), FeatureWithStepMethodsToInvoke.ScenarioStep3Text),
                new StepMethod(StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep4)), featureInstance), FeatureWithStepMethodsToInvoke.ScenarioStep4Text)
            });

            var output = new Mock<IScenarioOutput>();

            //act.
            await sut.ExecuteAsync(output.Object);

            //assert.
            Assert.NotNull(featureInstance.CallStack);
            Assert.Equal(4, featureInstance.CallStack.Count);

            Assert.Equal(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep1), featureInstance.CallStack[0]);
            output.Verify(o => o.StepPassed("Given " + FeatureWithStepMethodsToInvoke.ScenarioStep1Text), Times.Once);

            Assert.Equal(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep2), featureInstance.CallStack[1]);
            output.Verify(o => o.StepPassed("And " + FeatureWithStepMethodsToInvoke.ScenarioStep2Text), Times.Once);

            Assert.Equal(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep3), featureInstance.CallStack[2]);
            output.Verify(o => o.StepPassed("When " + FeatureWithStepMethodsToInvoke.ScenarioStep3Text), Times.Once);

            Assert.Equal(nameof(FeatureWithStepMethodsToInvoke.ScenarioStep4), featureInstance.CallStack[3]);
            output.Verify(o => o.StepPassed("Then " + FeatureWithStepMethodsToInvoke.ScenarioStep4Text), Times.Once);
        }

        private sealed class FeatureWithStepMethodsToInvoke : Feature
        {
            public List<string> CallStack { get; } = new List<string>();

            public const string ScenarioStep1Text = "I chose 12 as first number";

            [Given(ScenarioStep1Text)]
            public void ScenarioStep1()
            {
                CallStack.Add(nameof(ScenarioStep1));
            }

            [Given("Non matching given")]
            public void NonMatchingStep1()
            {
                CallStack.Add(nameof(NonMatchingStep1));
            }

            public const string ScenarioStep2Text = "I chose 15 as second number";

            [And(ScenarioStep2Text)]
            public void ScenarioStep2()
            {
                CallStack.Add(nameof(ScenarioStep2));
            }

            [And("Non matching and")]
            public void NonMatchingStep2()
            {
                CallStack.Add(nameof(NonMatchingStep2));
            }

            public const string ScenarioStep3Text = "I press add";

            [When(ScenarioStep3Text)]
            public void ScenarioStep3()
            {
                CallStack.Add(nameof(ScenarioStep3));
            }

            [When("Non matching when")]
            public void NonMatchingStep3()
            {
                CallStack.Add(nameof(NonMatchingStep3));
            }

            public const string ScenarioStep4Text = "the result should be 27 on the screen";

            [Then(ScenarioStep4Text)]
            public void ScenarioStep4()
            {
                CallStack.Add(nameof(ScenarioStep4));
            }

            [Then("Non matching then")]
            public void NonMatchingStep4()
            {
                CallStack.Add(nameof(NonMatchingStep4));
            }
        }

        [Fact]
        public async Task ExecuteAsync_Requires_Output()
        {
            //arrange.
            var sut = new Scenario(new List<StepMethod>());

            //act / assert.
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ExecuteAsync(null));
        }
    }
}

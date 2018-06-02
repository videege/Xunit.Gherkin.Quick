﻿using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Gherkin.Quick;

namespace UnitTests
{
    public sealed class ScenarioExecutorTests
    {
        private readonly Mock<IFeatureFileRepository> _featureFileRepository = new Mock<IFeatureFileRepository>();
        private readonly ScenarioExecutor _sut;

        public ScenarioExecutorTests()
        {
            _sut = new ScenarioExecutor(_featureFileRepository.Object);
        }

        [Fact]
        public void ExecuteScenario_Requires_FeatureInstance()
        {
            //act / assert.
            Assert.Throws<ArgumentNullException>(() => _sut.ExecuteScenario(null, "valid scenario name"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("                 ")]
        public void ExecuteScenario_Requires_ScenarioName(string scenarioName)
        {
            //arrange.
            var featureInstance = new UselessFeature();

            //act / assert.
            Assert.Throws<ArgumentNullException>(() => _sut.ExecuteScenario(featureInstance, scenarioName));
        }

        private sealed class UselessFeature : Feature { }

        [Fact]
        public void ExecuteScenario_Executes_Scenario_Steps()
        {
            //arrange.
            var scenarioName = "scenario 12345";
            _featureFileRepository.Setup(r => r.GetByFilePath($"{nameof(FeatureWithScenarioSteps)}.feature"))
                .Returns(new FeatureFile(CreateGherkinDocument(scenarioName,
                    "Given " + FeatureWithScenarioSteps.ScenarioStep1Text.Replace(@"(\d+)", "12", StringComparison.InvariantCultureIgnoreCase),
                    "And " + FeatureWithScenarioSteps.ScenarioStep2Text.Replace(@"(\d+)", "15", StringComparison.InvariantCultureIgnoreCase),
                    "When " + FeatureWithScenarioSteps.ScenarioStep3Text,
                    "Then " + FeatureWithScenarioSteps.ScenarioStep4Text.Replace(@"(\d+)", "27", StringComparison.InvariantCultureIgnoreCase)
                    )))
                .Verifiable();

            var featureInstance = new FeatureWithScenarioSteps();

            //act.
            _sut.ExecuteScenario(featureInstance, scenarioName);

            //assert.
            _featureFileRepository.Verify();

            Assert.Equal(4, featureInstance.CallStack.Count);

            Assert.Equal(nameof(FeatureWithScenarioSteps.ScenarioStep1), featureInstance.CallStack[0].Key);
            Assert.NotNull(featureInstance.CallStack[0].Value);
            Assert.Single(featureInstance.CallStack[0].Value);
            Assert.Equal(12, featureInstance.CallStack[0].Value[0]);

            Assert.Equal(nameof(FeatureWithScenarioSteps.ScenarioStep2), featureInstance.CallStack[1].Key);
            Assert.NotNull(featureInstance.CallStack[1].Value);
            Assert.Single(featureInstance.CallStack[1].Value);
            Assert.Equal(15, featureInstance.CallStack[1].Value[0]);

            Assert.Equal(nameof(FeatureWithScenarioSteps.ScenarioStep3), featureInstance.CallStack[2].Key);
            Assert.Null(featureInstance.CallStack[2].Value);

            Assert.Equal(nameof(FeatureWithScenarioSteps.ScenarioStep4), featureInstance.CallStack[3].Key);
            Assert.NotNull(featureInstance.CallStack[3].Value);
            Assert.Single(featureInstance.CallStack[3].Value);
            Assert.Equal(27, featureInstance.CallStack[3].Value[0]);
        }

        private static Gherkin.Ast.GherkinDocument CreateGherkinDocument(string scenario, params string[] steps)
        {
            var gherkinText =
@"Feature: Some Sample Feature
    In order to learn Math
    As a regular human
    I want to add two numbers using Calculator

Scenario: " + scenario + @"
"+ string.Join(Environment.NewLine, steps)
;
            using (var gherkinStream = new MemoryStream(Encoding.UTF8.GetBytes(gherkinText)))
            using (var gherkinReader = new StreamReader(gherkinStream))
            {
                var parser = new Gherkin.Parser();
                return parser.Parse(gherkinReader);
            }
        }

        private sealed class FeatureWithScenarioSteps : Feature
        {
            public List<KeyValuePair<string, object[]>> CallStack { get; } = new List<KeyValuePair<string, object[]>>();

            [Given("Non matching given")]
            public void NonMatchingStep1_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep1_before), null));
            }

            public const string ScenarioStep1Text = @"I chose (\d+) as first number";

            [Given(ScenarioStep1Text)]
            public void ScenarioStep1(int firstNumber)
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep1), new object[] { firstNumber }));
            }

            [Given("Non matching given")]
            public void NonMatchingStep1_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep1_after), null));
            }

            [And("Non matching and")]
            public void NonMatchingStep2_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep2_before), null));
            }

            public const string ScenarioStep2Text = @"I chose (\d+) as second number";

            [And(ScenarioStep2Text)]
            public void ScenarioStep2(int secondNumber)
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep2), new object[] { secondNumber }));
            }

            [And("Non matching and")]
            public void NonMatchingStep2_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep2_after), null));
            }

            [When("Non matching when")]
            public void NonMatchingStep3_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep3_before), null));
            }

            public const string ScenarioStep3Text = "I press add";

            [When(ScenarioStep3Text)]
            public void ScenarioStep3()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep3), null));
            }

            [When("Non matching when")]
            public void NonMatchingStep3_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep3_after), null));
            }

            [Then("Non matching then")]
            public void NonMatchingStep4_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep4_before), null));
            }

            public const string ScenarioStep4Text = @"the result should be (\d+) on the screen";

            [Then(ScenarioStep4Text)]
            public void ScenarioStep4(int result)
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep4), new object[] { result }));
            }

            [Then("Non matching then")]
            public void NonMatchingStep4_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep4_after), null));
            }
        }
    }
}

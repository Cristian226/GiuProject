using NUnit.Framework;

public class MissionStepTests
{
    [Test]
    public void Info_BuildsInfoStepWithTitleAndBody()
    {
        MissionStep step = MissionStep.Info("Title", "Body text");

        Assert.AreEqual(MissionStepKind.Info, step.kind);
        Assert.AreEqual("Title", step.title);
        Assert.AreEqual("Body text", step.prompt);
    }

    [Test]
    public void Single_BuildsSingleChoiceWithOptions()
    {
        MissionStep step = MissionStep.Single("Question?", "Explanation",
            MissionOption.Right("Correct"), MissionOption.Wrong("Wrong"));

        Assert.AreEqual(MissionStepKind.SingleChoice, step.kind);
        Assert.AreEqual(2, step.options.Count);
        Assert.IsTrue(step.options[0].correct);
        Assert.IsFalse(step.options[1].correct);
    }

    [Test]
    public void Multi_BuildsMultiSelectWithOptions()
    {
        MissionStep step = MissionStep.Multi("Pick all", "Explanation",
            MissionOption.Right("A"), MissionOption.Right("B"), MissionOption.Wrong("C"));

        Assert.AreEqual(MissionStepKind.MultiSelect, step.kind);
        Assert.AreEqual(3, step.options.Count);
    }

    [Test]
    public void Order_BuildsOrderingWithCorrectSequence()
    {
        MissionStep step = MissionStep.Order("Order them", "Explanation", "First", "Second", "Third");

        Assert.AreEqual(MissionStepKind.Ordering, step.kind);
        Assert.AreEqual(3, step.orderItems.Count);
        Assert.AreEqual("First", step.orderItems[0]);
        Assert.AreEqual("Third", step.orderItems[2]);
    }

    [Test]
    public void WithWrongFeedback_SetsFeedbackAndReturnsSameStep()
    {
        MissionStep step = MissionStep.Single("Q", "E", MissionOption.Right("A"))
            .WithWrongFeedback("Try again");

        Assert.AreEqual("Try again", step.wrongFeedback);
    }
}

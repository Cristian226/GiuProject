using System.Collections.Generic;

// The four kinds of mini-game screen:
//   Info         - a teaching panel, just press Continue.
//   SingleChoice - pick the one correct option.
//   MultiSelect  - pick ALL correct options and no wrong ones.
//   Ordering     - reorder the items into the correct sequence.
public enum MissionStepKind { Info, SingleChoice, MultiSelect, Ordering }

public class MissionOption
{
    public string label;
    public bool correct;

    public MissionOption(string label, bool correct)
    {
        this.label = label;
        this.correct = correct;
    }

    public static MissionOption Right(string label) => new MissionOption(label, true);
    public static MissionOption Wrong(string label) => new MissionOption(label, false);
}

public class MissionStep
{
    public MissionStepKind kind;
    public string title;
    public string prompt;
    public string explanation;     // shown once answered correctly
    public string wrongFeedback;   // SingleChoice: message on a wrong pick (optional)

    public List<MissionOption> options;  // SingleChoice / MultiSelect
    public List<string> orderItems;      // Ordering: the correct order

    public static MissionStep Info(string title, string body)
    {
        return new MissionStep { kind = MissionStepKind.Info, title = title, prompt = body };
    }

    public static MissionStep Single(string prompt, string explanation, params MissionOption[] opts)
    {
        return new MissionStep
        {
            kind = MissionStepKind.SingleChoice,
            title = "Quiz",
            prompt = prompt,
            explanation = explanation,
            options = new List<MissionOption>(opts)
        };
    }

    public static MissionStep Multi(string prompt, string explanation, params MissionOption[] opts)
    {
        return new MissionStep
        {
            kind = MissionStepKind.MultiSelect,
            title = "Pick all that apply",
            prompt = prompt,
            explanation = explanation,
            options = new List<MissionOption>(opts)
        };
    }

    public static MissionStep Order(string prompt, string explanation, params string[] correctOrder)
    {
        return new MissionStep
        {
            kind = MissionStepKind.Ordering,
            title = "Put them in order",
            prompt = prompt,
            explanation = explanation,
            orderItems = new List<string>(correctOrder)
        };
    }

    public MissionStep WithWrongFeedback(string feedback)
    {
        wrongFeedback = feedback;
        return this;
    }
}

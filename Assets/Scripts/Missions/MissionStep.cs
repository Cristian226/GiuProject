using System.Collections.Generic;

/// <summary>
/// One screen of a mission mini-game. Built in code by the mission-content
/// scripts (CuisineMission, HistoryMission, ...). There are four kinds:
///
///   Info         - a teaching panel, just press Continue.
///   SingleChoice - pick the one correct option (a quiz question).
///   MultiSelect  - pick ALL correct options and no wrong ones (e.g. ingredients).
///   Ordering     - reorder the items into the correct sequence (e.g. a timeline).
///
/// Use the static helpers (Info, Single, Multi, Order) to build steps cleanly.
/// </summary>
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
    public string title;        // short heading shown above the prompt
    public string prompt;       // the question / instruction
    public string explanation;  // teaching text shown once answered correctly
    public string wrongFeedback; // SingleChoice: message shown on a wrong pick (optional)

    public List<MissionOption> options;  // SingleChoice / MultiSelect
    public List<string> orderItems;      // Ordering: the CORRECT order

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

    /// <summary>Fluent: set the message shown when a SingleChoice answer is wrong.</summary>
    public MissionStep WithWrongFeedback(string feedback)
    {
        wrongFeedback = feedback;
        return this;
    }
}

/// <summary>
/// Implemented by mission-content components (one per mission scene). The
/// MissionStation runs whatever steps this returns.
/// </summary>
public interface IMissionContent
{
    string Title { get; }
    List<MissionStep> BuildSteps();
}

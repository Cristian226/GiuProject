using UnityEngine;

/// <summary>
/// Self-contained dialogue + quiz for the Police Officer NPC.
/// Attach alongside NPCInteraction on the police NPC.
///
/// FLOW:
///   Player clicks NPC  →  intro dialogue (Enter/Esc)
///   Enter accepted     →  5 quiz questions in sequence (press 1 / 2 / 3)
///   Wrong answer       →  explanation shown, then same question repeats
///   All 5 correct      →  completion message, dialogue closes
/// </summary>
public class PoliceDialogue : MonoBehaviour
{
    // =========================================================
    //  EDIT YOUR CONTENT HERE
    //  -------------------------------------------------------
    //  NPC_NAME       : displayed in green at the top of the box
    //  INTRO_TEXT     : shown before the quiz starts (Enter/Esc)
    //  COMPLETE_TEXT  : shown after all 5 questions are answered
    //  QUESTIONS[]    : the five quiz questions
    //    prompt       : the question text
    //    opt1/2/3     : the three answer options
    //    correct      : which option is right (1, 2, or 3)
    //    wrongFeedback: shown when the player picks a wrong answer
    // =========================================================

    private const string NPC_NAME = "Officer Maria";

    private const string INTRO_TEXT =
        "Hello! I am Officer Maria. I will help you get started in Romania.\n\n" +
        "I have a short quiz for you, answer correctly to continue.";

    private const string COMPLETE_TEXT =
        "Well done! You have completed the orientation quiz.\n\n" +
        "Bun venit în România!";

    private struct Question
    {
        public string prompt;
        public string opt1, opt2, opt3;
        public int    correct;        // 1, 2, or 3
        public string wrongFeedback;
    }

    private static readonly Question[] QUESTIONS =
    {
        new Question
        {
            prompt        = "Care este capitala României?",
            opt1          = "Cluj-Napoca",
            opt2          = "Timișoara",
            opt3          = "București",
            correct       = 3,
            wrongFeedback = "Not quite. Romania's capital is Bucharest, a large city in the south of the country."
        },
        new Question
        {
            prompt        = "Numărul de urgență în românia este?",
            opt1          = "911",
            opt2          = "112",
            opt3          = "999",
            correct       = 2,
            wrongFeedback = "Incorrect. In Romania and across the EU, the emergency number is 112."
        },
        new Question
        {
            prompt        = "Cum se numește moneda națională în România?",
            opt1          = "Euro",
            opt2          = "Zloty",
            opt3          = "Leu",
            correct       = 3,
            wrongFeedback = "Wrong. Romania has its own currency: the Romanian Leu (RON). It has not adopted the Euro yet."
        },
        new Question
        {
            prompt        = "Care din următoarele sunt formule de salut?",
            opt1          = "Vă rog/Mulțumesc",
            opt2          = "Bună ziua/Ceau/La revedere",
            opt3          = "Vă rog/Ceau",
            correct       = 2,
            wrongFeedback = "Not quite. Greetings in Romanian are „bună ziua” (good day), „ceau” (hi), „salut” (hi), „la revedere” (see you soon) and so on. „Mulțumesc” means thank you and „vă rog” means please. "
        },
        new Question
        {
            prompt        = "Prezintă-te",
            opt1          = "Salut! Numele meu este Alex, încântat de cunoștință!",
            opt2          = "Salut! O pâine te rog și o plasă cu mere!",
            opt3          = "Am venit cu mașina în oraș.",
            correct       = 1,
            wrongFeedback = "Incorrect. In order to introduce yourself you have to greet the other party, then say your name (numele)"
        }
    };

    // =========================================================
    //  END OF EDITABLE CONTENT
    // =========================================================

    private enum State { Idle, Intro, Question, Feedback, Complete }
    private State  state           = State.Idle;
    private int    questionIndex   = 0;

    // ── Entry point — called by NPCInteraction.Interact() ────
    public void BeginInteraction()
    {
        if (state != State.Idle) return;
        if (DialogueManager.Instance == null) return;

        state = State.Intro;
        DialogueManager.Instance.Show(INTRO_TEXT, OnIntroAccepted, OnIntroDeclined);

        // Override speaker name so it shows "Officer Maria" instead of nothing
        // (Show() doesn't set speakerNameText — harmless if it stays blank for intro)
    }

    private void OnIntroAccepted()
    {
        questionIndex = 0;
        state = State.Question;
        ShowQuestion();
    }

    private void OnIntroDeclined()
    {
        state = State.Idle;
    }

    // ── Per-frame input (only active during quiz) ─────────────
    void Update()
    {
        switch (state)
        {
            case State.Question:
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) HandleAnswer(1);
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) HandleAnswer(2);
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) HandleAnswer(3);
                if (Input.GetKeyDown(KeyCode.Escape)) Abandon();
                break;

            case State.Feedback:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    state = State.Question;
                    ShowQuestion();
                }
                if (Input.GetKeyDown(KeyCode.Escape)) Abandon();
                break;

            case State.Complete:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    state = State.Idle;
                    DialogueManager.Instance.Close();
                }
                break;
        }
    }

    // ── Display helpers ───────────────────────────────────────
    private void ShowQuestion()
    {
        Question q = QUESTIONS[questionIndex];
        int total  = QUESTIONS.Length;

        string body =
            $"<size=19><color=#AAAAAA>Question {questionIndex + 1} of {total}</color></size>\n\n" +
            $"{q.prompt}\n\n" +
            $"<color=#88CCFF>[1]</color>  {q.opt1}\n" +
            $"<color=#88CCFF>[2]</color>  {q.opt2}\n" +
            $"<color=#88CCFF>[3]</color>  {q.opt3}";

        DialogueManager.Instance.DisplayOnly(
            NPC_NAME,
            body,
            "<color=#88CCFF>[1 / 2 / 3]</color> Pick an answer        <color=#FF6666>[Esc]</color> Quit"
        );
    }

    private void ShowFeedback(string feedback)
    {
        DialogueManager.Instance.DisplayOnly(
            NPC_NAME,
            feedback,
            "<color=#44FF88>[Enter]</color> Try again        <color=#FF6666>[Esc]</color> Quit"
        );
    }

    private void ShowComplete()
    {
        DialogueManager.Instance.DisplayOnly(
            NPC_NAME,
            COMPLETE_TEXT,
            "<color=#44FF88>[Enter]</color> Finish"
        );
    }

    // ── Answer logic ──────────────────────────────────────────
    private void HandleAnswer(int choice)
    {
        Question q = QUESTIONS[questionIndex];

        if (choice == q.correct)
        {
            questionIndex++;
            if (questionIndex >= QUESTIONS.Length)
            {
                state = State.Complete;
                ShowComplete();
            }
            else
            {
                ShowQuestion();
            }
        }
        else
        {
            state = State.Feedback;
            ShowFeedback(q.wrongFeedback);
        }
    }

    private void Abandon()
    {
        state = State.Idle;
        DialogueManager.Instance.Close();
    }
}

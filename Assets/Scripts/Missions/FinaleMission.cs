using System.Collections.Generic;
using UnityEngine;
using O = MissionOption;

public class FinaleMission : MissionContent
{
    public override string Title => "The Grand Final Challenge";

    public override List<MissionStep> BuildSteps()
    {
        return new List<MissionStep>
        {
            MissionStep.Info(
                "One last challenge",
                "You've visited the kitchen, the train station, the castle and the theatre. Now let's " +
                "see how much you've learned about Romania — one question from each subject!"),

            MissionStep.Single(
                "Cuisine — which dish is a cabbage roll filled with minced meat and rice?",
                "<b>Sarmale</b> are the star of festive Romanian meals.",
                O.Right("Sarmale"),
                O.Wrong("Mămăligă"),
                O.Wrong("Papanași")),

            MissionStep.Single(
                "Geography — which mountain range arcs through the middle of Romania?",
                "The <b>Carpathians</b> curve through the country, home to brown bears and lynx.",
                O.Right("The Carpathians"),
                O.Wrong("The Alps"),
                O.Wrong("The Andes")),

            MissionStep.Order(
                "History — put these events in chronological order (oldest at the top):",
                "From Roman Dacia (106), to the small union (1859), the Great Union (1918) and the 1989 Revolution.",
                "106 — Roman conquest of Dacia",
                "1859 — Union of Moldavia and Wallachia",
                "1918 — The Great Union",
                "1989 — Romanian Revolution"),

            MissionStep.Single(
                "Music — Gheorghe Zamfir is a world-famous master of which instrument?",
                "The <b>nai</b> (pan flute) — its haunting sound made him famous worldwide.",
                O.Right("The pan flute (nai)"),
                O.Wrong("The piano"),
                O.Wrong("The trumpet")),

            MissionStep.Multi(
                "Accessibility — what makes a city more welcoming for everyone?",
                "Ramps, audible traffic signals and reserved parking all help people with disabilities.",
                O.Right("Wheelchair ramps"),
                O.Right("Audible pedestrian signals"),
                O.Right("Reserved accessible parking"),
                O.Wrong("Stairs with no alternative"),
                O.Wrong("Blocked sidewalks")),

            MissionStep.Info(
                "Congratulations!",
                "You've passed the Grand Final Challenge! You're now a true guide to Romanian culture.\n" +
                "Press Finish to receive your title.")
        };
    }
}

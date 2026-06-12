using System.Collections.Generic;
using UnityEngine;
using O = MissionOption;

/// <summary>
/// History mission content (the museum / castle scene). Attach to the
/// MissionStation object.
/// </summary>
public class HistoryMission : MonoBehaviour, IMissionContent
{
    public string Title => "Romanian History";

    public List<MissionStep> BuildSteps()
    {
        return new List<MissionStep>
        {
            MissionStep.Info(
                "Welcome to the museum",
                "Romania's story runs from ancient Dacia, through the union of its principalities, " +
                "to a modern European nation. Let's walk the timeline."),

            MissionStep.Order(
                "Drag these key events into chronological order (oldest at the top):",
                "From Roman Dacia (106 AD), to the 1859 union of Moldavia and Wallachia, the 1918 " +
                "Great Union, and finally the 1989 Revolution that ended communism.",
                "106 AD — Roman conquest of Dacia",
                "1859 — Union of Moldavia and Wallachia",
                "1918 — Great Union (Marea Unire)",
                "1989 — Romanian Revolution"),

            MissionStep.Single(
                "Who became ruler when Moldavia and Wallachia united in 1859?",
                "<b>Alexandru Ioan Cuza</b> was elected in both principalities, creating the modern " +
                "Romanian state through a clever double election.",
                O.Right("Alexandru Ioan Cuza"),
                O.Wrong("Vlad Țepeș"),
                O.Wrong("Decebal")),

            MissionStep.Single(
                "Romania's National Day is 1 December. What does it celebrate?",
                "1 December marks the <b>1918 Great Union</b>, when Transylvania united with the " +
                "Romanian Kingdom.",
                O.Right("The 1918 Great Union"),
                O.Wrong("Joining the European Union"),
                O.Wrong("The 1989 Revolution")),

            MissionStep.Single(
                "Decebal was the last king of which ancient kingdom?",
                "<b>Decebal</b> ruled <b>Dacia</b> and resisted Rome until Emperor Trajan conquered " +
                "it in 106 AD — commemorated on Trajan's Column in Rome.",
                O.Right("Dacia"),
                O.Wrong("The Roman Empire"),
                O.Wrong("Moldavia")),

            MissionStep.Info(
                "End of the tour",
                "You placed the timeline and met Cuza, Decebal and the date of the Great Union.\n" +
                "Press Finish to return to the island.")
        };
    }
}

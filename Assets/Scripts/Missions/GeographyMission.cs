using System.Collections.Generic;
using UnityEngine;
using O = MissionOption;

/// <summary>
/// Geography &amp; legends mission content (the map-room scene). Attach to the
/// MissionStation object.
/// </summary>
public class GeographyMission : MonoBehaviour, IMissionContent
{
    public string Title => "Geography & Legends";

    public List<MissionStep> BuildSteps()
    {
        return new List<MissionStep>
        {
            MissionStep.Info(
                "The map room",
                "Romania sits where the Carpathian Mountains, the Danube and the Black Sea meet. " +
                "Let's explore its regions — and one famous legend."),

            MissionStep.Multi(
                "Select Romania's historical regions (and avoid the foreign ones):",
                "Transylvania, Moldavia, Wallachia, Banat and Dobruja are Romania's historical " +
                "regions. Bavaria, Tuscany and Andalusia are elsewhere in Europe.",
                O.Right("Transylvania (Transilvania)"),
                O.Right("Moldavia (Moldova)"),
                O.Right("Wallachia (Țara Românească)"),
                O.Right("Dobruja (Dobrogea)"),
                O.Wrong("Bavaria"),
                O.Wrong("Tuscany"),
                O.Wrong("Andalusia")),

            MissionStep.Single(
                "Which great mountain range arcs through the middle of Romania?",
                "The <b>Carpathians</b> curve through the country and are home to brown bears, " +
                "lynx and the Transfăgărășan road.",
                O.Right("The Carpathians"),
                O.Wrong("The Alps"),
                O.Wrong("The Andes")),

            MissionStep.Single(
                "Where the Danube meets the Black Sea, it forms a vast wetland called the...?",
                "The <b>Danube Delta</b> is a UNESCO biosphere reserve — Europe's largest reed bed " +
                "and a paradise for birds.",
                O.Right("Danube Delta"),
                O.Wrong("Sahara Desert"),
                O.Wrong("Norwegian Fjord")),

            MissionStep.Single(
                "Bran Castle is popularly tied to which legendary figure?",
                "Bram Stoker's <b>Dracula</b> was loosely inspired by <b>Vlad Țepeș</b> (Vlad the " +
                "Impaler), a 15th-century ruler of Wallachia.",
                O.Right("Dracula / Vlad Țepeș"),
                O.Wrong("Robin Hood"),
                O.Wrong("King Arthur")),

            MissionStep.Single(
                "True or false: the vampire Dracula was a real historical person.",
                "False. The <b>vampire</b> Dracula is fiction. The real Vlad Țepeș was a harsh but " +
                "very human prince — the myth grew long after his death.",
                O.Right("False — the vampire is fiction"),
                O.Wrong("True — he really was a vampire")),

            MissionStep.Info(
                "Drum bun! (Safe travels)",
                "You mapped the regions, the Carpathians and the Delta, and separated the Dracula " +
                "myth from history.\nPress Finish to return to the island.")
        };
    }
}

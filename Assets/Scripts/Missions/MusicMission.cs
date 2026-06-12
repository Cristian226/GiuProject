using System.Collections.Generic;
using UnityEngine;
using O = MissionOption;

/// <summary>
/// Music mission content (the theatre / concert-hall scene). Attach to the
/// MissionStation object.
/// </summary>
public class MusicMission : MonoBehaviour, IMissionContent
{
    public string Title => "Romanian Music";

    public List<MissionStep> BuildSteps()
    {
        return new List<MissionStep>
        {
            MissionStep.Info(
                "Welcome to the theatre",
                "From village hora dances to George Enescu's concert halls, Romanian music " +
                "spans haunting folk laments and world-class classical works. Let's listen in."),

            MissionStep.Multi(
                "Select the traditional Romanian folk instruments:",
                "The <b>nai</b> (pan flute), <b>cobză</b> (lute), <b>țambal</b> (cimbalom), " +
                "<b>fluier</b> (flute) and <b>cimpoi</b> (bagpipe) are all part of Romanian folk music.",
                O.Right("Nai (pan flute)"),
                O.Right("Cobză (lute)"),
                O.Right("Țambal (cimbalom)"),
                O.Right("Fluier (flute)"),
                O.Wrong("Sitar"),
                O.Wrong("Banjo"),
                O.Wrong("Didgeridoo")),

            MissionStep.Single(
                "What is a <b>doina</b>?",
                "A <b>doina</b> is a slow, improvised solo song — a lyrical lament expressing " +
                "longing (dor). It is on UNESCO's list of intangible cultural heritage.",
                O.Right("A melancholic, improvised solo song"),
                O.Wrong("A fast brass march"),
                O.Wrong("A wooden percussion instrument"))
                .WithWrongFeedback("Not quite — the doina is a slow, mournful improvised song, not a dance or instrument."),

            MissionStep.Single(
                "The <b>hora</b> is a...?",
                "The <b>hora</b> is a traditional circle dance: dancers hold hands and step " +
                "together. It's a staple of weddings and village festivals.",
                O.Right("Traditional circle dance"),
                O.Wrong("Type of violin"),
                O.Wrong("Festive bread"))
                .WithWrongFeedback("Not quite — the hora is a round dance where everyone holds hands."),

            MissionStep.Single(
                "Who is Romania's most celebrated classical composer, known for the <b>Romanian Rhapsodies</b>?",
                "<b>George Enescu</b> (1881–1955), composer and virtuoso violinist, wove folk " +
                "melodies into orchestral music. Bucharest's international festival bears his name.",
                O.Right("George Enescu"),
                O.Wrong("Frédéric Chopin"),
                O.Wrong("Johann Strauss"))
                .WithWrongFeedback("Incorrect — that's George Enescu, Romania's greatest composer."),

            MissionStep.Single(
                "<b>Gheorghe Zamfir</b> is a world-famous master of which instrument?",
                "<b>Gheorghe Zamfir</b> made the <b>nai</b> (pan flute) famous around the world " +
                "with its haunting, breathy sound.",
                O.Right("The pan flute (nai)"),
                O.Wrong("The piano"),
                O.Wrong("The trumpet"))
                .WithWrongFeedback("Not quite — Zamfir is the legendary master of the pan flute (nai)."),

            MissionStep.Info(
                "Bravo!",
                "You met the nai, cobză and țambal, the doina and the hora, and the giants " +
                "Enescu and Zamfir.\nPress Finish to return to the island.")
        };
    }
}

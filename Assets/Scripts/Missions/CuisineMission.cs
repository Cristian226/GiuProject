using System.Collections.Generic;
using UnityEngine;
using O = MissionOption;

/// <summary>
/// Cuisine mission content (the kitchen scene). Attach to the MissionStation
/// object. Edit the steps below freely — text supports TMP rich-text tags.
/// </summary>
public class CuisineMission : MonoBehaviour, IMissionContent
{
    public string Title => "Romanian Cuisine";

    public List<MissionStep> BuildSteps()
    {
        return new List<MissionStep>
        {
            MissionStep.Info(
                "Welcome to the kitchen",
                "Romanian food blends Balkan, Ottoman, Slavic and Central-European flavours.\n" +
                "Let's cook a few traditional dishes and learn their names. Press Continue."),

            MissionStep.Multi(
                "Assemble traditional <b>SARMALE</b> (cabbage rolls). Pick every ingredient that belongs:",
                "Sarmale are minced meat and rice wrapped in pickled cabbage (or vine) leaves, " +
                "slow-cooked and often served with mămăligă and sour cream. A festive favourite!",
                O.Right("Cabbage leaves (varză murată)"),
                O.Right("Minced pork (carne tocată de porc)"),
                O.Right("Rice (orez)"),
                O.Right("Onion (ceapă)"),
                O.Wrong("Soy sauce"),
                O.Wrong("Curry paste"),
                O.Wrong("Parmesan")),

            MissionStep.Single(
                "Which dish is a polenta-like staple made from boiled cornmeal?",
                "<b>Mămăligă</b> is boiled cornmeal — a humble staple eaten with cheese, sour cream, " +
                "or alongside stews.",
                O.Right("Mămăligă"),
                O.Wrong("Baklava"),
                O.Wrong("Paella")),

            MissionStep.Single(
                "What are <b>mici</b> (mititei)?",
                "<b>Mici</b> are skinless grilled rolls of seasoned minced meat — the star of any " +
                "Romanian barbecue, usually eaten with mustard and bread.",
                O.Right("Grilled minced-meat rolls"),
                O.Wrong("A cold fruit soup"),
                O.Wrong("A type of cheese")),

            MissionStep.Single(
                "A <b>ciorbă</b> is a kind of...?",
                "<b>Ciorbă</b> is a sour soup, soured with borș (fermented wheat bran) or lemon. " +
                "Ciorbă de burtă (tripe) and ciorbă de perișoare (meatball) are classics.",
                O.Right("Sour soup"),
                O.Wrong("Flatbread"),
                O.Wrong("Grilled sausage")),

            MissionStep.Single(
                "<b>Papanași</b> are a traditional...?",
                "<b>Papanași</b> are fried (or boiled) cheese doughnuts topped with sour cream and " +
                "jam — a beloved dessert.",
                O.Right("Cheese-doughnut dessert"),
                O.Wrong("Spicy meat stew"),
                O.Wrong("Herbal drink")),

            MissionStep.Info(
                "Poftă bună! (Enjoy your meal)",
                "You learned about sarmale, mămăligă, mici, ciorbă and papanași.\n" +
                "Press Finish to head back to the island.")
        };
    }
}

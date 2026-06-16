using System.Collections.Generic;
using UnityEngine;

public abstract class MissionContent : MonoBehaviour
{
    public abstract string Title { get; }
    public abstract List<MissionStep> BuildSteps();
}

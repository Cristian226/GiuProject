/// <summary>
/// Anything the player can look at and activate with E / left-click.
/// Driven centrally by <see cref="PlayerInteraction"/> via a camera raycast, so
/// only the single object under the crosshair is ever triggered (no double-fires
/// when props are clustered together).
/// </summary>
public interface IInteractable
{
    /// <summary>Short verb shown in the on-screen hint, e.g. "Talk", "Inspect".</summary>
    string Prompt { get; }

    /// <summary>Called when the player interacts with this object.</summary>
    void Interact();
}

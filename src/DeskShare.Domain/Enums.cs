namespace DeskShare.Domain;

[Flags]
public enum DeskFeatures
{
    None = 0,
    Monitor = 1,
    Standing = 2,
    Window = 4,
}

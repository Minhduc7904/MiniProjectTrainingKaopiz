namespace BuildingBlocks.Presentation.Actors;

[Flags]
public enum ActorAccess
{
    None = 0,
    Admin = 1,
    Student = 2,
    Any = Admin | Student,
}

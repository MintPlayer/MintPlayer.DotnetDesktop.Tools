namespace MintPlayer.ThreeDee.Commands;

/// <summary>A reversible edit. <see cref="Do"/> is also used to redo (it recreates its effect).</summary>
public interface ICommand
{
    string Name { get; }
    void Do();
    void Undo();
}

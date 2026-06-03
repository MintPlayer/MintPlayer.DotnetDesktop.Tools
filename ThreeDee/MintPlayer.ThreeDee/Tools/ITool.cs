namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// A modeling tool: a state machine that handles viewport input for one operation. Exactly one
/// tool is active at a time. Navigation (orbit/pan/zoom) is handled by the viewport regardless of
/// the active tool, so tools only see selection/drawing gestures.
/// </summary>
public interface ITool
{
    string Name { get; }
    void Activate(ToolContext ctx);
    void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers);
    void OnMouseMove(ToolContext ctx, Point p, Keys modifiers);
    void OnMouseUp(ToolContext ctx, MouseButtons button, Point p, Keys modifiers);
    void OnDoubleClick(ToolContext ctx, Point p, Keys modifiers);
    void OnKeyDown(ToolContext ctx, Keys key);
    void DrawOverlay(Graphics g, ToolContext ctx);

    /// <summary>One-line instruction shown in the status bar while this tool is active.</summary>
    string Hint { get; }

    /// <summary>True when a typed value (the VCB) would refine the current operation.</summary>
    bool WantsValue { get; }

    /// <summary>Live measurement to show in the VCB (length, distance, dimensions), or null.</summary>
    string? Measurement { get; }

    /// <summary>Apply a typed VCB value to the current operation; return true if consumed.</summary>
    bool TryApplyValue(ToolContext ctx, string text);
}

/// <summary>Convenience base so tools only override the events they care about.</summary>
public abstract class ToolBase : ITool
{
    public abstract string Name { get; }
    public virtual void Activate(ToolContext ctx) { }
    public virtual void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers) { }
    public virtual void OnMouseMove(ToolContext ctx, Point p, Keys modifiers) { }
    public virtual void OnMouseUp(ToolContext ctx, MouseButtons button, Point p, Keys modifiers) { }
    public virtual void OnDoubleClick(ToolContext ctx, Point p, Keys modifiers) { }
    public virtual void OnKeyDown(ToolContext ctx, Keys key) { }
    public virtual void DrawOverlay(Graphics g, ToolContext ctx) { }
    public virtual string Hint => "";
    public virtual bool WantsValue => false;
    public virtual string? Measurement => null;
    public virtual bool TryApplyValue(ToolContext ctx, string text) => false;
}

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>SketchUp-style display styles (PRD §5.6).</summary>
public enum RenderStyle
{
    /// <summary>Edges only, nothing occludes anything (see-through).</summary>
    Wireframe,
    /// <summary>Faces occlude but are not shaded; only edges are drawn.</summary>
    HiddenLine,
    /// <summary>Flat-shaded faces, no edges.</summary>
    Shaded,
    /// <summary>Flat-shaded faces with feature edges drawn on top.</summary>
    ShadedEdges,
}

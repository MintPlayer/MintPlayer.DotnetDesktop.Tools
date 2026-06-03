using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee;

/// <summary>
/// Builds the M2 demo document as real half-edge topology: a few box solids (built and stitched
/// through the geometry layer) plus one open quad "panel" that shows back-face tinting when
/// orbited behind. Replaces M1's hand-built triangle soup.
/// </summary>
public static class DemoScene
{
    public static Scene Build()
    {
        var scene = new Scene();
        var mesh = scene.Mesh;

        MeshBuilder.Box(mesh, new Vector3(0, 1.0f, 0), new Vector3(4, 2, 3), new Vector3(200, 120, 90));      // body
        MeshBuilder.Box(mesh, new Vector3(0, 2.6f, 0), new Vector3(4.4f, 1.2f, 3.4f), new Vector3(170, 80, 70)); // roof block
        MeshBuilder.Box(mesh, new Vector3(3.2f, 0.75f, 1), new Vector3(1.5f, 1.5f, 1.5f), new Vector3(110, 150, 200)); // cube
        MeshBuilder.Box(mesh, new Vector3(-3f, 1.75f, -1.5f), new Vector3(1.2f, 3.5f, 1.2f), new Vector3(120, 180, 120)); // pillar

        // An open single-sided quad standing upright — front off-white, back tinted blue.
        mesh.AddFace(
        [
            mesh.AddVertex(new Vector3(-6, 0, 3)), mesh.AddVertex(new Vector3(-3.5f, 0, 3)),
            mesh.AddVertex(new Vector3(-3.5f, 2.2f, 3)), mesh.AddVertex(new Vector3(-6, 2.2f, 3)),
        ], new Vector3(225, 225, 230));

        mesh.Twin(); // stitch all shared edges across the whole mesh at once
        return scene;
    }
}

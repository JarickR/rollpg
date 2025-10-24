// res://Scripts/Godot/EdgeFrame3D.cs
#nullable enable
using Godot;
using System.Collections.Generic;

namespace DiceArena.Godot
{
	/// <summary>
	/// Procedurally builds thin box "rods" along the 12 edges of a cube to create visible borders.
	/// Add this as a child of your dice. Assumes a 2*CubeHalf sized cube centered at origin.
	/// </summary>
	public partial class EdgeFrame3D : Node3D
	{
		[Export(PropertyHint.Range, "0.1,2.0,0.01")]
		public float CubeHalf { get; set; } = 0.5f; // half-extent of your dice (0.5 for a 1x1x1 cube)

		[Export(PropertyHint.Range, "0.001,0.2,0.001")]
		public float EdgeThickness { get; set; } = 0.02f;

		[Export]
		public Color EdgeColor { get; set; } = new Color(0.85f, 0.85f, 0.85f); // silver-ish

		[Export]
		public bool Unshaded { get; set; } = true;

		[Export]
		public bool NoDepthTest { get; set; } = false; // true if you always want edges on top

		private readonly List<MeshInstance3D> _edges = new();

		public override void _Ready()
		{
			BuildEdges();
		}

		public override void _ExitTree()
		{
			foreach (var e in _edges)
				e.QueueFree();
			_edges.Clear();
		}

		private void BuildEdges()
		{
			// Clean any old
			foreach (var e in _edges)
				e.QueueFree();
			_edges.Clear();

			// One reusable thin box mesh (length along local Z)
			var rod = new BoxMesh
			{
				Size = new Vector3(EdgeThickness, EdgeThickness, 2f * CubeHalf + EdgeThickness)
			};

			var mat = new StandardMaterial3D
			{
				AlbedoColor = EdgeColor,
				ShadingMode = Unshaded ? BaseMaterial3D.ShadingModeEnum.Unshaded
									   : BaseMaterial3D.ShadingModeEnum.PerPixel,
				NoDepthTest = NoDepthTest
			};

			// Helper to instance a rod with transform and material:
			MeshInstance3D MakeRod(Vector3 pos, Basis rot)
			{
				var mi = new MeshInstance3D
				{
					Mesh = rod,
					Transform = new Transform3D(rot, pos)
				};
				mi.MaterialOverride = mat;
				AddChild(mi);
				_edges.Add(mi);
				return mi;
			}

			float h = CubeHalf;

			// Rod default is along local Z, so:
			// - For Z edges: use Basis.Identity
			// - For X edges: rotate around Y by +90°
			// - For Y edges: rotate around X by +90°
			var rotZ = Basis.Identity;
			var rotX = Basis.Identity.Rotated(Vector3.Up, Mathf.Pi * 0.5f);
			var rotY = Basis.Identity.Rotated(Vector3.Right, Mathf.Pi * 0.5f);

			// Along Z (x=±h, y=±h, z spans)
			MakeRod(new Vector3( +h, +h, 0), rotZ);
			MakeRod(new Vector3( +h, -h, 0), rotZ);
			MakeRod(new Vector3( -h, +h, 0), rotZ);
			MakeRod(new Vector3( -h, -h, 0), rotZ);

			// Along X (y=±h, z=±h, x spans)
			MakeRod(new Vector3( 0, +h, +h), rotX);
			MakeRod(new Vector3( 0, +h, -h), rotX);
			MakeRod(new Vector3( 0, -h, +h), rotX);
			MakeRod(new Vector3( 0, -h, -h), rotX);

			// Along Y (x=±h, z=±h, y spans)
			MakeRod(new Vector3( +h, 0, +h), rotY);
			MakeRod(new Vector3( -h, 0, +h), rotY);
			MakeRod(new Vector3( +h, 0, -h), rotY);
			MakeRod(new Vector3( -h, 0, -h), rotY);
		}
	}
}

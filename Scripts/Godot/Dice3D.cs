// res://Scripts/Godot/Dice3D.cs
#nullable enable
using Godot;
using System.Collections.Generic;
using System.IO;

namespace DiceArena.Godot
{
	/// <summary>
	/// A physics dice whose six faces are Sprite3D or MeshInstance3D children.
	/// We paint those faces with textures from the player's loadout and
	/// keep helpers to read which face is up after a roll.
	/// </summary>
	public partial class Dice3D : RigidBody3D
	{
		[Export] public float DepthBiasUp    { get; set; } = 0.0f;
		[Export] public float DepthBiasDown  { get; set; } = 0.0f;
		[Export] public float DepthBiasLeft  { get; set; } = 0.0f;
		[Export] public float DepthBiasRight { get; set; } = 0.0f;
		[Export] public float DepthBiasFront { get; set; } = 0.0f;
		[Export] public float DepthBiasBack  { get; set; } = 0.0f;

		// Face node paths (children of the dice)
		[Export] public NodePath FaceUpPath    { get; set; } = default!;
		[Export] public NodePath FaceDownPath  { get; set; } = default!;
		[Export] public NodePath FaceLeftPath  { get; set; } = default!;
		[Export] public NodePath FaceRightPath { get; set; } = default!;
		[Export] public NodePath FaceFrontPath { get; set; } = default!;
		[Export] public NodePath FaceBackPath  { get; set; } = default!;

		// Tunables
		[Export(PropertyHint.Range, "0,0.05,0.001")]
		public float IconDepthEpsilon { get; set; } = 0.015f;  // offset outward along local Z

		[Export(PropertyHint.Range, "0.5,2.5,0.01")]
		public float IconUniformScale { get; set; } = 1.35f;   // you can raise this to make faces bigger

		[Export] public Vector3 IconLocalOffset { get; set; } = Vector3.Zero;
		[Export] public bool    FaceNoDepthTest { get; set; } = true;

		// Cached nodes
		private Node3D _up    = default!;
		private Node3D _down  = default!;
		private Node3D _left  = default!;
		private Node3D _right = default!;
		private Node3D _front = default!;
		private Node3D _back  = default!;

		// Cache original LOCAL transforms so repeated calls don’t drift
		private readonly Dictionary<Node3D, Vector3> _baseLocalPos   = new();
		private readonly Dictionary<Node3D, Vector3> _baseLocalScale = new();

		public override void _Ready()
		{
			_up    = GetNode<Node3D>(FaceUpPath);
			_down  = GetNode<Node3D>(FaceDownPath);
			_left  = GetNode<Node3D>(FaceLeftPath);
			_right = GetNode<Node3D>(FaceRightPath);
			_front = GetNode<Node3D>(FaceFrontPath);
			_back  = GetNode<Node3D>(FaceBackPath);

			CacheBaseTransform(_up);
			CacheBaseTransform(_down);
			CacheBaseTransform(_left);
			CacheBaseTransform(_right);
			CacheBaseTransform(_front);
			CacheBaseTransform(_back);
		}

		private void CacheBaseTransform(Node3D n)
		{
			_baseLocalPos[n]   = n.Position;
			_baseLocalScale[n] = n.Scale;
		}

		/// <summary>
		/// Maps:
		///   Up    = classIcon (index 0)
		///   Down  = upgradeIcon (index 5)
		///   Left/Right/Front/Back = slotIcons1to4 order (indices 1..4)
		/// Any missing/null uses fallback (hidden when null).
		/// </summary>
		public void ApplyLoadoutFaces(
			Texture2D? classIcon,
			System.Collections.Generic.IReadOnlyList<Texture2D?> slotIcons1to4,
			Texture2D? upgradeIcon,
			Texture2D? fallback = null)
		{
			Texture2D? s0 = slotIcons1to4.Count > 0 ? (slotIcons1to4[0] ?? fallback) : fallback;
			Texture2D? s1 = slotIcons1to4.Count > 1 ? (slotIcons1to4[1] ?? fallback) : fallback;
			Texture2D? s2 = slotIcons1to4.Count > 2 ? (slotIcons1to4[2] ?? fallback) : fallback;
			Texture2D? s3 = slotIcons1to4.Count > 3 ? (slotIcons1to4[3] ?? fallback) : fallback;

			SetFace(_up,    classIcon ?? fallback);
			SetFace(_down,  upgradeIcon ?? fallback);
			SetFace(_left,  s0);
			SetFace(_right, s1);
			SetFace(_front, s2);
			SetFace(_back,  s3);

			if (_up.Visible)    _up.Position    += new Vector3(0, 0, DepthBiasUp);
			if (_down.Visible)  _down.Position  += new Vector3(0, 0, DepthBiasDown);
			if (_left.Visible)  _left.Position  += new Vector3(0, 0, DepthBiasLeft);
			if (_right.Visible) _right.Position += new Vector3(0, 0, DepthBiasRight);
			if (_front.Visible) _front.Position += new Vector3(0, 0, DepthBiasFront);
			if (_back.Visible)  _back.Position  += new Vector3(0, 0, DepthBiasBack);
		}

		// --- NEW: determine which face is up after physics settles ---
		/// <summary>
		/// Returns (topIndex, texture, semanticName) where topIndex uses the same mapping
	/// as ApplyLoadoutFaces: 0=Up(class), 1=Left, 2=Right, 3=Front, 4=Back, 5=Down(upgrade).
		/// If nothing is visible, returns (-1, null, "none").
		/// </summary>
		public (int topIndex, Texture2D? texture, string name) ResolveTopFace()
		{
			var candidates = new (int idx, Node3D node, string name)[]
			{
				(0, _up,    "class"),
				(5, _down,  "upgrade"),
				(1, _left,  "slot1"),
				(2, _right, "slot2"),
				(3, _front, "slot3"),
				(4, _back,  "slot4")
			};

			int bestIdx = -1;
			float bestDot = -999f;
			Texture2D? bestTex = null;
			string bestName = "none";

			foreach (var f in candidates)
			{
				if (!IsInstanceValid(f.node) || !f.node.Visible) continue;

				// Sprite3D / Quad faces look along -Z. Outward normal is -Z in world space.
				Vector3 outward = -(f.node.GlobalTransform.Basis.Z).Normalized();
				float d = outward.Dot(Vector3.Up);
				if (d > bestDot)
				{
					bestDot  = d;
					bestIdx  = f.idx;
					bestTex  = ExtractTexture(f.node);
					bestName = f.name;

					// If almost perfectly up, we can short-circuit
					if (bestDot > 0.999f) break;
				}
			}

			return (bestIdx, bestTex, bestName);
		}

		private static Texture2D? ExtractTexture(Node3D node)
		{
			switch (node)
			{
				case Sprite3D s:
					return s.Texture as Texture2D
						   ?? (s.MaterialOverride as StandardMaterial3D)?.AlbedoTexture as Texture2D;
				case MeshInstance3D m:
					return (m.MaterialOverride as StandardMaterial3D)?.AlbedoTexture as Texture2D
						   ?? (m.GetActiveMaterial(0) as StandardMaterial3D)?.AlbedoTexture as Texture2D;
				default:
					return null;
			}
		}

		// -------- internals --------
		private void SetFace(Node3D node, Texture2D? tex)
		{
			// Reset to authoring values to avoid cumulative offsets
			node.Position = _baseLocalPos[node];
			node.Scale    = _baseLocalScale[node];

			if (tex is null)
			{
				node.Visible = false;
				return;
			}

			node.Visible = true;

			switch (node)
			{
				case Sprite3D s:
				{
					var mat = s.MaterialOverride as StandardMaterial3D ?? new StandardMaterial3D();
					mat.AlbedoTexture = tex;
					mat.Transparency  = BaseMaterial3D.TransparencyEnum.Alpha;
					mat.NoDepthTest   = FaceNoDepthTest;
					s.MaterialOverride = mat;
					s.Texture = tex;
					break;
				}
				case MeshInstance3D m:
				{
					var mat = (m.MaterialOverride as StandardMaterial3D)
							  ?? (m.GetActiveMaterial(0) as StandardMaterial3D)
							  ?? new StandardMaterial3D();
					mat.AlbedoTexture = tex;
					mat.Transparency  = BaseMaterial3D.TransparencyEnum.Alpha;
					mat.NoDepthTest   = FaceNoDepthTest;
					m.MaterialOverride = mat;
					break;
				}
			}

			// Ensure the icon faces outward and sits slightly off the cube face
			var sc = _baseLocalScale[node];
			sc.Z = -Mathf.Abs(sc.Z);                 // front faces outward
			node.Scale = sc * IconUniformScale;
			node.Position += new Vector3(0, 0, IconDepthEpsilon) + IconLocalOffset;
		}
	}
}

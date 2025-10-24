// res://Scenes/DiceInteractor.cs
#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace DiceArena.Godot
{
	/// Pick-up / hover-drag at fixed height, cursor-centric tumble,
	/// and a velocity-based throw + spin on release.
	/// Attach this script to a plain Node (host), not the SubViewportContainer.
	/// Point OverlayPath at your SubViewportContainer (e.g. "DiceOverlay").
	public partial class DiceInteractor : Node
	{
		// ----- Inspector Paths (set these to your actual nodes) -----
		[Export] public NodePath OverlayPath  { get; set; } = "DiceOverlay";                                // -> SubViewportContainer
		[Export] public NodePath ViewportPath { get; set; } = "DiceOverlay/DiceViewport";                    // -> SubViewport
		[Export] public NodePath CamPath      { get; set; } = "DiceOverlay/DiceViewport/DiceWorld/Camera3D"; // -> Camera3D inside the SubViewport
		[Export] public NodePath DicePath     { get; set; } = "DiceOverlay/DiceViewport/DiceWorld/Dice3d";   // -> RigidBody3D (the dice)
		[Export] public NodePath DefaultDockCardPath { get; set; } = "";                                     // optional UI Control to dock under

		// ----- Tunables -----
		[Export] public bool  AutoShowOnPlay          { get; set; } = true;
		[Export(PropertyHint.Range, "0.1,15.0,0.01")] public float LiftHeight { get; set; } = 0.75f;
		[Export(PropertyHint.Range, "2,40,0.5")]      public float HoverFollowSpeed { get; set; } = 16f;
		[Export(PropertyHint.Range, "0.25,8,0.05")]   public float ThrowImpulseMultiplier { get; set; } = 2.6f;
		[Export(PropertyHint.Range, "0,4,0.05")]      public float TorqueImpulseMultiplier { get; set; } = 1.4f;
		[Export(PropertyHint.Range, "0,2,0.05")]      public float PickupNudgeTorque { get; set; } = 0.25f;
		[Export(PropertyHint.Range, "2,20,1")]        public int   VelocitySampleFrames { get; set; } = 6;
		[Export(PropertyHint.Range, "0.5,10,0.1")]    public float DragMaxRadius { get; set; } = 6f;
		[Export] public bool RequireRayHitToDrag { get; set; } = true;
		[Export] public bool VerboseLogs         { get; set; } = false;

		// Cursor-centric tumble
		[Export(PropertyHint.Range, "0.0005,0.02,0.0005")] public float SpinRadiansPerPixel { get; set; } = 0.006f;
		[Export(PropertyHint.Range, "2,40,0.5")]           public float AngularFollowRate   { get; set; } = 18f;
		[Export(PropertyHint.Range, "0,2,0.01")]           public float CenterBias          { get; set; } = 0.35f;
		[Export] public bool RequireRayHitToDrag { get; set; } = false; // was true
		// ----- Runtime refs -----
		private SubViewportContainer _overlay = default!;
		private SubViewport          _vp      = default!;
		private Camera3D             _cam     = default!;
		private RigidBody3D          _dice    = default!;

		// Drag state
		private bool _dragging;
		private Vector3 _dragTarget;
		private readonly Queue<Vector3> _posSamples = new();

		public override void _Ready()
		{
			// Fetch nodes
			_overlay = GetNode<SubViewportContainer>(OverlayPath);
			_vp      = GetNode<SubViewport>(ViewportPath);
			_cam     = GetNode<Camera3D>(CamPath);
			_dice    = GetNode<RigidBody3D>(DicePath);

			// SubViewport basics
			_vp.TransparentBg = true; // overlay on UI (set UpdateMode/ClearMode in Inspector)

			// Ensure overlay receives mouse events and is on top
			_overlay.MouseFilter = Control.MouseFilterEnum.Stop;
			_overlay.FocusMode   = Control.FocusModeEnum.All;
			_overlay.TopLevel    = true;
			_overlay.ZIndex      = 1000;

			// Keep SubViewport pixels in sync with container size (no stretching)
			_overlay.Stretch = false;
			EnsureOverlaySized();
			SyncViewportToOverlay();
			_overlay.Resized += OnOverlayResized;

			// Camera framing consistency
			_cam.KeepAspect = Camera3D.KeepAspectEnum.Width;

			// RigidBody defaults good for interaction
			_dice.ContinuousCd = true;
			_dice.CanSleep     = false;
			_dice.Freeze       = false;

			// START HIDDEN and NON-BLOCKING
			_overlay.Visible = false;
			_overlay.MouseFilter = Control.MouseFilterEnum.Ignore;     // don't intercept clicks when hidden
			_overlay.ProcessMode = ProcessModeEnum.Disabled;           // don't tick while hidden

			_overlay.GuiInput += OnOverlayGuiInput;

			if (AutoShowOnPlay)
			{
				EnableDice(true);
				SpawnAtCenterAndWake();
				if (VerboseLogs) GD.Print("[DiceInteractor] Auto-show: overlay visible, dice spawned and awake.");
			}

			_dice.GravityScale = 2f;

			SetProcess(false);
			SetPhysicsProcess(true);
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_dragging)
			{
				var cur  = _dice.GlobalTransform.Origin;
				var next = cur.Lerp(_dragTarget, (float)delta * HoverFollowSpeed);

				var t = _dice.GlobalTransform;
				t.Origin = next;
				_dice.GlobalTransform = t;

				SamplePosition(next);
			}
		}

		// ----- SubViewport sizing helpers -----
		private void SyncViewportToOverlay()
		{
			Vector2 sz = _overlay.Size;
			_vp.Size = new Vector2I(Mathf.RoundToInt(sz.X), Mathf.RoundToInt(sz.Y));
		}
		private void OnOverlayResized() => SyncViewportToOverlay();

		private void EnsureOverlaySized(int min = 256)
		{
			if (_overlay.Size.X < 2 || _overlay.Size.Y < 2)
			{
				_overlay.CustomMinimumSize = new Vector2(min, min);
				_overlay.Size = _overlay.CustomMinimumSize;
				if (VerboseLogs) GD.Print($"[DiceInteractor] Overlay minimum size forced to {min}x{min}");
			}
		}

		// ================= Public API =================
		public void EnableDice(bool on)
		{
			_overlay.Visible = on;
			_overlay.MouseFilter = on ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
			_overlay.ProcessMode = on ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

			if (VerboseLogs)
				GD.Print($"[DiceInteractor] EnableDice({on}) → Visible={_overlay.Visible}, MouseFilter={_overlay.MouseFilter}, ProcessMode={_overlay.ProcessMode}");
		}

		public void SpawnAtCenterAndWake()
		{
			var t = _dice.GlobalTransform;
			t.Origin = new Vector3(0f, LiftHeight, 0f);
			_dice.GlobalTransform = t;

			_dice.LinearVelocity  = Vector3.Zero;
			_dice.AngularVelocity = Vector3.Zero;
			_dice.Sleeping        = false;
		}

		public void ResetPose(Transform3D worldTransform)
		{
			_dice.GlobalTransform = worldTransform;
			_dice.LinearVelocity  = Vector3.Zero;
			_dice.AngularVelocity = Vector3.Zero;
			_dice.Sleeping        = false;
		}

		/// Show/position the dice overlay under a UI card (Control) without reparenting.
		public void AppearUnderCard(Control card, int pixelsBelow = 8)
		{
			EnsureOverlaySized();
			SyncViewportToOverlay();

			var r = card.GetGlobalRect();
			float x = r.Position.X + r.Size.X * 0.5f - _overlay.Size.X * 0.5f;
			float y = r.Position.Y + r.Size.Y + pixelsBelow;

			_overlay.GlobalPosition = new Vector2I(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
			EnableDice(true); // make sure it’s visible and interactive

			if (VerboseLogs) GD.Print($"[DiceInteractor] AppearUnderCard -> pos={_overlay.GlobalPosition}, size={_overlay.Size}");
		}

		public void DockToDefaultCard(int pixelsBelow = 8)
		{
			if (string.IsNullOrEmpty(DefaultDockCardPath)) return;
			var card = GetNodeOrNull<Control>(DefaultDockCardPath);
			if (card == null)
			{
				GD.PushWarning($"[DiceInteractor] DefaultDockCardPath not found: '{DefaultDockCardPath}'.");
				return;
			}
			AppearUnderCard(card, pixelsBelow);
		}

		public void HideDice() => EnableDice(false);

		// ================= Input / Dragging =================
		private void OnOverlayGuiInput(InputEvent e)
		{
			if (!_overlay.Visible) return;

			if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
			{
				if (mb.Pressed)
				{
					bool hit = RayHitsDice(mb.Position);
					if (VerboseLogs) GD.Print($"[DiceInteractor] LMB down. RayHitsDice={hit}");

					if (hit || !RequireRayHitToDrag)
						BeginDrag(mb.Position);
					else if (VerboseLogs)
						GD.Print("[DiceInteractor] Click ignored (RequireRayHitToDrag=true and no hit).");
				}
				else
				{
					if (_dragging) EndDragAndThrow();
				}
			}
			else if (e is InputEventMouseMotion mm && _dragging)
			{
				if (TryProjectToPlaneY(mm.Position, LiftHeight, out var worldOnPlane))
				{
					worldOnPlane = ClampToRadius(worldOnPlane);
					MoveDiceTo(worldOnPlane);
				}

				var v2 = mm.Velocity;
				float speed = v2.Length();

				var camBasis   = _cam.GlobalTransform.Basis;
				Vector3 camRight   = camBasis.X;
				Vector3 camForward = -camBasis.Z;
				Vector3 moveWorld  = (camRight * v2.X) + (camForward * v2.Y);

				Vector3 toCursor = _dragTarget - _dice.GlobalTransform.Origin;
				toCursor.Y = 0f;

				Vector3 targetAngVel = Vector3.Zero;

				if (speed > 0.1f && moveWorld.LengthSquared() > 0.0001f)
				{
					moveWorld = moveWorld.Normalized();
					Vector3 baseAxis  = moveWorld.Cross(Vector3.Up).Normalized();

					Vector3 orbitAxis = Vector3.Zero;
					if (toCursor.LengthSquared() > 0.0001f)
					{
						var offsetDir = toCursor.Normalized();
						orbitAxis = offsetDir.Cross(moveWorld).Normalized();
					}

					float orbitWeight = Mathf.Clamp(toCursor.Length() * CenterBias, 0f, 1f);
					Vector3 axis = (baseAxis * (1f - orbitWeight) + orbitAxis * orbitWeight).Normalized();

					float omega = speed * SpinRadiansPerPixel;
					targetAngVel = axis * omega;
				}

				var current = _dice.AngularVelocity;
				var nextAng = current.Lerp(targetAngVel, (float)GetPhysicsProcessDeltaTime() * AngularFollowRate);
				_dice.AngularVelocity = nextAng;
			}
		}

		private void BeginDrag(Vector2 mousePos)
		{
			_dragging = true;
			_posSamples.Clear();

			if (TryProjectToPlaneY(mousePos, LiftHeight, out var worldOnPlane))
			{
				worldOnPlane = ClampToRadius(worldOnPlane);

				var t = _dice.GlobalTransform;
				t.Origin = worldOnPlane;
				_dice.GlobalTransform = t;

				_dragTarget = worldOnPlane;

				_dice.Sleeping        = false;
				_dice.LinearVelocity  = Vector3.Zero;
				_dice.AngularVelocity = Vector3.Zero;

				if (PickupNudgeTorque > 0.0f)
				{
					var rnd = (float)GD.RandRange(0.3, 0.9);
					var torque = new Vector3(rnd * 0.6f, rnd * 0.35f, -rnd * 0.55f) * PickupNudgeTorque;
					_dice.ApplyTorqueImpulse(torque);
				}

				SamplePosition(worldOnPlane);
			}
		}

		private void MoveDiceTo(Vector3 worldTarget) => _dragTarget = worldTarget;

		private void EndDragAndThrow()
		{
			_dragging = false;

			if (_posSamples.Count >= 2)
			{
				Vector3 v = Vector3.Zero;
				Vector3? prev = null;
				foreach (var p in _posSamples) { if (prev.HasValue) v += (p - prev.Value); prev = p; }

				v /= Math.Max(1, _posSamples.Count - 1);
				v *= ThrowImpulseMultiplier * 60f;
				v.Y += ThrowImpulseMultiplier * 0.9f;

				_dice.ApplyImpulse(v);

				var t = new Vector3(
					(v.Z - v.X) * 0.30f,
					(v.X + v.Z) * 0.22f,
					-(v.X - v.Z) * 0.28f
				) * TorqueImpulseMultiplier;

				_dice.ApplyTorqueImpulse(t);
			}

			_posSamples.Clear();
		}

		private void SamplePosition(Vector3 p)
		{
			_posSamples.Enqueue(p);
			while (_posSamples.Count > Math.Max(2, VelocitySampleFrames))
				_ = _posSamples.Dequeue();
		}

		// ================= Helpers =================
		private bool RayHitsDice(Vector2 mousePosInViewport)
		{
			var space  = _dice.GetWorld3D().DirectSpaceState;
			var origin = _cam.ProjectRayOrigin(mousePosInViewport);
			var dir    = _cam.ProjectRayNormal(mousePosInViewport);
			var to     = origin + dir * 1000f;

			var query = PhysicsRayQueryParameters3D.Create(origin, to);
			var res   = space.IntersectRay(query);
			if (res.Count == 0) return false;

			if (!res.TryGetValue("collider", out var colliderVar)) return false;

			var go = colliderVar.AsGodotObject();
			if (go is not Node n) return false;

			var cur = n; int hops = 0;
			while (cur != null && hops++ < 16)
			{
				if (cur == _dice) return true;
				cur = cur.GetParent();
			}
			return false;
		}

		private bool TryProjectToPlaneY(Vector2 mousePosInViewport, float planeY, out Vector3 worldPoint)
		{
			var origin = _cam.ProjectRayOrigin(mousePosInViewport);
			var dir    = _cam.ProjectRayNormal(mousePosInViewport);

			const float EPS = 1e-5f;
			if (Mathf.Abs(dir.Y) < EPS) { worldPoint = Vector3.Zero; return false; }

			float t = (planeY - origin.Y) / dir.Y;
			worldPoint = origin + dir * t;
			return t > 0f;
		}

		private Vector3 ClampToRadius(Vector3 world)
		{
			if (DragMaxRadius <= 0f) return world;
			var camXZ = new Vector2(_cam.GlobalTransform.Origin.X, _cam.GlobalTransform.Origin.Z);
			var pXZ   = new Vector2(world.X, world.Z);
			var delta = pXZ - camXZ;

			if (delta.Length() > DragMaxRadius)
			{
				delta   = delta.Normalized() * DragMaxRadius;
				world.X = camXZ.X + delta.X;
				world.Z = camXZ.Y + delta.Y;
			}
			return world;
		}
	}
}

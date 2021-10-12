using Godot;
using System;

public class CameraOrbit : Spatial
{    
    [Export]
    public Vector2 yLimits = new Vector2(10, 45f);

    [Export]
    public float orbitRadius = 8f;

    private Spatial _cameraPos;

    private Vector2 _camAngle;

    private Camera _camera;

    private float lookSensitivity = 0.08f;

    private Panel _menuPanel;

    private Position3D _fixedCameraPos;

    private Canoe _canoe;

    private Slider _sensitivitySlider;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
        _cameraPos = GetNode<Position3D>("CameraPos");
        _camAngle = new Vector2(0, yLimits.y - yLimits.x);
        _camera = GetNode<Camera>("../MainCamera");
        _menuPanel = GetNode<Panel>("/root/Level/MenuPanel");
        _canoe = GetNode<Canoe>("../");
        _sensitivitySlider = _menuPanel.GetNode<Slider>("SensSlider");
    }

    public override void _Process(float delta)
    {
        if(_fixedCameraPos != null) {
            var transform = GlobalTransform;

            transform.origin = transform.origin.LinearInterpolate(_fixedCameraPos.GlobalTransform.origin, 3f * delta);
            transform.basis = transform.basis.LerpTowards(_fixedCameraPos.GlobalTransform.basis, 3f * delta);

            GlobalTransform = transform;
        } else if (Transform.origin != Vector3.Zero) {
            var transform = Transform;
            transform.origin = transform.origin.LinearInterpolate(Vector3.Zero, 3f * delta);
            transform.basis = transform.basis.LerpTowards(Basis.Identity, 3f * delta);

            if(transform.origin.Length() < 0.1f) {
                transform.origin = Vector3.Zero;
                transform.basis = Basis.Identity;
            }

            Transform = transform;
        }
    }

    public void SetFixedCameraPos(Position3D position) {
        _fixedCameraPos = position;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_cancel")) {
            Input.SetMouseMode(Input.MouseMode.Visible);
            _menuPanel.Visible = true;
        } 
        else if(@event.IsActionPressed("click")) {
            Input.SetMouseMode(Input.MouseMode.Captured);
            GetTree().SetInputAsHandled();
            _menuPanel.Visible = false;
        }

        if(Input.GetMouseMode() != Input.MouseMode.Captured) {
            return;
        }
        if(_fixedCameraPos == null && @event is InputEventMouseMotion mouseEvent) {
            var mouseInput = mouseEvent.Relative;

            _camAngle.x += mouseInput.x * lookSensitivity * (float)_sensitivitySlider.Value;
            _camAngle.y = Mathf.Clamp(_camAngle.y += mouseInput.y * lookSensitivity * (float)_sensitivitySlider.Value, yLimits.x, yLimits.y);

            var camPosTran = _cameraPos.Transform;
            camPosTran.origin = new Quat(new Vector3(Mathf.Deg2Rad(-_camAngle.y), -Mathf.Deg2Rad(_camAngle.x), 0)).Xform(-Vector3.Forward) * orbitRadius;
            _cameraPos.Transform = camPosTran;

            var camPosGlobal = _cameraPos.GlobalTransform;
            camPosGlobal = _cameraPos.GlobalTransform.LookingAt(GlobalTransform.origin, Vector3.Up);

            _cameraPos.GlobalTransform = camPosGlobal;
        }
    }
}

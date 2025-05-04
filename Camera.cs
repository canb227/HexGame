using Godot;
using System;

public partial class Camera : Camera3D
{


    float zoomSpeed = 0.5f;
    float moveSpeed = 0.5f;

    float zoomOffset = 0f;
    float zoomAmount = 0f;
    float zoomMax = -3f;
    float zoomMin = 3f;

    Layout layout;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        Vector2 input = Input.GetVector("CameraMoveLeft", "CameraMoveRight", "CameraMoveUp", "CameraMoveDown");



        Vector3 input2 = new Vector3((float)input.X, 0, (float)input.Y);

        input2.Rotated(Vector3.Up, this.Rotation.Y); // Rotate the input vector based on the camera's current rotation to ensure movement is in the correct direction


        zoomOffset = this.Position.Y;
        if ( Input.IsActionJustPressed("CameraZoomIn")  )
        {
            if (zoomAmount < zoomMin)
            {
                zoomAmount -= 1f;
            }
            
        }
        else if (Input.IsActionJustPressed("CameraZoomOut"))
        {
            if (zoomAmount > zoomMax)
            {
                zoomAmount += 1f;
            }
        }

        
        //add mouse pan

        this.Position = new Vector3(this.Position.X + (float)input2.Z * moveSpeed, zoomOffset + zoomAmount, this.Position.Z + -(float)input2.X * moveSpeed);
        this.zoomAmount = Mathf.Lerp(this.zoomAmount, 0f, 0.5f);
    }

    public override void _Input(InputEvent @event)
    {
        //return;
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2 mouse_pos = GetViewport().GetMousePosition();
                Vector3 origin = this.ProjectRayOrigin(mouse_pos);
                Vector3 direction = this.ProjectRayNormal(mouse_pos);
                float distance = -origin.Y / direction.Y;
                Vector3 position = origin + direction * distance;

                GD.Print("You just clicked!");
                
                Point point = new Point(position.Z, position.X);
                GD.Print("    Mouse position: " + point.x + "," + point.y);

                FractionalHex fHex = Global.layout.PixelToHex(point);
                Hex hex = fHex.HexRound();
                GD.Print("    Hex position: " + hex.q + "," + hex.r + "," + hex.s);

            }
            else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
            {
                // Handle right mouse button click
                GD.Print("Right mouse button clicked");
            }
        }
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            // Handle mouse movement
            Vector2 mousePosition = mouseMotionEvent.Position;
            //GD.Print($"Mouse moved to position: {mousePosition}");
        }
       
    }
}

using Godot;
using System;
using System.Collections.Generic;

public partial class Camera : Camera3D
{


    float zoomSpeed = 0.5f;
    float moveSpeed = 0.5f;

    float zoomOffset = 0f;
    float zoomAmount = 0f;
    float zoomMax = -3f;
    float zoomMin = 3f;

    Layout layout;

    Game game;
    GraphicManager graphicManager;

    public bool blockClick = false;



    public void SetGame(Game game)
    {
        this.game = game;
    }

    public void SetGraphicManager(GraphicManager manager)
    {
        this.graphicManager = manager;
    }

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

    public override void _UnhandledInput(InputEvent @event)
    {
        //return;
        if (!blockClick && @event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            Vector2 mouse_pos = GetViewport().GetMousePosition();
            Vector3 origin = this.ProjectRayOrigin(mouse_pos);
            Vector3 direction = this.ProjectRayNormal(mouse_pos);
            float distance = -origin.Y / direction.Y;
            Vector3 position = origin + direction * distance;
            Point point = new Point(position.Z, position.X);
            FractionalHex fHex = Global.layout.PixelToHex(point);
            Hex hex = fHex.HexRound();

            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
               /* GD.Print("You just clicked!");
                GD.Print("    Mouse position: " + point.x + "," + point.y);
                GD.Print("    Hex position: " + hex.q + "," + hex.r + "," + hex.s);*/
                ProcessHexLeftClick(hex);

            }
            else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
            {
                /*GD.Print("Right mouse button clicked");*/
                ProcessHexRightClick(hex);
            }
        }
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            // Handle mouse movement
            Vector2 mousePosition = mouseMotionEvent.Position;
            //GD.Print($"Mouse moved to position: {mousePosition}");
        }
       
    }

    private void ProcessHexLeftClick(Hex hex)
    {
        GameHex gameHex;
        game.mainGameBoard.gameHexDict.TryGetValue(hex, out gameHex);
        if(gameHex == null)
        {
            if(graphicManager.GetWaitForTargeting())
            {
                graphicManager.ClearWaitForTarget();
            }
        }
        else if (graphicManager.GetWaitForTargeting())
        {
            if (graphicManager.waitingAbility != null)
            {
                if (graphicManager.waitingAbility.validTargetTypes.IsHexValidTarget(gameHex.gameBoard.gameHexDict[hex], graphicManager.waitingAbility.usingUnit))
                {
                    graphicManager.waitingAbility.ActivateAbility(gameHex.gameBoard.gameHexDict[hex]);
                    graphicManager.ClearWaitForTarget();
                }
            }
            else if(graphicManager.waitingCity != null)
            {
                List<Hex> hexes = graphicManager.waitingCity.ValidUrbanBuildHexes(BuildingLoader.buildingsDict[graphicManager.waitingBuildingName].TerrainTypes);
                if (hexes.Count > 0 && hexes.Contains(hex))
                {
                    graphicManager.waitingCity.BuildOnHex(hex, graphicManager.waitingBuildingName);
                    graphicManager.ClearWaitForTarget();
                }
            }
            else
            {
                graphicManager.ClearWaitForTarget();
            }
        }
        else if (gameHex.units.Count != 0)
        {
            if (graphicManager.selectedObject != graphicManager.graphicObjectDictionary[gameHex.units[0].id])
            {
                graphicManager.ChangeSelectedObject(gameHex.units[0].id, graphicManager.graphicObjectDictionary[gameHex.units[0].id]);
                return;
            }
        }
        else if (gameHex.district != null && graphicManager.selectedObject != graphicManager.graphicObjectDictionary[gameHex.district.id])
        {
            /*foreach(Building building in gameHex.district.buildings)
            {
                graphicManager.ChangeSelectedObject(building.district.id, graphicManager.graphicObjectDictionary[building.district.id]);
                return;

                if (building.buildingType == "Palace" || building.buildingType == "CityCenter")
                {
                    
                }
            }*/
        }
        
    }

    private void ProcessHexRightClick(Hex hex)
    {
        graphicManager.selectedObject.ProcessRightClick(hex);
    }
}

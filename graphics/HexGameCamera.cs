using Godot;
using NetworkMessages;
using System.Collections.Generic;
using System;
using Google.Protobuf.WellKnownTypes;

[GlobalClass]
public partial class HexGameCamera : Camera3D
{

    float zoomSpeed = 0.5f;
    float moveSpeed = 0.5f;

    float zoomOffset = 0f;
    float zoomAmount = 0f;
    float zoomMax = -3f;
    float zoomMin = 3f;

    private bool activeHexTarget = false;

    public bool blockClick = false;

    public int currentCameraQ = 0;

    private float elapsedMovementTime = 0.0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float duration;

    public override void _Process(double delta)
    {
        Vector2 input = Input.GetVector("CameraMoveLeft", "CameraMoveRight", "CameraMoveUp", "CameraMoveDown");

        if(Math.Abs(currentCameraQ - GetCameraHexQ()) > 2)
        {
            UpdateHexChunkLocations(Global.camera.GetCameraHexQ(), Global.gameManager.game.mainGameBoard.right / ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]).chunkList.Count, ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]).chunkList.Count);
            currentCameraQ = GetCameraHexQ();
        }


        Vector3 input2 = new Vector3((float)input.X, 0, (float)input.Y);

        input2.Rotated(Vector3.Up, this.Rotation.Y); // Rotate the input vector based on the camera's current rotation to ensure movement is in the correct direction

        if(activeHexTarget)
        {
            elapsedMovementTime += (float)delta;
            float time = Mathf.Clamp(elapsedMovementTime / duration, 0.0f, 1.0f);
            this.Transform = new Transform3D(Transform.Basis, startPosition.Lerp(targetPosition, time));
            if (elapsedMovementTime > duration)
            {
                activeHexTarget = false;
            }
        }
        else
        {
            if (Input.IsActionJustPressed("CameraZoomIn"))
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
        }
        zoomOffset = this.Position.Y;
        
        this.zoomAmount = Mathf.Lerp(this.zoomAmount, 0f, 0.5f);
    }

    public override void _UnhandledInput(InputEvent iEvent)
    {

        if (iEvent is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Keycode == Key.Escape) // In Godot 4, use Keycode instead of Scancode
            {
                if(Global.gameManager.graphicManager.uiManager.windowOpen)
                {
                    Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
                }
                else
                {
                    if (Global.gameManager.graphicManager.GetWaitForTargeting())
                    {
                        Global.gameManager.graphicManager.ClearWaitForTarget();
                    }
                    else
                    {
                        Global.gameManager.graphicManager.UnselectObject();
                    }
                }
            }
        }
        if (!blockClick && iEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            Vector2 mouse_pos = GetViewport().GetMousePosition();
            Vector3 origin = this.ProjectRayOrigin(mouse_pos);
            Vector3 direction = this.ProjectRayNormal(mouse_pos);
            float distance = -origin.Y / direction.Y;
            Vector3 position = origin + direction * distance;
            Point point = new Point(position.Z, position.X);
            //GD.Print(point.x + ", " + point.y);
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
        if (iEvent is InputEventMouseMotion mouseMotionEvent)
        {
            // Handle mouse movement
            Vector2 mousePosition = mouseMotionEvent.Position;
            //GD.Print($"Mouse moved to position: {mousePosition}");
        }

    }

    public void UpdateHexChunkLocations(int qCam, int wChunk, int numChunks)
    {
        int centerChunk = (numChunks / 2); // Define the middle chunk index
        int leadingChunk = ((qCam / wChunk) + centerChunk) % numChunks;
        int absoluteChunk = qCam / wChunk;
        if (leadingChunk < 0) leadingChunk += numChunks; // Ensure positive index

        List<int> orderedChunks = new List<int>();

        for (int i = 0; i < numChunks; i++)
        {
            orderedChunks.Add((leadingChunk + i) % numChunks + 1);
        }
        int j = absoluteChunk - 2;
        foreach(int i in orderedChunks)
        {
            int q = j * wChunk;
            ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]).chunkList[i-1].UpdateGraphicalOrigin(new Hex(q,0,-q));
            j++;
        }
    }

    private void ProcessHexLeftClick(Hex hex)
    {        
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        Hex wrapHex = hex.WrapHex();
        //GD.Print(wrapHex);

        GameHex gameHex;
        Global.gameManager.game.mainGameBoard.gameHexDict.TryGetValue(wrapHex, out gameHex);
        if (gameHex == null)
        {
            if (Global.gameManager.graphicManager.GetWaitForTargeting())
            {
                Global.gameManager.graphicManager.ClearWaitForTarget();
            }
        }
        else if (Global.gameManager.graphicManager.GetWaitForTargeting())
        {
            if (Global.gameManager.graphicManager.selectedObject is GraphicUnit)
            {
                if (((GraphicUnit)Global.gameManager.graphicManager.selectedObject).waitingAbility.validTargetTypes.IsHexValidTarget(Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex], Global.gameManager.game.unitDictionary[((GraphicUnit)Global.gameManager.graphicManager.selectedObject).waitingAbility.usingUnitID]))
                {
                    GraphicUnit tempUnit = ((GraphicUnit)Global.gameManager.graphicManager.selectedObject);
                    Global.gameManager.ActivateAbility(tempUnit.unit.id, tempUnit.waitingAbility.name, wrapHex); //networked command
                    Global.gameManager.graphicManager.ClearWaitForTarget();
                }
            }
            else if (Global.gameManager.graphicManager.selectedObject is GraphicCity)
            {
                GraphicCity graphicCity = ((GraphicCity)Global.gameManager.graphicManager.selectedObject);
                if (graphicCity.waitingToGrow)
                {
                    if (graphicCity.city.ValidExpandHex(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast }, Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex]))
                    {
                        graphicCity.city.ExpandToHex(wrapHex);
                        graphicCity.waitingToGrow = false;
                        Global.gameManager.graphicManager.Update2DUI(UIElement.endTurnButton);
                        Global.gameManager.graphicManager.ClearWaitForTarget();
                    }
                    else if (graphicCity.city.ValidUrbanExpandHex(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast }, Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex]))
                    {
                        graphicCity.city.DevelopDistrict(wrapHex);
                        graphicCity.waitingToGrow = false;
                        Global.gameManager.graphicManager.Update2DUI(UIElement.endTurnButton);
                        Global.gameManager.graphicManager.ClearWaitForTarget();
                    }
                }
                else if (graphicCity.waitingBuildingName != "")
                {
                    if (graphicCity.city.ValidUrbanBuildHex(BuildingLoader.buildingsDict[graphicCity.waitingBuildingName].TerrainTypes, Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex]))
                    {
                        graphicCity.city.AddBuildingToQueue(graphicCity.waitingBuildingName, Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex]);
                        graphicCity.waitingBuildingName = "";
                        Global.gameManager.graphicManager.ClearWaitForTarget();
                    }
                }
            }
            else
            {
                Global.gameManager.graphicManager.ClearWaitForTarget();
            }
        }
        else if (gameHex.units.Count != 0)
        {
            if (Global.gameManager.graphicManager.selectedObject != Global.gameManager.graphicManager.graphicObjectDictionary[gameHex.units[0]])
            {
                if (Global.gameManager.game.unitDictionary[gameHex.units[0]].teamNum == Global.gameManager.game.localPlayerTeamNum)
                {
                    Global.gameManager.graphicManager.ChangeSelectedObject(gameHex.units[0], Global.gameManager.graphicManager.graphicObjectDictionary[gameHex.units[0]]);
                }
                return;
            }
        }
        else if (gameHex.district != null && Global.gameManager.graphicManager.selectedObject != Global.gameManager.graphicManager.graphicObjectDictionary[gameHex.district.id])
        {
            /*foreach(Building building in gameHex.district.buildings)
            {
                Global.gameManager.graphicManager.ChangeSelectedObject(building.district.id, Global.gameManager.graphicManager.graphicObjectDictionary[building.district.id]);
                return;

                if (building.buildingType == "Palace" || building.buildingType == "CityCenter")
                {
                    
                }
            }*/
        }

    }

    public int GetCameraHexQ()
    {
        FractionalHex fHex = Global.layout.PixelToHex(new Point(this.Transform.Origin.Z, this.Transform.Origin.X));
        Hex hex = fHex.HexRound();
        return hex.q + (hex.r >> 1);
    }

    private void ProcessHexRightClick(Hex hex)
    {
        Hex wrapHex = hex.WrapHex();
        GameHex wrappedHex = Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex];
//        GameHex unwrappedHex = Global.gameManager.game.mainGameBoard.gameHexDict[hex];
        GD.Print("Hex: " + hex + " | Wrapped to: " + wrapHex);
        //GD.Print("Wrapped Hex Info|TerrainType: " + wrappedHex.terrainType);
       // GD.Print("Unwrapped Hex Info|TerrainType: " + unwrappedHex.terrainType);
        if (Global.gameManager.graphicManager.selectedObject != null)
        {
            Global.gameManager.graphicManager.selectedObject.ProcessRightClick(wrapHex);
        }
    }

    public void SetHexTarget(Hex hex)
    {
        startPosition = Transform.Origin;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        Hex graphicalHex = ggb.chunkList[ggb.hexToChunkDictionary[hex]].HexToGraphicalHex(hex);
        Point targetPoint = Global.layout.HexToPixel(new Hex(graphicalHex.q-1, graphicalHex.r+2, -(graphicalHex.q-1) -(graphicalHex.r+2)));
        targetPosition = new Vector3((float)targetPoint.y, 40.0f, (float)targetPoint.x); //Transform.Origin.Y
        float distance = startPosition.DistanceTo(targetPosition);
        float lerpMoveSpeed = 500.0f;
        duration = distance / lerpMoveSpeed;
        if(duration < 0.025f)
        {
            duration = 0;
        }
        else
        {
            if (duration > 0.25f)
            {
                duration = 0.25f;
            }
            elapsedMovementTime = 0;
            activeHexTarget = true;
        }

    }
}
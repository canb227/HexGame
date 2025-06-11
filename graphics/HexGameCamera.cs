using Godot;
using NetworkMessages;
using System.Collections.Generic;
using System;

[GlobalClass]
public partial class HexGameCamera : Camera3D
{

    float zoomSpeed = 0.5f;
    float moveSpeed = 0.5f;

    float zoomOffset = 0f;
    float zoomAmount = 0f;
    float zoomMax = -3f;
    float zoomMin = 3f;

    public bool blockClick = false;

    public int currentCameraQ = 0;

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


        zoomOffset = this.Position.Y;
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
        this.zoomAmount = Mathf.Lerp(this.zoomAmount, 0f, 0.5f);
    }

    public override void _UnhandledInput(InputEvent iEvent)
    {

        if (iEvent is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Keycode == Key.Escape) // In Godot 4, use Keycode instead of Scancode
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
        GD.Print(wrapHex);

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
                    ((GraphicUnit)Global.gameManager.graphicManager.selectedObject).waitingAbility.ActivateAbility(Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex]);
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
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[wrapHex];
        GD.Print("Hex: " + hex + " | Wrapped to: " + wrapHex);
        GD.Print("Hex Info|TerrainType: " + gameHex.terrainType + " TerrainTemp: " + gameHex.terrainTemp);
        if(Global.gameManager.graphicManager.selectedObject != null)
        {
            Global.gameManager.graphicManager.selectedObject.ProcessRightClick(wrapHex);
        }
    }
}
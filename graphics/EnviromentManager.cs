using Godot;
using System;
public partial class EnviromentManager: Node
{

    public DirectionalLight3D sun;
    public WorldEnvironment worldEnvironment;


    enum sunPosition
    {
        NOON,
        MORNING,
        EVENING,
        NIGHT,
        DEFAULT
    }

    public EnviromentManager()
    {
        sun = new();
        AddChild(sun);
        sun.Position = new Vector3(0, 10, 0); // Set the position of the sun
        sun.ShadowEnabled = true;
        SetSun(sunPosition.DEFAULT);
        sun.RotateY(90.0f);


        worldEnvironment = new();
        AddChild(worldEnvironment);
        worldEnvironment.Environment = new Godot.Environment();
        worldEnvironment.Environment.BackgroundMode = Godot.Environment.BGMode.Sky;
        worldEnvironment.Environment.Sky = new Godot.Sky();
        worldEnvironment.Environment.Sky.SkyMaterial = new Godot.ProceduralSkyMaterial();
        worldEnvironment.Environment.SdfgiEnabled = true;
        //worldEnvironment.Environment.VolumetricFogEnabled = true;
        //worldEnvironment.Environment.VolumetricFogDensity = 0.005f;
        
    }

    private void SetSun(sunPosition position)
    {
        switch (position)
        {
            case sunPosition.DEFAULT:
                sun.RotationDegrees = new Vector3(-60, 0, 0);
                break;
        }
    }
}
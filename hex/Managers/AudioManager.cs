using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

public partial class AudioManager : Node
{
    public AudioStreamPlayer audioPlayer = new();
    public bool audioEnabled;
    public AudioManager(bool audioEnabled = false)
    {
        this.audioEnabled = audioEnabled;
        this.Name = "AudioManager";
        AddChild(audioPlayer);
    }
    public void PlayAudio(string audioPath)
    {
        if(audioEnabled)
        {
            audioPlayer.Stream = GD.Load<AudioStream>(audioPath);
            audioPlayer.Play();
        }
    }
}

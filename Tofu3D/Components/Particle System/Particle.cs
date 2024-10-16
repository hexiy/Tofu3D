﻿namespace Tofu3D;

public class Particle
{
    public Color Color = Color.White;

    [XmlIgnore] public RendererInstancingData InstancingData = new();

    public float Lifetime = 0;
    public Vector3 Size = new(1);
    public Color SpawnColor;
    public Vector3 Velocity = new(0, 0, 0);
    public bool Visible = false;
    public Vector3 WorldPosition = new(0, 0, 0);
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandNoise : MonoBehaviour {

    [SerializeField, Range(0.001f, 0.1f)]
    float sandAmount;

    [SerializeField, Range(0f, 1f)]
    float sandOpacity;

    [SerializeField]
    Shader sandReformShader;

    private Material sandReformMaterial;
    private Terrain terrain;
    private TerrainDeformTracks playerTracks;

    // Use this for initialization
    void Start ()
    {
        terrain = GetComponent<Terrain>();
        sandReformMaterial = new Material(sandReformShader);
        playerTracks = FindObjectOfType<TerrainDeformTracks>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (playerTracks.SandMaterial != null)
        {
            sandReformMaterial.SetFloat("_SandAmount", sandAmount);
            sandReformMaterial.SetFloat("_SandOpacity", sandOpacity);
            RenderTexture sand = (RenderTexture)terrain.materialTemplate.GetTexture("_Splat");
            RenderTexture temp = RenderTexture.GetTemporary(sand.width, sand.height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(sand, temp, sandReformMaterial);
            Graphics.Blit(temp, sand);
            terrain.materialTemplate.SetTexture("_Splat", sand);
            RenderTexture.ReleaseTemporary(temp);
        }
    }
}

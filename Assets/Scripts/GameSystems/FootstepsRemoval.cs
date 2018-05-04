using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsRemoval : MonoBehaviour
{
    //    RenderTexture splatMap, temp;

    //    Vector4 coord;

    //    Material fadeMat;

    [SerializeField]
    float startTime;

    //    TerrainDeformTracks deformer;


    //    public void RemovePrint(TerrainDeformTracks deformer, RenderTexture splatMap, RenderTexture temp, Vector4 coord, Material fadeMat, float brushStrength)
    //    {
    //        startTime = Time.time;
    //        this.splatMap = splatMap;
    //        this.coord = coord;
    //        this.fadeMat = fadeMat;
    //        this.temp = temp;
    //        this.brushStrength = brushStrength;
    //        this.deformer = deformer;
    //        fadeMat.SetVector("_Coordinate", coord);
    //        fadeMat.SetFloat("_Strength", brushStrength);
    //        deformer.RemoveSteps.Add(this);
    //    }

    //    void Update()
    //    {
    //        if (Time.time < startTime + 1f)
    //        {
    //            return;
    //        }
    //        else if (Time.time < startTime + 15f)
    //        {
    //            Debug.Log("FootprintRemoved");
    //            fadeMat.SetVector("_Coordinate", coord);
    //            fadeMat.SetFloat("_Strength", brushStrength);
    //            Graphics.Blit(splatMap, temp);
    //            Graphics.Blit(splatMap, temp, fadeMat);
    //            RenderTexture.ReleaseTemporary(temp);
    //        }
    //        else
    //        {
    //            deformer.RemoveSteps.Remove(this);
    //            Destroy(this);
    //        }

    //    }

    //void update()
    //{
    //    fademat.setvector("_color", vector4.lerp(fademat.getvector("_color"), color.black, 10f));
    //    graphics.blit(splatmap, temp);
    //    graphics.blit(temp, splatmap, fademat);

    //}

    //[SerializeField]
    //Shader reverseShader;
    //[SerializeField, Range(0, 500)]
    //float brushSize;
    //[SerializeField, Range(-5, 5)]
    //float brushStrength;

    //private RenderTexture splatMap;
    //private Material sandMaterial, drawMaterial, fadeMat;
    //private RaycastHit hit;
    //RenderTexture temp;
    //Vector4 footstepCoordinate;

    void Update()
    {
        startTime -= Time.deltaTime;

        if (startTime < 1)
        {
            Destroy(this);
        }
    }
    //    drawMaterial = new Material(reverseShader);
    //    drawMaterial.SetVector("_Color", Color.black);
    //}

    //void Update()
    //{
    //    StartCoroutine("RemoveFootstep");
    //    Destroy(this);
    //}

    //IEnumerator RemoveFootstep()
    //{
    //    yield return new WaitForSeconds(4);

    //    if (Physics.Raycast(transform.position, Vector3.down, out hit))
    //    {
    //        if (hit.transform.CompareTag("Sand"))
    //        {
    //            if (sandMaterial == null)
    //            {
    //                sandMaterial = hit.transform.GetComponent<Terrain>().materialTemplate;
    //                sandMaterial.SetTexture("_Splat", splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat));
    //            }
    //            drawMaterial.SetVector("_Coordinate", transform.position);
    //            drawMaterial.SetFloat("_Strength", brushStrength);
    //            drawMaterial.SetFloat("_Size", brushSize);
    //            temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
    //            Graphics.Blit(splatMap, temp);
    //            Graphics.Blit(temp, splatMap, drawMaterial);
    //            RenderTexture.ReleaseTemporary(temp);
    //        }
    //    }
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class TerrainDeformTracks : MonoBehaviour
{
    [SerializeField]
    Shader drawShader;
    [SerializeField]
    GameObject rightFoot, leftFoot;
    [SerializeField, Range(0, 500)]
    float brushSize;
    [SerializeField, Range(0, 5)]
    float brushStrength;

    private RenderTexture splatMap;
    private Material sandMaterial, drawMaterial;
    private RaycastHit hit;
    RenderTexture temp;

    // Use this for initialization
    void Start()
    {
        drawMaterial = new Material(drawShader);
        drawMaterial.SetVector("_Color", Color.red);
    }

    void TerrainDeform(Transform foot)
    {
        if (Physics.Raycast(foot.gameObject.transform.position, Vector3.down, out hit))
        {
            if (hit.transform.CompareTag("Sand"))
            {
                if (sandMaterial == null)
                {
                    sandMaterial = hit.transform.GetComponent<Terrain>().materialTemplate;
                    sandMaterial.SetTexture("_Splat", splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat));
                }
                drawMaterial.SetVector("_Coordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
                drawMaterial.SetFloat("_Strength", brushStrength);
                drawMaterial.SetFloat("_Size", brushSize);
                temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(splatMap, temp);
                Graphics.Blit(temp, splatMap, drawMaterial);
                RenderTexture.ReleaseTemporary(temp);
            }
        }
    }

    void RightFootDeform()
    {
        TerrainDeform(rightFoot.transform);
    }

    void LeftFootDeform()
    {
        TerrainDeform(leftFoot.transform);
    }
}

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
    [SerializeField, Range(-5, 5)]
    float brushStrength;

    private RenderTexture splatMap;
    private Material sandMaterial, drawMaterial, fadeMat;
    private RaycastHit hit;
    RenderTexture temp;
    Vector4 footstepCoordinate;

    //List<FootstepsRemoval> removeSteps = new List<FootstepsRemoval>();

    //public List<FootstepsRemoval> RemoveSteps
    //{
    //    get { return this.removeSteps; }
    //}

    // Use this for initialization
    void Start()
    {
        drawMaterial = new Material(drawShader);
        //fadeMat = new Material(reverseShader);
        drawMaterial.SetVector("_Color", Color.red);
        //fadeMat.SetVector("_Color", Color.black);
    }

    //void Update()
    //{
    //    foreach(FootstepsRemoval step in removeSteps)
    //    {
    //        step.Fade();
    //    }
    //}

    void RightFootDeform()
    {
        if (Physics.Raycast(rightFoot.gameObject.transform.position, Vector3.down, out hit))
        {
            if (hit.transform.CompareTag("Sand"))
            {
                if (sandMaterial == null)
                {
                    sandMaterial = hit.transform.GetComponent<Terrain>().materialTemplate;
                    sandMaterial.SetTexture("_Splat", splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat));
                }
                drawMaterial.SetVector("_Coordinate", footstepCoordinate = new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
                drawMaterial.SetFloat("_Strength", brushStrength);
                drawMaterial.SetFloat("_Size", brushSize);
                temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(splatMap, temp);
                Graphics.Blit(temp, splatMap, drawMaterial);
                //drawMaterial.SetVector("_Color", Color.black);
                //
                //FootstepsRemoval fader = new FootstepsRemoval(this, splatMap, temp, footstepCoordinate, fadeMat, brushStrength);
                //drawMaterial.SetVector("_Color", Color.red);

                ////fadeMat.SetFloat("_Strength", drawMaterial.GetFloat("_Strength") * -0.001f);
                ////fadeMat.SetVector("_Color", Color.black);

                //FootstepsRemoval remover = Instantiate((Resources.Load("FootprintRemover") as GameObject).GetComponent<FootstepsRemoval>());
                //remover.RemovePrint(this, splatMap, temp, footstepCoordinate, fadeMat, brushStrength);

                RenderTexture.ReleaseTemporary(temp);

                //Instantiate(footstepRemover, hit.transform.position, hit.transform.rotation);
            }
        }
    }

    void LeftFootDeform()
    {
        if (Physics.Raycast(leftFoot.gameObject.transform.position, Vector3.down, out hit))
        {
            if (hit.transform.CompareTag("Sand"))
            {
                if (sandMaterial == null)
                {
                    sandMaterial = hit.transform.GetComponent<Terrain>().materialTemplate;
                    sandMaterial.SetTexture("_Splat", splatMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat));
                }
                drawMaterial.SetVector("_Coordinate", footstepCoordinate = new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
                drawMaterial.SetFloat("_Strength", brushStrength);
                drawMaterial.SetFloat("_Size", brushSize);
                temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(splatMap, temp);
                Graphics.Blit(temp, splatMap, drawMaterial);

                //drawMaterial.SetVector("_Color", Color.black);
                //Material fadeMat = new Material(drawShader);
                //FootstepsRemoval fader = new FootstepsRemoval(this, splatMap, temp, footstepCoordinate, fadeMat, brushStrength);
                //drawMaterial.SetVector("_Color", Color.red);

                ////fadeMat.SetFloat("_Strength", drawMaterial.GetFloat("_Strength") * -0.001f);
                ////fadeMat.SetVector("_Color", Color.black);

                //FootstepsRemoval remover = Instantiate((Resources.Load("FootprintRemover") as GameObject).GetComponent<FootstepsRemoval>());
                //remover.RemovePrint(this, splatMap, temp, footstepCoordinate, fadeMat, brushStrength);

                RenderTexture.ReleaseTemporary(temp);

                //Instantiate(footstepRemover, hit.transform.position, hit.transform.rotation);
            }
        }
    }

    //IEnumerator RemoveFootstep()
    //{
    //    yield return new WaitForSeconds(3);
    //    drawMaterial.SetVector("_Coordinate", footstepCoordinate);
    //    drawMaterial.SetVector("_Color", Color.black);
    //    drawMaterial.SetFloat("_Strength", brushStrength);
    //    drawMaterial.SetFloat("_Size", brushSize);
    //    temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.ARGBFloat);
    //    Graphics.Blit(splatMap, temp);
    //    Graphics.Blit(temp, splatMap, drawMaterial);
    //    drawMaterial.SetVector("_Color", Color.red);
    //}

}

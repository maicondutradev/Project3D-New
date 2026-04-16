using UnityEngine;

public class GeradorDeColisores : MonoBehaviour
{
    public float larguraTronco = 1.5f;
    public float alturaTronco = 4f;

    [ContextMenu("Gerar Colisores para NavMesh")]
    public void GerarColisores()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null) return;

        Transform colisoresAntigos = transform.Find("ColisoresDasArvores");
        if (colisoresAntigos != null)
        {
            DestroyImmediate(colisoresAntigos.gameObject);
        }

        GameObject grupoColisores = new GameObject("ColisoresDasArvores");
        grupoColisores.transform.SetParent(transform);
        grupoColisores.transform.localPosition = Vector3.zero;

        TerrainData data = terrain.terrainData;

        foreach (TreeInstance tree in data.treeInstances)
        {
            Vector3 posicaoReal = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            GameObject cilindro = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cilindro.name = "ColisorArvore";
            cilindro.transform.position = posicaoReal;
            cilindro.transform.SetParent(grupoColisores.transform);
            cilindro.transform.localScale = new Vector3(larguraTronco, alturaTronco, larguraTronco);

            MeshRenderer renderer = cilindro.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                DestroyImmediate(renderer);
            }
        }
    }
}
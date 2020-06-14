using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePieceHelperClass : MonoBehaviour
{
    public static GamePieceHelperClass gamePieceHelperClass;
    private void Awake()
    {
        gamePieceHelperClass = this;
    }
    public void ReturnPiece(GameObject pieceGameObject, Vector3 target)
    {
        StartCoroutine(ReturnPieceMain(pieceGameObject, target));
    }
    public void DeletePiece(GameObject pieceGameObject, Vector3 target)
    {
        StartCoroutine(DeletePieceMain(pieceGameObject, target));
    }
    IEnumerator ReturnPieceMain(GameObject pieceGameObject,Vector3 target)
    {
        for (int i = 0; i <= 20; i++)
        {
            pieceGameObject.transform.position = Vector3.Lerp(pieceGameObject.transform.position, target, 0.2f);
            yield return new WaitForFixedUpdate();
        }
        MeshRenderer meshRen = pieceGameObject.GetComponent<MeshRenderer>();
        for (int x = 0; x <= 30; x++)
        {
            Material m = meshRen.material;
            Color c = m.color;
            c.a = 1 - x / 30f;
            m.color = c;
            meshRen.material = m;
            yield return new WaitForFixedUpdate();
        }
        Destroy(pieceGameObject);
    }

    IEnumerator DeletePieceMain(GameObject pieceGameObject, Vector3 target)
    {
        for (int i = 0; i <= 10; i++)
        {
            pieceGameObject.transform.position = Vector3.Lerp(pieceGameObject.transform.position, target, 0.2f);
            yield return new WaitForFixedUpdate();
        }
        pieceGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        yield return new WaitForSeconds(1f);
        Destroy(pieceGameObject);
    }
}

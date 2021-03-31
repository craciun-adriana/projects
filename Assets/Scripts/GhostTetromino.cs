using UnityEngine;

/// <summary>
/// Controleaza un ghost tetramino.
/// </summary>
public class GhostTetromino : MonoBehaviour
{
    private void Start()
    {
        tag = "currentGhostTetromino";
        foreach (Transform mino in transform)
        {
            mino.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .2f);
        }
    }

    private void Update()
    {
        FollowActiveTetromino();
    }

    /// <summary>
    /// Pozitioneaza si roteste ghost tetramino in functie de piesa activa.
    /// </summary>
    private void FollowActiveTetromino()
    {
        Transform currentActivTetrominoTransform = GameObject.FindGameObjectWithTag("currentActivTetromino").transform;
        transform.position = currentActivTetrominoTransform.position;
        transform.rotation = currentActivTetrominoTransform.rotation;
        MoveDown();
    }

    /// <summary>
    /// Pozitioneaza ghost tetramino cat mai jos posibil.
    /// </summary>
    private void MoveDown()
    {
        while (CheckIsValidPosition())
        {
            transform.position -= new Vector3(0, 1, 0);
        }
        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
        }
    }

    /// <summary>
    /// Verifica daca pozitia actuala este valida.
    /// </summary>
    /// <returns>True daca pozitia este valida, false altfel.</returns>
    private bool CheckIsValidPosition()
    {
        foreach (Transform mino in transform)
        {
            Vector2 pos = FindObjectOfType<Game>().Round(mino.position);

            if (FindObjectOfType<Game>().CheckIsInsideGrid(pos) == false)
                return false;
            if (FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent.CompareTag("currentActivTetromino"))
                return true;
            if (FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent != transform)
                return false;
        }
        return true;
    }
}
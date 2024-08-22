using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoardViewer", menuName = "BoardViewer", order = 0)]
public class BoardViewer : ScriptableObject {

    public SquareSprite lightSquareSprite;
    public SquareSprite darkSquareSprite;

    [System.Serializable]
    public struct SquareSprite {
        public Color normal;
        public Color legal;
        public Color selected;
        public Color moveFromHighlight;
        public Color moveToHighlight;
    }
}

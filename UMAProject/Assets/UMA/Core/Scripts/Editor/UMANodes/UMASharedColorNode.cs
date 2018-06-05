using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
using XNode;

public class UMASharedColorNode: XNode.Node {

    [Output] public OverlayColorData Color;

    public override object GetValue(NodePort port)
    {
        return Color;
    }
}

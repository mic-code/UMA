using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using XNode;

public class UMAOverlayNode : XNode.Node {
    [Input]  public OverlayColorData SharedColor;
    [Output] public OverlayData Overlay;

    public override object GetValue(NodePort port)
    {
        return Overlay;
    }
}

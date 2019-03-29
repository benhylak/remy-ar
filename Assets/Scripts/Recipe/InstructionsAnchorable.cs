using UnityEngine;

public interface InstructionsAnchorable
{
    void AnchorInstructions(InstructionUI instructions);

    Transform GetBestAnchorPoint();
    
    void DeAnchor();
}
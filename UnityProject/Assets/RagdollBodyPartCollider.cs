using UnityEngine;
using System.Collections;

public class RagdollBodyPartCollider : RagdollBodyPart
{

    public void AddHitForce(Vector3 position, Vector3 force)
    {
        LoseStrengthAll();
        script.Hit(this);

        RagdollRigid.AddForceAtPosition(force, position);
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();
        if (UseRigidbody)
        {
            //Rigidbody
            //RagdollRigid.AddForce((AnimTrans.localPosition - RagdollTrans.localPosition) * strength, ForceMode.Impulse);

            //Quaternion relative = Quaternion.Inverse(AnimTrans.localRotation) * RagdollTrans.localRotation;

            //RagdollRigid.AddTorque(AngleDifferenz());
        }

        //RagdollTrans.localRotation = Quaternion.Lerp(RagdollTrans.localRotation, AnimTrans.localRotation, Time.fixedDeltaTime * strength);
    }

    public Vector3 AngleDifferenz()
    {
        Vector3 diff;

        Vector3 start = RagdollTrans.localRotation.eulerAngles;
        Vector3 end = AnimTrans.localRotation.eulerAngles;

        diff = end - start;
        

        return (diff/2f) * strength;
    }
}

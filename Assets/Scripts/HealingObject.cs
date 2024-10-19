using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingObject : MonoBehaviourPun, IPunObservable
{
    private Animator animator;

    PhotonView photonView;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallController : MonoBehaviour, IPunObservable
{
    private float lifeTime;
    private float maxlifeTime = 2.0f;
    private float moveSpeed = 8.0f;
    private Vector3 direction;
    private string owner;

    public string Owner {  get { return owner; } }

    public void Init(Vector2 direction, string nickname)
    {
        this.direction = direction;
        lifeTime = 0;
        owner = nickname;
    }

    private void FixedUpdate()
    {
        transform.position += direction * moveSpeed * Time.fixedDeltaTime;
        lifeTime += Time.fixedDeltaTime;

        if (lifeTime > maxlifeTime) 
        {
            DestroyObj();
        }
    }

    public void DestroyObj()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(owner);
        }
        else
        {
            owner = (string)stream.ReceiveNext();
        }
    }
}
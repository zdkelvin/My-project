using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;
using TMPro;

public enum Direction
{
    None,
    Left,
    Right
}

public enum State
{
    None,
    Idle,
    Walk,
    Jump,
    Dead
}

public class PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] private Animator animator;
    [SerializeField] private PhotonView view;
    [SerializeField] private SpriteRenderer healthBar;
    [SerializeField] private TextMesh nicknameText;
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private Transform onHeadObjects;

    private Direction faceDirection = Direction.None;
    private Direction moveDirection = Direction.None;
    private State currentState;
    private bool isJump = false;
    private bool isGrounded = false;
    private float health;
    private float maxHealth = 10;
    private string nickname;

    public string Nickname { get { return nickname; } }

    public void Init(Direction direction)
    {
        DirectionUpdate(direction);
        nickname = PhotonNetwork.NickName;
        UpdateNickname();
        health = maxHealth;
        UpdateHealth();

        if (view.IsMine)
            CameraController.Instance.Init(transform);
    }

    private void Update()
    {
        if (!view.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            view.RPC("RPC_Jump", RpcTarget.AllViaServer);

        if (Input.GetKey(KeyCode.LeftArrow))
            view.RPC("RPC_Move", RpcTarget.AllViaServer, Direction.Left);
        else if (Input.GetKey(KeyCode.RightArrow))
            view.RPC("RPC_Move", RpcTarget.AllViaServer, Direction.Right);

        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            if (!isJump && isGrounded)
                view.RPC("RPC_Idle", RpcTarget.AllViaServer, false);
        }

        if (Input.GetKeyDown(KeyCode.Space))
            view.RPC("RPC_Attack", RpcTarget.AllViaServer);
    }

    private void FixedUpdate()
    {
        Vector2 force = Vector2.zero;
        if (moveDirection == Direction.Left)
            force.x = -1.5f;
        else if (moveDirection == Direction.Right)
            force.x = 1.5f;

        if (isJump)
        {
            force.y = 35;
            StateUpdate(State.Jump);
            isGrounded = false;
            isJump = false;
        }

        rigidbody2D.AddForce(force, ForceMode2D.Impulse);
    }

    private void DirectionUpdate(Direction direction)
    {
        if (faceDirection == direction)
            return;

        faceDirection = direction;
        rigidbody2D.transform.localRotation = Quaternion.Euler(0, faceDirection == Direction.Left ? 180 : 0, 0);
        onHeadObjects.localRotation = Quaternion.Euler(0, faceDirection == Direction.Left ? 180 : 0, 0);
    }

    private void StateUpdate(State state, bool force = false)
    {
        if (currentState == state)
            return;

        currentState = state;

        switch(currentState)
        {
            case State.Idle:
                animator.Play("Idle");
                break;
            case State.Walk:
                animator.Play("Walk");
                break;
            case State.Jump:
                animator.Play("Jump");
                break;
            case State.Dead:
                animator.Play("Dead");
                StartCoroutine(Respawn());
                break;
            default:
                break;
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSecondsRealtime(1);

        rigidbody2D.transform.position = SpawnManager.Instance.GetRespawnPoint().position;
        DirectionUpdate(SpawnManager.Instance.GetFaceDirection());
        health = maxHealth;
        UpdateHealth();
        view.RPC("RPC_Idle", RpcTarget.AllViaServer, true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            if (isGrounded)
                return;

            isGrounded = true;
            StateUpdate(State.Idle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Fireball" && view.IsMine)
        {
            if (currentState != State.Dead)
            {
                health -= 1;
                OnBulletHit(collision.gameObject.GetPhotonView().ViewID);
                view.RPC("RPC_HealthUpdate", RpcTarget.AllViaServer, health, collision.GetComponent<FireBallController>().Owner);
            }
        }
    }

    private void UpdateHealth()
    {
        healthBar.size = new Vector2((health / maxHealth), 1);
    }

    public void UpdateNickname()
    {
        nicknameText.text = nickname;
    }

    public void OnBulletHit(int viewId)
    {
        view.RPC("RPC_OnBulletHit", RpcTarget.AllViaServer, viewId);
    }

    [PunRPC]
    public void RPC_Jump()
    {
        isJump = true;
        StateUpdate(State.Jump);
    }

    [PunRPC]
    public void RPC_Move(Direction direction)
    {
        moveDirection = direction;
        DirectionUpdate(direction);
        StateUpdate(State.Walk);
    }

    [PunRPC]
    public void RPC_Idle(bool force = false)
    {
        moveDirection = Direction.None;
        StateUpdate(State.Idle, force);
    }

    [PunRPC]
    public void RPC_Attack()
    {
        if (view.IsMine)
        {
            animator.Play("Attack");
            SpawnManager.Instance.SpawnFireBall(transform, faceDirection, nickname);
        }
    }

    [PunRPC]
    public void RPC_OnBulletHit(int viewId)
    {
        Destroy(PhotonView.Find(viewId).gameObject);
    }

    [PunRPC]
    public void RPC_HealthUpdate(float health, string attacker)
    {
        this.health = health;
        UpdateHealth();

        if (view.IsMine && health <= 0)
        {
            view.RPC("RPC_Dead", RpcTarget.AllViaServer, nickname, attacker);
        }
    }

    [PunRPC]
    public void RPC_Dead(string killed, string attacker)
    {
        StateUpdate(State.Dead);
        GameSceneUI.Instance.PlayerKilled(killed, attacker);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(nickname);
        }
        else
        {
            nickname = (string)stream.ReceiveNext();
            UpdateNickname();
        }
    }
}

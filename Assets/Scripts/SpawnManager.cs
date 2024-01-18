using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameObject fireBallPrefab;

    public PlayerController LocalPlayer;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        int index = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[index];
        GameObject p = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
        LocalPlayer = p.GetComponent<PlayerController>();
        LocalPlayer.Init(GetFaceDirection());
    }

    public void SpawnFireBall(Transform from, Direction direction, string owner)
    {
        Vector2 pos = new Vector2(from.position.x + (direction == Direction.Right ? 1 : -1), from.position.y - 0.1f);
        GameObject fireball = PhotonNetwork.Instantiate(fireBallPrefab.name, pos, Quaternion.identity);
        fireball.transform.localScale = direction == Direction.Right ? new Vector2(0.8f, 0.8f) : new Vector2(-0.8f, 0.8f);
        fireball.GetComponent<FireBallController>().Init(direction == Direction.Right ? Vector2.right : Vector2.left, owner);
        Physics2D.IgnoreCollision(fireball.GetComponent<Collider2D>(), LocalPlayer.GetComponent<Collider2D>());
    }

    public Transform GetRespawnPoint()
    {
        int index = Random.Range(0, spawnPoints.Count);
        return spawnPoints[index];
    }

    public Direction GetFaceDirection()
    {
        return Random.Range(0, 2) == 0 ? Direction.Right : Direction.Left;
    }
}

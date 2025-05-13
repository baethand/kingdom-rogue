using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomsPlacer : MonoBehaviour {
    
    [Header("Room Settings")]
    public Room[] RoomPrefabs;
    public Room StartingRoom;
    public Room BossRoomPrefab; // Префаб босс-комнаты
    public Grid levelGenGrid;
    public int roomsToGen;

    [Header("Positioning")]
    public float stepX = 50f;
    public float stepY = 20f;

    private Room[,] spawnedRooms;
    private bool bossRoomSpawned = false;
    private Vector2Int bossRoomPosition;

    private void Start()
    {
        spawnedRooms = new Room[11, 11];
        spawnedRooms[5, 5] = StartingRoom;

        for (int i = 0; i < roomsToGen; i++)
        {
            PlaceOneRoom();
        }
        PlaceBossRoom();
    }

    private void PlaceOneRoom()
    {
        HashSet<Vector2Int> vacantPlaces = new HashSet<Vector2Int>();
        for (int x = 0; x < spawnedRooms.GetLength(0); x++)
        {
            for (int y = 0; y < spawnedRooms.GetLength(1); y++)
            {
                if (spawnedRooms[x, y] == null) continue;

                int maxX = spawnedRooms.GetLength(0) - 1;
                int maxY = spawnedRooms.GetLength(1) - 1;

                if (x > 0 && spawnedRooms[x - 1, y] == null) vacantPlaces.Add(new Vector2Int(x - 1, y));
                if (y > 0 && spawnedRooms[x, y - 1] == null) vacantPlaces.Add(new Vector2Int(x, y - 1));
                if (x < maxX && spawnedRooms[x + 1, y] == null) vacantPlaces.Add(new Vector2Int(x + 1, y));
                if (y < maxY && spawnedRooms[x, y + 1] == null) vacantPlaces.Add(new Vector2Int(x, y + 1));
            }
        }

        // Эту строчку можно заменить на выбор комнаты с учётом её вероятности, вроде как в ChunksPlacer.GetRandomChunk()
        Room newRoom = Instantiate(RoomPrefabs[Random.Range(0, RoomPrefabs.Length)], levelGenGrid.transform);

        int limit = 500;
        while (limit-- > 0)
        {
            // Эту строчку можно заменить на выбор положения комнаты с учётом того насколько он далеко/близко от центра,
            // или сколько у него соседей, чтобы генерировать более плотные, или наоборот, растянутые данжи
            Vector2Int position = vacantPlaces.ElementAt(Random.Range(0, vacantPlaces.Count));

            if (ConnectToSomething(newRoom, position))
            {
                // Учитываем шаг stepX и stepY при размещении комнаты
                newRoom.transform.position = new Vector3((position.x - 1) * 12.5f, (position.y - 1) * 5, 0);
                spawnedRooms[position.x, position.y] = newRoom;
                
                return;
            }
        }

        Destroy(newRoom.gameObject);
    }

    private void PlaceBossRoom()
    {
        if (bossRoomSpawned || BossRoomPrefab == null) return;

        // Находим самую дальнюю комнату от стартовой
        Vector2Int farthestRoomPos = FindFarthestRoomPosition();

        // Создаем босс-комнату
        Room bossRoom = Instantiate(BossRoomPrefab, levelGenGrid.transform);
        bossRoom.transform.position = new Vector3(
            (farthestRoomPos.x - 1) * 12.5f, 
            (farthestRoomPos.y - 1) * 5, 
            0
        );

        // Подключаем босс-комнату к соседней комнате
        ConnectBossRoom(bossRoom, farthestRoomPos);
        
        spawnedRooms[farthestRoomPos.x, farthestRoomPos.y] = bossRoom;
        bossRoomPosition = farthestRoomPos;
        bossRoomSpawned = true;
    }

    private Vector2Int FindFarthestRoomPosition()
    {
        Vector2Int startPos = new Vector2Int(5, 5); // Позиция стартовой комнаты
        Vector2Int farthestPos = startPos;
        float maxDistance = 0;

        for (int x = 0; x < spawnedRooms.GetLength(0); x++)
        {
            for (int y = 0; y < spawnedRooms.GetLength(1); y++)
            {
                if (spawnedRooms[x, y] == null || (x == 5 && y == 5)) continue;

                float dist = Vector2Int.Distance(startPos, new Vector2Int(x, y));
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    farthestPos = new Vector2Int(x, y);
                }
            }
        }

        // Находим свободное место рядом с самой дальней комнатой
        return FindAdjacentEmptyPosition(farthestPos);
    }

    private Vector2Int FindAdjacentEmptyPosition(Vector2Int aroundPos)
    {
        int maxX = spawnedRooms.GetLength(0) - 1;
        int maxY = spawnedRooms.GetLength(1) - 1;

        List<Vector2Int> possiblePositions = new List<Vector2Int>();

        if (aroundPos.x > 0 && spawnedRooms[aroundPos.x - 1, aroundPos.y] == null)
            possiblePositions.Add(new Vector2Int(aroundPos.x - 1, aroundPos.y));
        if (aroundPos.y > 0 && spawnedRooms[aroundPos.x, aroundPos.y - 1] == null)
            possiblePositions.Add(new Vector2Int(aroundPos.x, aroundPos.y - 1));
        if (aroundPos.x < maxX && spawnedRooms[aroundPos.x + 1, aroundPos.y] == null)
            possiblePositions.Add(new Vector2Int(aroundPos.x + 1, aroundPos.y));
        if (aroundPos.y < maxY && spawnedRooms[aroundPos.x, aroundPos.y + 1] == null)
            possiblePositions.Add(new Vector2Int(aroundPos.x, aroundPos.y + 1));

        return possiblePositions.Count > 0 ? 
            possiblePositions[Random.Range(0, possiblePositions.Count)] : 
            aroundPos;
    }

    private void ConnectBossRoom(Room bossRoom, Vector2Int bossPos)
    {
        int maxX = spawnedRooms.GetLength(0) - 1;
        int maxY = spawnedRooms.GetLength(1) - 1;

        // Проверяем соседей и подключаемся к существующей комнате
        if (bossPos.x > 0 && spawnedRooms[bossPos.x - 1, bossPos.y] != null)
        {
            bossRoom.DoorL.SetActive(false);
            spawnedRooms[bossPos.x - 1, bossPos.y].DoorR.SetActive(false);
        }
        else if (bossPos.y > 0 && spawnedRooms[bossPos.x, bossPos.y - 1] != null)
        {
            bossRoom.DoorD.SetActive(false);
            spawnedRooms[bossPos.x, bossPos.y - 1].DoorU.SetActive(false);
        }
        else if (bossPos.x < maxX && spawnedRooms[bossPos.x + 1, bossPos.y] != null)
        {
            bossRoom.DoorR.SetActive(false);
            spawnedRooms[bossPos.x + 1, bossPos.y].DoorL.SetActive(false);
        }
        else if (bossPos.y < maxY && spawnedRooms[bossPos.x, bossPos.y + 1] != null)
        {
            bossRoom.DoorU.SetActive(false);
            spawnedRooms[bossPos.x, bossPos.y + 1].DoorD.SetActive(false);
        }
    }

    private bool ConnectToSomething(Room room, Vector2Int p)
    {
        int maxX = spawnedRooms.GetLength(0) - 1;
        int maxY = spawnedRooms.GetLength(1) - 1;

        List<Vector2Int> neighbours = new List<Vector2Int>();

        if (room.DoorU != null && p.y < maxY && spawnedRooms[p.x, p.y + 1]?.DoorD != null) neighbours.Add(Vector2Int.up);
        if (room.DoorD != null && p.y > 0 && spawnedRooms[p.x, p.y - 1]?.DoorU != null) neighbours.Add(Vector2Int.down);
        if (room.DoorR != null && p.x < maxX && spawnedRooms[p.x + 1, p.y]?.DoorL != null) neighbours.Add(Vector2Int.right);
        if (room.DoorL != null && p.x > 0 && spawnedRooms[p.x - 1, p.y]?.DoorR != null) neighbours.Add(Vector2Int.left);

        if (neighbours.Count == 0) return false;

        Vector2Int selectedDirection = neighbours[Random.Range(0, neighbours.Count)];
        Room selectedRoom = spawnedRooms[p.x + selectedDirection.x, p.y + selectedDirection.y];

        if (selectedDirection == Vector2Int.up)
        {
            room.DoorU.SetActive(false);
            selectedRoom.DoorD.SetActive(false);
        }
        else if (selectedDirection == Vector2Int.down)
        {
            room.DoorD.SetActive(false);
            selectedRoom.DoorU.SetActive(false);
        }
        else if (selectedDirection == Vector2Int.right)
        {
            room.DoorR.SetActive(false);
            selectedRoom.DoorL.SetActive(false);
        }
        else if (selectedDirection == Vector2Int.left)
        {
            room.DoorL.SetActive(false);
            selectedRoom.DoorR.SetActive(false);
        }
        
        return true;
    }
}
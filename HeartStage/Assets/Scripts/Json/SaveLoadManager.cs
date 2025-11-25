using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SaveDataVC = SaveDataV1;

public class SaveLoadManager
{
    public static int SaveDataVersion { get; } = 1;

    public static SaveDataVC Data { get; set; } = new SaveDataVC();

    static SaveLoadManager()
    {
        Load();
    }

    private static readonly string[] SaveFilename =
    {
        "SaveAuto.json",
        "Save1.json",
        "Save2.json",
        "Save3.json",
    };

    public static string SaveDirectory => $"{Application.persistentDataPath}/Save";

    private static JsonSerializerSettings settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All
    };

    public static bool Save(int slot = 0)
    {
        if (Data == null || slot < 0 || slot > SaveFilename.Length)
            return false;

        try
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            var path = Path.Combine(SaveDirectory, SaveFilename[slot]);
            var json = JsonConvert.SerializeObject(Data, settings);
            File.WriteAllText(path, json);

            return true;
        }
        catch
        {
            Debug.Log("Save 예외 발생");
            return false;
        }
    }
    public static bool Load(int slot = 0)
    {
        if (slot < 0 || slot > SaveFilename.Length)
            return false;

        var path = Path.Combine(SaveDirectory, SaveFilename[slot]);
        if (!File.Exists(path))
            return false;

        try
        {
            var json = File.ReadAllText(path);
            var dataSave = JsonConvert.DeserializeObject<SaveData>(json, settings);
            while (dataSave.Version < SaveDataVersion)
            {
                dataSave = dataSave.VersionUp();
            }
            Data = dataSave as SaveDataVC;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Load 예외 발생: " + e);
            return false;
        }
    }

    //캐릭터 획득 처리
    public static void AcquireCharacter(int baseId, CharacterTable charTable)
    {
        var row = charTable.Get(baseId);
        if (row == null)
            return;

        string name = row.char_name;

        // 1) 도감/해금 true
        Data.unlockedByName[name] = true;

        // 2) 보유 id 등록
        Data.ownedIds.Add(baseId);

        // 3) exp 초기화
        if (!Data.expById.ContainsKey(baseId))
            Data.expById[baseId] = 0;

        Save(); // 원하면 즉시 저장
    }

    public static void ReplaceOwnedId(int currentId, int nextId, int remainExp)
    {
        //레벨 업 후 or 랭크 업 후 호출
        // id 교체 및 경험치 갱신
        int idx = Data.ownedIds.IndexOf(currentId);
        if (idx < 0)
            return;

        Data.ownedIds[idx] = nextId;

        Data.expById.Remove(currentId);
        Data.expById[nextId] = remainExp;
    }
    public static void CommitUpgradeResult(int startId, int finalId, int remainExp)
    {
        //레벨 업/랭크 업 결과 확정 처리
        if (finalId != startId)
        {
            ReplaceOwnedId(startId, finalId, remainExp);
        }
        else
        {
            // 레벨업/랭크업 안 됐으면 exp만 업데이트
            Data.expById[startId] = remainExp;
        }

        Save(); // 최종 1회 저장
    }

}
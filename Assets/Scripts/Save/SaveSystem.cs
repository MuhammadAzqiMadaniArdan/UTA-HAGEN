using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private string savePath;
    
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }
    
    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            greedLevel = GameManager.Instance.greedLevel,
            suspicionLevel = GameManager.Instance.suspicionLevel,
            currentStoryProgress = GameManager.Instance.currentStoryProgress,
            unlockedEndings = GetUnlockedEndings()
        };
        
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        
        Debug.Log("Game saved successfully!");
    }
    
    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            
            GameManager.Instance.greedLevel = saveData.greedLevel;
            GameManager.Instance.suspicionLevel = saveData.suspicionLevel;
            GameManager.Instance.currentStoryProgress = saveData.currentStoryProgress;
            
            SetUnlockedEndings(saveData.unlockedEndings);
            
            Debug.Log("Game loaded successfully!");
        }
        else
        {
            Debug.Log("No save file found.");
        }
    }
    
    private string[] GetUnlockedEndings()
    {
        // Get unlocked endings from PlayerPrefs
        System.Collections.Generic.List<string> unlockedEndings = new System.Collections.Generic.List<string>();
        
        foreach (EndingType endingType in System.Enum.GetValues(typeof(EndingType)))
        {
            if (endingType != EndingType.None && PlayerPrefs.GetInt($"Ending_{endingType}", 0) == 1)
            {
                unlockedEndings.Add(endingType.ToString());
            }
        }
        
        return unlockedEndings.ToArray();
    }
    
    private void SetUnlockedEndings(string[] unlockedEndings)
    {
        foreach (string endingName in unlockedEndings)
        {
            PlayerPrefs.SetInt($"Ending_{endingName}", 1);
        }
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class SaveData
{
    public float greedLevel;
    public float suspicionLevel;
    public int currentStoryProgress;
    public string[] unlockedEndings;
}

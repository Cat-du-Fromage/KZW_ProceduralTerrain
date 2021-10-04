using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class FolderStructure
{
    const string DefaultSaveName = "DefaultSaveName";
    const string GameSaves = "SavedGame";
    
    readonly string selectedSaveName;

    readonly string gameSavesPath;
    readonly string selectedSavePath;
    public FolderStructure(string selectedSaveName)
    {
        this.selectedSaveName = selectedSaveName == String.Empty ? DefaultSaveName : selectedSaveName;
        
        gameSavesPath = Path.Combine(Application.persistentDataPath, GameSaves);
        selectedSavePath = Path.Combine(Application.persistentDataPath, GetGameSavesPath(), this.selectedSaveName);
    }
    
    public string GetSaveName() => selectedSaveName;

    public string GetGameSavesPath()
    {
        if (!Directory.Exists(gameSavesPath))
            Directory.CreateDirectory(gameSavesPath);
        return gameSavesPath;
    }

    public string GetSelectedSavePath()
    {
        if (!Directory.Exists(selectedSavePath))
            Directory.CreateDirectory(selectedSavePath);
        return selectedSavePath;
    }

    public bool SelectedSaveExists() => Directory.Exists(selectedSavePath);

    public void CreateNewSave()
    {
        string directoryName = selectedSavePath;
        for (int count = 0; Directory.Exists(directoryName); count++)
        {
            directoryName  = Path.Combine(Application.persistentDataPath, GetGameSavesPath(), $"{selectedSaveName}({count})");
        }
        Directory.CreateDirectory(directoryName);
    }
}

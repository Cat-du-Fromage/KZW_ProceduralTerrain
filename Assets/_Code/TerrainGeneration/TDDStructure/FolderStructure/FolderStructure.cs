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
        
        gameSavesPath = CreateGameSaves();
        selectedSavePath = CreateNewSave();
    }
    
    string CreateGameSaves() => Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, GameSaves)).FullName;
    
    public string GetSaveName() => selectedSaveName;

    public string GetGameSavesPath() => gameSavesPath;
    /*
    {
        if (!Directory.Exists(gameSavesPath))
            Directory.CreateDirectory(gameSavesPath);
        return gameSavesPath;
    }
*/
    public string GetSelectedSavePath()
    {
        if (!Directory.Exists(selectedSavePath))
            Directory.CreateDirectory(selectedSavePath);
        return selectedSavePath;
    }

    public bool SelectedSaveExists() => Directory.Exists(selectedSavePath);

    public string CreateNewSave()
    {
        string directoryName = selectedSavePath ?? Path.Combine(gameSavesPath, selectedSaveName);
        for (int count = 0; Directory.Exists(directoryName); count++)
        {
            directoryName  = Path.Combine(Application.persistentDataPath, GetGameSavesPath(), $"{selectedSaveName}({count})");
        }
        return Directory.CreateDirectory(directoryName).FullName;
    }
}

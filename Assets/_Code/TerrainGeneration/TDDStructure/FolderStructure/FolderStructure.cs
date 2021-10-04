using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class FolderStructure
{
    const string DefaultSaveName = "DefaultSaveName";
    const string SavedGameRoot = "SavedGame";

    readonly string savedGamePath;
    readonly string selectedSavePath;
    
    string selectedSaveName;

    public FolderStructure(string selectedSaveName)
    {
        this.selectedSaveName = selectedSaveName == String.Empty ? DefaultSaveName : selectedSaveName;
        
        savedGamePath = Path.Combine(Application.persistentDataPath, SavedGameRoot);
        selectedSavePath = Path.Combine(Application.persistentDataPath, GetSavesRoot(), this.selectedSaveName);
    }
    public string GetSaveName() => selectedSaveName;

    public string GetSavesRoot()
    {
        if (!Directory.Exists(savedGamePath))
            Directory.CreateDirectory(savedGamePath);
        return savedGamePath;
    }

    public string GetSaveNamePath()
    {
        if (!Directory.Exists(selectedSavePath))
            Directory.CreateDirectory(selectedSavePath);
        return selectedSavePath;
    }

    public bool CheckSaveName() => Directory.Exists(selectedSavePath);
}

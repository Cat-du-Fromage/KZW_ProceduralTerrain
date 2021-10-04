using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UAssert = UnityEngine.Assertions.Assert;

[TestFixture]
public class SaveFolderStructureTest
{
    FolderStructure folderStructure;
    [SetUp]
    public void SetUpTest()
    {
        folderStructure = new FolderStructure("");
        Directory.Delete(folderStructure.GetGameSavesPath(), true);
    }
    // A Test behaves as an ordinary method
    [Test, Order(0)]
    public void FolderStruct_CallStructure_Exist()
    {
        //Arrange
        
        //Act

        //Assert
        Assert.IsInstanceOf<FolderStructure>(folderStructure);
    }
    
    [Test, Order(1)]
    public void FolderStruct_GetSaveName_NotEmpty()
    {
        //Arrange
        
        //Act
        string saveName = folderStructure.GetSaveName();
        //Assert
        Assert.IsNotEmpty(saveName);
    }
    
    [Test, Order(2)]
    public void FolderStruct_GetDirectorySavedGame_True()
    {
        //Arrange
        
        //Act
        string savesRoot = folderStructure.GetGameSavesPath();
        //Assert
        Assert.IsTrue(Directory.Exists(savesRoot));
    }
    
    [Test, Order(3)]
    public void FolderStruct_GetDirectorySelectedSave_True()
    {
        //Arrange
        
        //Act
        string saveNamePath = folderStructure.GetSelectedSavePath();
        //Assert
        Assert.IsTrue(Directory.Exists(saveNamePath));
    }
    
    [Test, Order(4)]
    public void FolderStruct_SelectedSaveNameAlreadyExist_True()
    {
        //Arrange
        //Directory.Delete(folderStructure.GetGameSavesPath(), true);
        folderStructure.GetSelectedSavePath();
        //Act
        bool exist = folderStructure.SelectedSaveExists();
        //Assert
        Assert.IsTrue(exist);
    }
    
    [Test, Order(5)]
    public void FolderStruct_CreateDirectoryWhenNameAlreadyUse_True()
    {
        //Arrange
        //Directory.Delete(folderStructure.GetSavesRoot(), true);
        folderStructure.CreateNewSave();
        int numDirectoryAtStart = Directory.GetDirectories(folderStructure.GetGameSavesPath()).Length;
        //Act
        folderStructure.CreateNewSave();
        //Assert
        Assert.AreEqual(numDirectoryAtStart + 1, Directory.GetDirectories(folderStructure.GetGameSavesPath()).Length);
    }
    
    [Test, Order(6)]
    public void FolderStruct_Create3DirectoryWhenNameAlreadyUse_True()
    {
        //Arrange
        //Directory.Delete(folderStructure.GetGameSavesPath(), true);
        folderStructure.CreateNewSave();
        int numDirectoryAtStart = Directory.GetDirectories(folderStructure.GetGameSavesPath()).Length;
        int numDirectoryToCreate = 3;
        //Act
        for (int i = 0; i < numDirectoryToCreate; i++)
        {
            folderStructure.CreateNewSave();
        }
        //Assert
        Assert.AreEqual(numDirectoryAtStart + numDirectoryToCreate, Directory.GetDirectories(folderStructure.GetGameSavesPath()).Length);
    }
    
}

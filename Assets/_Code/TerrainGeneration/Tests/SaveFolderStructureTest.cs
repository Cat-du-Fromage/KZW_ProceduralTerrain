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
    [SetUp]
    public void SetUpTest()
    {
        
    }
    // A Test behaves as an ordinary method
    [Test]
    public void FolderStruct_CallStructure_Exist()
    {
        //Arrange
        FolderStructure folderStructure = new FolderStructure("");
        //Act

        //Assert
        Assert.IsInstanceOf<FolderStructure>(folderStructure);
    }
    
    [Test]
    public void FolderStruct_GetSaveName_NotEmpty()
    {
        //Arrange
        FolderStructure folderStructure = new FolderStructure("");
        //Act
        string saveName = folderStructure.GetSaveName();
        //Assert
        Assert.IsNotEmpty(saveName);
    }
    
    [Test]
    public void FolderStruct_GetDirectoryContainingSaveFiles_EXIST()
    {
        //Arrange
        FolderStructure folderStructure = new FolderStructure("");
        //Act
        string savesRoot = folderStructure.GetSavesRoot();
        //Assert
        Assert.IsTrue(Directory.Exists(savesRoot));
    }
    
    [Test]
    public void FolderStruct_GetDirectorySaveName_EXIST()
    {
        //Arrange
        FolderStructure folderStructure = new FolderStructure("");
        //Act
        string saveNamePath = folderStructure.GetSaveNamePath();
        //Assert
        Assert.IsTrue(Directory.Exists(saveNamePath));
    }
    
    [Test]
    public void FolderStruct_CheckIfSaveNameAlreadyExist_EXIST()
    {
        //Arrange
        FolderStructure folderStructure = new FolderStructure("");
        //Act
        bool exist = folderStructure.CheckSaveName();
        //Assert
        Assert.IsTrue(exist);
    }
    
}

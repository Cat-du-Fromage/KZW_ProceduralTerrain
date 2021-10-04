using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;
using KaizerWaldCode.TerrainGeneration;
using AssertUnity = UnityEngine.Assertions.Assert;

namespace KaizerWaldCode.TerrainGeneration
{
    [TestFixture]
    public class MapSettingsTest
    {
        MapSettings mapSettings;
        
        [SetUp]
        public void SetUpTest()
        {
            //mapSettings = ScriptableObject.CreateInstance<MapSettingsSO>();
        }
        
        [Test]
        public void MapSettings_CheckSaveNameNoEmpty_GetDefaultValue()
        {
            //Arrange
            
            //Act
            mapSettings = new MapSettings(5,2,2, "");
            //Assert
            Assert.IsNotEmpty(mapSettings.SaveName);
        }
        
        [Test]
        public void MapSettings_ChangeSaveName_NotDefaultName()
        {
            //Arrange
            string defaultSaveName = "DefaultSaveName";
            //Act
            mapSettings = new MapSettings(5,2,2, "SaveTest");
            //Assert
            Assert.AreNotEqual(defaultSaveName,mapSettings.SaveName);
        }

        [Test]
        public void MapSettings_CheckNameValue_Value()
        {
            //Arrange
            
        }
    }
}


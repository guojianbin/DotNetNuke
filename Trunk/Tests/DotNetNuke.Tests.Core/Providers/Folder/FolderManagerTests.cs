﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using MbUnit.Framework;

using Moq;

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class FolderManagerTests
    {
        #region Private Variables

        private FolderManager _folderManager;
        private Mock<FolderProvider> _mockFolder;
        private Mock<DataProvider> _mockData;
        private Mock<FolderManager> _mockFolderManager;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFolderMappingController> _folderMappingController;
        private Mock<IDirectory> _directory;
        private Mock<ICBO> _cbo;
        private Mock<IPathUtils> _pathUtils;

        #endregion

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
            _mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            _mockData = MockComponentProvider.CreateDataProvider();

            _folderMappingController = new Mock<IFolderMappingController>();
            _directory = new Mock<IDirectory>();
            _cbo = new Mock<ICBO>();
            _pathUtils = new Mock<IPathUtils>();

            FolderMappingController.RegisterInstance(_folderMappingController.Object);
            DirectoryWrapper.RegisterInstance(_directory.Object);
            CBOWrapper.RegisterInstance(_cbo.Object);
            PathUtils.RegisterInstance(_pathUtils.Object);

            _mockFolderManager = new Mock<FolderManager>() { CallBase = true };

            _folderManager = new FolderManager();

            _folderInfo = new Mock<IFolderInfo>();
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        #endregion

        #region AddFolder

        [Test]
        [ExpectedArgumentException]
        public void AddFolder_Throws_On_Null_FolderPath()
        {
            _folderManager.AddFolder(It.IsAny<FolderMappingInfo>(), null);
        }

        [Test]
        public void AddFolder_Calls_FolderProvider_AddFolder()
        {
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var folderMapping = new FolderMappingInfo
                                    {
                                        FolderMappingID = Constants.FOLDER_ValidStorageLocation,
                                        FolderProviderType = Constants.FOLDER_ValidFolderProviderType,
                                        PortalID = Constants.CONTENT_ValidPortalId
                                    };

            _mockFolder.Setup(mf => mf.AddFolder(Constants.FOLDER_ValidSubFolderRelativePath, folderMapping)).Verifiable();

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(Constants.FOLDER_ValidSubFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidSubFolderPath));
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath, Constants.FOLDER_ValidStorageLocation));

            _mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath);

            _mockFolder.Verify();
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void AddFolder_Throws_When_FolderProvider_Throws()
        {
            var folderMapping = new FolderMappingInfo
                                    {
                                        FolderMappingID = Constants.FOLDER_ValidStorageLocation,
                                        FolderProviderType = Constants.FOLDER_ValidFolderProviderType
                                    };

            _mockFolder.Setup(mf => mf.AddFolder(Constants.FOLDER_ValidSubFolderRelativePath, folderMapping)).Throws<Exception>();

            _mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath);
        }

        [Test]
        public void AddFolder_Calls_FolderManager_CreateFolderInFileSystem_And_CreateFolderInDatabase()
        {
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var folderMapping = new FolderMappingInfo
                                    {
                                        FolderMappingID = Constants.FOLDER_ValidStorageLocation,
                                        FolderProviderType = Constants.FOLDER_ValidFolderProviderType,
                                        PortalID = Constants.CONTENT_ValidPortalId
                                    };

            _mockFolder.Setup(mf => mf.AddFolder(Constants.FOLDER_ValidSubFolderRelativePath, folderMapping));

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(Constants.FOLDER_ValidSubFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidSubFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath, Constants.FOLDER_ValidStorageLocation)).Verifiable();

            _mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath);

            _mockFolderManager.Verify();
        }

        #endregion

        #region DeleteFolder

        [Test]
        [ExpectedArgumentException]
        public void DeleteFolder_Throws_On_Null_Folder()
        {
            _folderManager.DeleteFolder(null);
        }

        [Test]
        public void DeleteFolder_Calls_FolderProvider_DeleteFolder()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.DeleteFolder(_folderInfo.Object)).Verifiable();

            _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));

            _mockFolderManager.Object.DeleteFolder(_folderInfo.Object);

            _mockFolder.Verify();
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void DeleteFolder_Throws_When_FolderProvider_Throws()
        {
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.DeleteFolder(_folderInfo.Object)).Throws<Exception>();

            _mockFolderManager.Object.DeleteFolder(_folderInfo.Object);
        }

        [Test]
        public void DeleteFolder_Calls_Directory_Delete()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.DeleteFolder(_folderInfo.Object));

            _directory.Setup(d => d.Delete(Constants.FOLDER_ValidFolderPath, false)).Verifiable();

            _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));

            _mockFolderManager.Object.DeleteFolder(_folderInfo.Object);

            _directory.Verify();
        }

        [Test]
        public void DeleteFolder_Calls_FolderManager_DeleteFolder_Overload()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.DeleteFolder(_folderInfo.Object));

            _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

            _mockFolderManager.Object.DeleteFolder(_folderInfo.Object);

            _mockFolderManager.Verify();
        }

        #endregion

        #region FolderExists

        [Test]
        [ExpectedArgumentException]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
            _folderManager.FolderExists(Constants.CONTENT_ValidPortalId, null);
        }

        [Test]
        public void ExistsFolder_Calls_FolderManager_GetFolder()
        {
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object).Verifiable();

            _mockFolderManager.Object.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            _mockFolderManager.Verify();
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);

            var result = _mockFolderManager.Object.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns<IFolderInfo>(null);

            var result = _mockFolderManager.Object.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.IsFalse(result);
        }

        #endregion

        #region GetFiles

        [Test]
        [ExpectedArgumentException]
        public void GetFilesByFolder_Throws_On_Null_Folder()
        {
            _folderManager.GetFiles(null);
        }

        [Test]
        public void GetFilesByFolder_Calls_DataProvider_GetFiles()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var files = new DataTable();
            files.Columns.Add("FolderName");

            var dr = files.CreateDataReader();

            _mockData.Setup(md => md.GetFiles(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderId)).Returns(dr).Verifiable();

            var filesList = new List<FileInfo> { new FileInfo() { FileName = Constants.FOLDER_ValidFileName } };

            _cbo.Setup(cbo => cbo.FillCollection<FileInfo>(dr)).Returns(filesList);

            _folderManager.GetFiles(_folderInfo.Object);

            _mockData.Verify();
        }

        [Test]
        public void GetFilesByFolder_Count_Equals_DataProvider_GetFiles_Count()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var files = new DataTable();
            files.Columns.Add("FileName");
            files.Rows.Add(Constants.FOLDER_ValidFileName);

            var dr = files.CreateDataReader();

            _mockData.Setup(md => md.GetFiles(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderId)).Returns(dr);

            var filesList = new List<FileInfo>();
            filesList.Add(new FileInfo() { FileName = Constants.FOLDER_ValidFileName });

            _cbo.Setup(cbo => cbo.FillCollection<FileInfo>(dr)).Returns(filesList);

            var result = _folderManager.GetFiles(_folderInfo.Object);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetFilesByFolder_Returns_Valid_FileNames_When_Folder_Contains_Files()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var files = new DataTable();
            files.Columns.Add("FileName");
            files.Rows.Add(Constants.FOLDER_ValidFileName);
            files.Rows.Add(Constants.FOLDER_OtherValidFileName);

            var dr = files.CreateDataReader();

            _mockData.Setup(md => md.GetFiles(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderId)).Returns(dr);

            var filesList = new List<FileInfo>
                                {
                                    new FileInfo { FileName = Constants.FOLDER_ValidFileName },
                                    new FileInfo { FileName = Constants.FOLDER_OtherValidFileName }
                                };

            _cbo.Setup(cbo => cbo.FillCollection<FileInfo>(dr)).Returns(filesList);

            var result = _folderManager.GetFiles(_folderInfo.Object).Cast<FileInfo>();

            Assert.AreElementsEqual(filesList, result);
        }

        #endregion

        #region GetFolder

        [Test]
        public void GetFolder_Calls_DataProvider_GetFolder()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            _mockData.Setup(md => md.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(folderDataTable.CreateDataReader()).Verifiable();

            _folderManager.GetFolder(Constants.FOLDER_ValidFolderId);

            _mockData.Verify();
        }

        [Test]
        public void GetFolder_Returns_Null_When_Folder_Does_Not_Exist()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            var dr = folderDataTable.CreateDataReader();

            _mockData.Setup(md => md.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(dr);
            _cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns<FolderInfo>(null);

            var result = _folderManager.GetFolder(Constants.FOLDER_ValidFolderId);

            Assert.IsNull(result);
        }

        [Test]
        public void GetFolder_Returns_Valid_Folder_When_Folder_Exists()
        {
            _folderInfo.Setup(fi => fi.FolderName).Returns(Constants.FOLDER_ValidFolderName);

            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");
            folderDataTable.Rows.Add(Constants.FOLDER_ValidFolderName);

            var dr = folderDataTable.CreateDataReader();

            _mockData.Setup(md => md.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(dr);

            var folderInfo = new FolderInfo();
            folderInfo.FolderPath = Constants.FOLDER_ValidFolderRelativePath;

            _cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns(folderInfo);

            var result = _mockFolderManager.Object.GetFolder(Constants.FOLDER_ValidFolderId);

            Assert.AreEqual(Constants.FOLDER_ValidFolderName, result.FolderName);
        }

        [Test]
        [ExpectedArgumentException]
        public void GetFolder_Throws_On_Null_FolderPath()
        {
            _folderManager.GetFolder(It.IsAny<int>(), null);
        }

        [Test]
        public void GetFolder_Calls_DataProvider_GetFolder_When_IgnoreCache_Is_True()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            var dr = folderDataTable.CreateDataReader();

            _mockData.Setup(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(dr).Verifiable();

            _cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns(It.IsAny<FolderInfo>());

            _mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            _mockData.Verify();
        }

        [Test]
        public void GetFolder_Calls_GetFoldersSorted_When_IgnoreCache_Is_False()
        {
            var foldersSorted = new List<IFolderInfo>();

            _mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(foldersSorted).Verifiable();

            _mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            _mockFolderManager.Verify();
        }

        [Test]
        public void GetFolder_Calls_DataProvider_GetFolder_When_Folder_Is_Not_In_Cache()
        {
            var foldersSorted = new List<IFolderInfo>();

            _mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(foldersSorted);

            _mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            _mockData.Verify(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath), Times.Once());
        }

        [Test]
        public void GetFolder_Returns_Null_When_Folder_Does_Not_Exist_Overload()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            var dr = folderDataTable.CreateDataReader();

            _mockData.Setup(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(dr);

            _cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns<FolderInfo>(null);

            var result = _mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.IsNull(result);
        }

        [Test]
        public void GetFolder_Returns_Valid_Folder_When_Folder_Exists_Overload()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");
            folderDataTable.Rows.Add(Constants.FOLDER_ValidFolderName);

            var dr = folderDataTable.CreateDataReader();

            _mockData.Setup(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(dr);

            var folderInfo = new FolderInfo { FolderPath = Constants.FOLDER_ValidFolderRelativePath };

            _cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns(folderInfo);

            var result = _mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.AreEqual(Constants.FOLDER_ValidFolderName, result.FolderName);
        }

        #endregion

        #region GetFolders

        [Test]
        [ExpectedArgumentException]
        public void GetFoldersByParentFolder_Throws_On_Null_ParentFolder()
        {
            _folderManager.GetFolders((IFolderInfo)null);
        }

        [Test]
        public void GetFoldersByParentFolder_Returns_Empty_List_When_ParentFolder_Contains_No_Subfolders()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            _mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(new List<IFolderInfo>());

            var result = _mockFolderManager.Object.GetFolders(_folderInfo.Object);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetFoldersByParentFolder_Returns_Valid_Subfolders()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var foldersSorted = new List<IFolderInfo>
                                    {
                                        new FolderInfo { FolderPath = Constants.FOLDER_ValidFolderRelativePath} ,
                                        new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath }
                                    };

            _mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(foldersSorted);

            var result = _mockFolderManager.Object.GetFolders(_folderInfo.Object);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(Constants.FOLDER_ValidSubFolderRelativePath, result[0].FolderPath);
        }

        #endregion

        #region GetFolders

        [Test]
        public void GetFoldersSorted_Calls_CBO_GetCachedObject()
        {
            var folders = new SortedList<string, FolderInfo>();

            _cbo.Setup(cbo => cbo.GetCachedObject<SortedList<string, FolderInfo>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>())).Returns(folders).Verifiable();

            _mockFolderManager.Object.GetFolders(Constants.CONTENT_ValidPortalId);

            _cbo.Verify();
        }

        #endregion

        #region RenameFolder

        [Test]
        [ExpectedArgumentException]
        public void RenameFolder_Throws_On_Null_Folder()
        {
            _folderManager.RenameFolder(null, It.IsAny<string>());
        }

        [Test]
        [Row(null)]
        [Row("")]
        [ExpectedArgumentException]
        public void RenameFolder_Throws_On_Null_Or_Empty_NewFolderName(string newFolderName)
        {
            _folderManager.RenameFolder(_folderInfo.Object, newFolderName);
        }

        [Test]
        public void RenameFolder_Calls_FolderProvider_RenameFolder_When_NewFolderName_Is_Different_From_FolderName()
        {
            var folderInfo = new FolderInfo();
            folderInfo.FolderPath = Constants.FOLDER_ValidFolderRelativePath;
            folderInfo.StorageLocation = Constants.FOLDER_ValidStorageLocation;

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMapping);

            _mockFolderManager.Setup(mfm => mfm.RenameSubfolders(folderInfo, Constants.FOLDER_OtherValidFolderName));

            _mockFolderManager.Object.RenameFolder(folderInfo, Constants.FOLDER_OtherValidFolderName);

            _mockFolder.Verify(mf => mf.RenameFolder(folderInfo, Constants.FOLDER_OtherValidFolderName), Times.Once());
        }

        [Test]
        public void RenameFolder_Does_Not_Call_FolderProvider_RenameFolder_When_NewFolderName_Equals_FolderName()
        {
            var folderInfo = new FolderInfo();
            folderInfo.FolderPath = Constants.FOLDER_ValidFolderRelativePath;

            _folderManager.RenameFolder(folderInfo, Constants.FOLDER_ValidFolderName);

            _mockFolder.Verify(mf => mf.RenameFolder(folderInfo, Constants.FOLDER_ValidFolderName), Times.Never());
        }

        #endregion

        #region UpdateFolder

        [Test]
        [ExpectedArgumentException]
        public void UpdateFolder_Throws_On_Null_Folder()
        {
            _folderManager.UpdateFolder(null);
        }

        [Test]
        public void UpdateFolder_Calls_DataProvider_UpdateFolder()
        {
            _mockFolderManager.Setup(mfm => mfm.AddLogEntry(_folderInfo.Object, It.IsAny<EventLogController.EventLogType>()));
            _mockFolderManager.Setup(mfm => mfm.SaveFolderPermissions(_folderInfo.Object));
            _mockFolderManager.Setup(mfm => mfm.ClearFolderCache(It.IsAny<int>()));

            _mockFolderManager.Object.UpdateFolder(_folderInfo.Object);

            _mockData.Verify(md => md.UpdateFolder(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>()), Times.Once());
        }

        #endregion

        #region Synchronize

        [Test]
        [ExpectedArgumentException]
        public void SynchronizeFolder_Throws_On_Null_RelativePath()
        {
            _folderManager.Synchronize(It.IsAny<int>(), null, It.IsAny<bool>(), It.IsAny<bool>());
        }

        [Test]
        public void SynchronizeFolder_Calls_FolderManager_SynchronizeFiles_When_Folder_Needs_Synchronization()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            var mergedTreeItem = new FolderManager.MergedTreeItem();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, mergedTreeItem);

            _mockFolderManager.Setup(mfm => mfm.GetMergedTree(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false)).Returns(mergedTree);
            _mockFolderManager.Setup(mfm => mfm.ProcessMergedTreeItem(mergedTreeItem, 0, mergedTree, Constants.CONTENT_ValidPortalId)).Returns<string>(null);
            _mockFolderManager.Setup(mfm => mfm.NeedsSynchronization(mergedTreeItem, mergedTree)).Returns(true);
            _mockFolderManager.Setup(mfm => mfm.SynchronizeFiles(mergedTreeItem, Constants.CONTENT_ValidPortalId)).Verifiable();

            _mockFolderManager.Object.Synchronize(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false, true);

            _mockFolderManager.Verify();
        }

        [Test]
        public void SynchronizeFolder_Does_Not_Call_FolderManager_SynchronizeFiles_When_Folder_Does_Not_Need_Synchronization()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            var mergedTreeItem = new FolderManager.MergedTreeItem();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, mergedTreeItem);

            _mockFolderManager.Setup(mfm => mfm.GetMergedTree(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false)).Returns(mergedTree);
            _mockFolderManager.Setup(mfm => mfm.ProcessMergedTreeItem(mergedTreeItem, 0, mergedTree, Constants.CONTENT_ValidPortalId)).Returns<string>(null);
            _mockFolderManager.Setup(mfm => mfm.NeedsSynchronization(mergedTreeItem, mergedTree)).Returns(false).Verifiable();

            _mockFolderManager.Object.Synchronize(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false, true);

            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.SynchronizeFiles(mergedTreeItem, Constants.CONTENT_ValidPortalId), Times.Never());
        }

        [Test]
        public void SynchronizeFolder_Calls_FolderManager_LogCollisions_When_There_Are_Collisions()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            var mergedTreeItem = new FolderManager.MergedTreeItem();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, mergedTreeItem);

            _mockFolderManager.Setup(mfm => mfm.GetMergedTree(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false)).Returns(mergedTree);
            _mockFolderManager.Setup(mfm => mfm.ProcessMergedTreeItem(mergedTreeItem, 0, mergedTree, Constants.CONTENT_ValidPortalId)).Returns("");
            _mockFolderManager.Setup(mfm => mfm.NeedsSynchronization(mergedTreeItem, mergedTree)).Returns(false);
            _mockFolderManager.Setup(mfm => mfm.LogCollisions(Constants.CONTENT_ValidPortalId, It.IsAny<List<string>>())).Verifiable();

            _mockFolderManager.Object.Synchronize(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false, false);

            _mockFolderManager.Verify();
        }

        [Test]
        public void SynchronizeFolder_Does_Not_Call_FolderManager_LogCollisions_When_There_Are_No_Collisions()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            var mergedTreeItem = new FolderManager.MergedTreeItem();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, mergedTreeItem);

            _mockFolderManager.Setup(mfm => mfm.GetMergedTree(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false)).Returns(mergedTree);
            _mockFolderManager.Setup(mfm => mfm.ProcessMergedTreeItem(mergedTreeItem, 0, mergedTree, Constants.CONTENT_ValidPortalId)).Returns<string>(null);
            _mockFolderManager.Setup(mfm => mfm.NeedsSynchronization(mergedTreeItem, mergedTree)).Returns(false);

            _mockFolderManager.Object.Synchronize(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false, false);

            _mockFolderManager.Verify(mfm => mfm.LogCollisions(Constants.CONTENT_ValidPortalId, It.IsAny<List<string>>()), Times.Never());
        }

        [Test]
        public void SynchronizeFolder_Returns_The_Number_Of_Collisions()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            var mergedTreeItem = new FolderManager.MergedTreeItem();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, mergedTreeItem);

            _mockFolderManager.Setup(mfm => mfm.GetMergedTree(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false)).Returns(mergedTree);
            _mockFolderManager.Setup(mfm => mfm.ProcessMergedTreeItem(mergedTreeItem, 0, mergedTree, Constants.CONTENT_ValidPortalId)).Returns("");
            _mockFolderManager.Setup(mfm => mfm.NeedsSynchronization(mergedTreeItem, mergedTree)).Returns(false);
            _mockFolderManager.Setup(mfm => mfm.LogCollisions(Constants.CONTENT_ValidPortalId, It.IsAny<List<string>>()));

            var result = _mockFolderManager.Object.Synchronize(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false, false);

            Assert.AreEqual(1, result);
        }

        #endregion

        #region GetFileSystemFolders

        [Test]
        public void GetFileSystemFolders_Returns_Empty_List_When_Folder_Does_Not_Exist()
        {
            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            _directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(false);

            var result = _mockFolderManager.Object.GetFileSystemFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetFileSystemFolders_Returns_One_Item_When_Folder_Exists_And_Is_Not_Recursive()
        {
            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            _directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            var result = _mockFolderManager.Object.GetFileSystemFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.Count(1, result);
            Assert.IsTrue(result.Values[0].ExistsInFileSystem);
        }

        [Test]
        public void GetFileSystemFolders_Calls_FolderManager_GetFileSystemFoldersRecursive_When_Folder_Exists_And_Is_Recursive()
        {
            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(Constants.FOLDER_ValidFolderPath);

            _mockFolderManager.Setup(mfm => mfm.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderPath))
                .Returns(It.IsAny<SortedList<string, FolderManager.MergedTreeItem>>())
                .Verifiable();

            _directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            _mockFolderManager.Object.GetFileSystemFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, true);

            _mockFolderManager.Verify();
        }

        #endregion

        #region GetFileSystemFoldersRecursive

        [Test]
        public void GetFileSystemFoldersRecursive_Returns_One_Item_When_Folder_Does_Not_Have_SubFolders()
        {
            _pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderPath)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            _directory.Setup(d => d.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(new string[0]);

            var result = _mockFolderManager.Object.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderPath);

            Assert.Count(1, result);
        }

        [Test]
        public void GetFileSystemFoldersRecursive_Returns_All_The_Folders_In_Folder_Tree()
        {
            var relativePaths = new Dictionary<string, string>();

            relativePaths.Add(@"C:\folder", "folder/");
            relativePaths.Add(@"C:\folder\subfolder", "folder/subfolder/");
            relativePaths.Add(@"C:\folder\subfolder2", "folder/subfolder2/");
            relativePaths.Add(@"C:\folder\subfolder2\subsubfolder", "folder/subfolder2/subsubfolder/");
            relativePaths.Add(@"C:\folder\subfolder2\subsubfolder2", "folder/subfolder2/subsubfolder2/");

            _pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, It.IsAny<string>()))
                .Returns<int, string>((portalID, physicalPath) => relativePaths[physicalPath]);

            var directories = new List<string>();

            directories.Add(@"C:\folder\subfolder");
            directories.Add(@"C:\folder\subfolder2");
            directories.Add(@"C:\folder\subfolder2\subsubfolder");
            directories.Add(@"C:\folder\subfolder2\subsubfolder2");

            _directory.Setup(d => d.GetDirectories(It.IsAny<string>()))
                .Returns<string>(path => directories.FindAll(sub => sub.StartsWith(path + "\\") && sub.LastIndexOf("\\") == path.Length).ToArray());

            var result = _mockFolderManager.Object.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, @"C:\folder");

            Assert.Count(5, result);

        }

        [Test]
        public void GetFileSystemFoldersRecursive_Sets_ExistsInFileSystem_For_All_Items()
        {
            var relativePaths = new Dictionary<string, string>();

            relativePaths.Add(@"C:\folder", "folder/");
            relativePaths.Add(@"C:\folder\subfolder", "folder/subfolder/");
            relativePaths.Add(@"C:\folder\subfolder2", "folder/subfolder2/");
            relativePaths.Add(@"C:\folder\subfolder2\subsubfolder", "folder/subfolder2/subsubfolder/");
            relativePaths.Add(@"C:\folder\subfolder2\subsubfolder2", "folder/subfolder2/subsubfolder2/");

            _pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, It.IsAny<string>()))
                .Returns<int, string>((portalID, physicalPath) => relativePaths[physicalPath]);

            var directories = new List<string>();

            directories.Add(@"C:\folder");
            directories.Add(@"C:\folder\subfolder");
            directories.Add(@"C:\folder\subfolder2");
            directories.Add(@"C:\folder\subfolder2\subsubfolder");
            directories.Add(@"C:\folder\subfolder2\subsubfolder2");

            _directory.Setup(d => d.GetDirectories(It.IsAny<string>()))
                .Returns<string>(path => directories.FindAll(sub => sub.StartsWith(path + "\\") && sub.LastIndexOf("\\") == path.Length).ToArray());

            var result = _mockFolderManager.Object.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, @"C:\folder");

            Assert.ForAll<FolderManager.MergedTreeItem>(result.Values, item => item.ExistsInFileSystem);
        }

        #endregion

        #region GetDatabaseFolders

        [Test]
        public void GetDatabaseFolders_Returns_Empty_List_When_Folder_Does_Not_Exist()
        {
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns<IFolderInfo>(null);

            var result = _mockFolderManager.Object.GetDatabaseFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetDatabaseFolders_Returns_One_Item_When_Folder_Exists_And_Is_Not_Recursive()
        {
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);

            var result = _mockFolderManager.Object.GetDatabaseFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.Count(1, result);
            Assert.IsTrue(result.Values[0].ExistsInDatabase);
        }

        [Test]
        public void GetDatabaseFolders_Calls_FolderManager_GetDatabaseFoldersRecursive_When_Folder_Exists_And_Is_Recursive()
        {
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(_folderInfo.Object);

            _mockFolderManager.Setup(mfm => mfm.GetDatabaseFoldersRecursive(_folderInfo.Object))
                .Returns(It.IsAny<SortedList<string, FolderManager.MergedTreeItem>>())
                .Verifiable();

            _mockFolderManager.Object.GetDatabaseFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, true);

            _mockFolderManager.Verify();
        }

        #endregion

        #region GetDatabaseFoldersRecursive

        [Test]
        public void GetDatabaseFoldersRecursive_Returns_One_Item_When_Folder_Does_Not_Have_SubFolders()
        {
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var subfolders = new List<IFolderInfo>();

            _mockFolderManager.Setup(mfm => mfm.GetFolders(_folderInfo.Object)).Returns(subfolders);

            var result = _mockFolderManager.Object.GetDatabaseFoldersRecursive(_folderInfo.Object);

            Assert.Count(1, result);
        }

        [Test]
        public void GetDatabaseFoldersRecursive_Returns_All_The_Folders_In_Folder_Tree()
        {
            _folderInfo.Setup(fi => fi.FolderPath).Returns("folder/");
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var subfolders = new List<IFolderInfo>();

            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder/", StorageLocation = Constants.FOLDER_ValidStorageLocation });
            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder2/", StorageLocation = Constants.FOLDER_ValidStorageLocation });
            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder2/subsubfolder/", StorageLocation = Constants.FOLDER_ValidStorageLocation });
            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder2/subsubfolder2/", StorageLocation = Constants.FOLDER_ValidStorageLocation });

            _mockFolderManager.Setup(mfm => mfm.GetFolders(It.IsAny<IFolderInfo>()))
                .Returns<IFolderInfo>(parent => subfolders.FindAll(sub =>
                    sub.FolderPath.StartsWith(parent.FolderPath) &&
                    sub.FolderPath.Length > parent.FolderPath.Length &&
                    sub.FolderPath.Substring(parent.FolderPath.Length).IndexOf("/") == sub.FolderPath.Substring(parent.FolderPath.Length).LastIndexOf("/")));

            var result = _mockFolderManager.Object.GetDatabaseFoldersRecursive(_folderInfo.Object);

            Assert.Count(5, result);
        }

        [Test]
        public void GetDatabaseFoldersRecursive_Sets_ExistsInDatabase_For_All_Items()
        {
            _folderInfo.Setup(fi => fi.FolderPath).Returns("folder/");
            _folderInfo.Setup(fi => fi.StorageLocation).Returns(Constants.FOLDER_ValidStorageLocation);

            var subfolders = new List<IFolderInfo>();

            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder/", StorageLocation = Constants.FOLDER_ValidStorageLocation });
            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder2/", StorageLocation = Constants.FOLDER_ValidStorageLocation });
            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder2/subsubfolder/", StorageLocation = Constants.FOLDER_ValidStorageLocation });
            subfolders.Add(new FolderInfo() { FolderPath = "folder/subfolder2/subsubfolder2/", StorageLocation = Constants.FOLDER_ValidStorageLocation });

            _mockFolderManager.Setup(mfm => mfm.GetFolders(It.IsAny<IFolderInfo>()))
                .Returns<IFolderInfo>(parent => subfolders.FindAll(sub =>
                    sub.FolderPath.StartsWith(parent.FolderPath) &&
                    sub.FolderPath.Length > parent.FolderPath.Length &&
                    sub.FolderPath.Substring(parent.FolderPath.Length).IndexOf("/") == sub.FolderPath.Substring(parent.FolderPath.Length).LastIndexOf("/")));

            var result = _mockFolderManager.Object.GetDatabaseFoldersRecursive(_folderInfo.Object);

            Assert.ForAll<FolderManager.MergedTreeItem>(result.Values, item => item.ExistsInDatabase);
        }

        #endregion

        #region GetFolderMappingFolders

        [Test]
        public void GetFolderMappingFolders_Returns_Empty_List_When_Folder_Does_Not_Exist()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _mockFolder.Setup(mf => mf.ExistsFolder(Constants.FOLDER_ValidFolderRelativePath, folderMapping)).Returns(false);

            var result = _mockFolderManager.Object.GetFolderMappingFolders(folderMapping, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetFolderMappingFolders_Returns_One_Item_When_Folder_Exists_And_Is_Not_Recursive()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _mockFolder.Setup(mf => mf.ExistsFolder(Constants.FOLDER_ValidFolderRelativePath, folderMapping)).Returns(true);

            var result = _mockFolderManager.Object.GetFolderMappingFolders(folderMapping, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.Count(1, result);
            Assert.IsTrue(result.Values[0].ExistsInFolderMappings.Contains(Constants.FOLDER_ValidStorageLocation));
        }

        [Test]
        public void GetFolderMappingFolders_Calls_FolderManager_GetFolderMappingFoldersRecursive_When_Folder_Exists_And_Is_Recursive()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _mockFolder.Setup(mf => mf.ExistsFolder(Constants.FOLDER_ValidFolderRelativePath, folderMapping)).Returns(true);

            _mockFolderManager.Setup(mfm => mfm.GetFolderMappingFoldersRecursive(folderMapping, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(It.IsAny<SortedList<string, FolderManager.MergedTreeItem>>()).Verifiable();

            _mockFolderManager.Object.GetFolderMappingFolders(folderMapping, Constants.FOLDER_ValidFolderRelativePath, true);

            _mockFolderManager.Verify();
        }

        #endregion

        #region GetFolderMappingFoldersRecursive

        [Test]
        public void GetFolderMappingFoldersRecursive_Returns_One_Item_When_Folder_Does_Not_Have_SubFolders()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            var subfolders = new List<string>();

            _mockFolder.Setup(mf => mf.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping)).Returns(subfolders);

            var result = _mockFolderManager.Object.GetFolderMappingFoldersRecursive(folderMapping, Constants.FOLDER_ValidFolderRelativePath);

            Assert.Count(1, result);
        }

        [Test]
        public void GetFolderMappingFoldersRecursive_Returns_All_The_Folders_In_Folder_Tree()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            var subfolders = new List<string>();

            subfolders.Add("folder/subfolder");
            subfolders.Add("folder/subfolder2");
            subfolders.Add("folder/subfolder2/subsubfolder");
            subfolders.Add("folder/subfolder2/subsubfolder2");

            _mockFolder.Setup(mf => mf.GetSubFolders(It.IsAny<string>(), folderMapping))
                .Returns<string, FolderMappingInfo>((parent, fm) => subfolders.FindAll(sub =>
                    sub.StartsWith(parent) &&
                    sub.Length > parent.Length &&
                    sub.Substring(parent.Length).IndexOf("/") == sub.Substring(parent.Length).LastIndexOf("/")));

            var result = _mockFolderManager.Object.GetFolderMappingFoldersRecursive(folderMapping, "folder/");

            Assert.Count(5, result);
        }

        [Test]
        public void GetDatabaseFoldersRecursive_Sets_ExistsInFolderMappings_For_All_Items()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            var subfolders = new List<string>();

            subfolders.Add("folder/subfolder");
            subfolders.Add("folder/subfolder2");
            subfolders.Add("folder/subfolder2/subsubfolder");
            subfolders.Add("folder/subfolder2/subsubfolder2");

            _mockFolder.Setup(mf => mf.GetSubFolders(It.IsAny<string>(), folderMapping))
                .Returns<string, FolderMappingInfo>((parent, fm) => subfolders.FindAll(sub =>
                    sub.StartsWith(parent) &&
                    sub.Length > parent.Length &&
                    sub.Substring(parent.Length).IndexOf("/") == sub.Substring(parent.Length).LastIndexOf("/")));

            var result = _mockFolderManager.Object.GetFolderMappingFoldersRecursive(folderMapping, "folder/");

            Assert.ForAll<FolderManager.MergedTreeItem>(result.Values, item => item.ExistsInFolderMappings.Contains(Constants.FOLDER_ValidStorageLocation));
        }

        #endregion

        #region MergeFolderLists

        [Test]
        public void MergeFolderLists_Returns_Empty_List_When_Both_Lists_Are_Empty()
        {
            var list1 = new SortedList<string, FolderManager.MergedTreeItem>();
            var list2 = new SortedList<string, FolderManager.MergedTreeItem>();

            var result = _folderManager.MergeFolderLists(list1, list2);

            Assert.IsEmpty(result);
        }

        [Test]
        public void MergeFolderLists_Count_Equals_The_Intersection_Count_Between_Both_Lists()
        {
            var list1 = new SortedList<string, FolderManager.MergedTreeItem>();
            list1.Add("folder1", new FolderManager.MergedTreeItem() { FolderPath = "folder1" });
            list1.Add("folder2", new FolderManager.MergedTreeItem() { FolderPath = "folder2" });

            var list2 = new SortedList<string, FolderManager.MergedTreeItem>();
            list2.Add("folder1", new FolderManager.MergedTreeItem() { FolderPath = "folder1" });
            list2.Add("folder3", new FolderManager.MergedTreeItem() { FolderPath = "folder3" });

            var result = _folderManager.MergeFolderLists(list1, list2);

            Assert.Count(3, result);
        }

        [Test]
        public void MergeFolderLists_Merges_TreeItem_Properties()
        {
            var list1 = new SortedList<string, FolderManager.MergedTreeItem>();
            list1.Add("folder1", new FolderManager.MergedTreeItem() { FolderPath = "folder1", ExistsInFileSystem = true, ExistsInDatabase = true, StorageLocation = Constants.FOLDER_ValidStorageLocation });

            var list2 = new SortedList<string, FolderManager.MergedTreeItem>();
            list2.Add("folder1", new FolderManager.MergedTreeItem() { FolderPath = "folder1", ExistsInFileSystem = false, ExistsInDatabase = false, ExistsInFolderMappings = new List<int> { Constants.FOLDER_ValidStorageLocation } });

            var result = _folderManager.MergeFolderLists(list1, list2);

            Assert.Count(1, result);
            Assert.IsTrue(result.Values[0].ExistsInFileSystem);
            Assert.IsTrue(result.Values[0].ExistsInDatabase);
            Assert.AreEqual(Constants.FOLDER_ValidStorageLocation, result.Values[0].StorageLocation);
            Assert.IsTrue(result.Values[0].ExistsInFolderMappings.Contains(Constants.FOLDER_ValidStorageLocation));
        }

        #endregion

        #region ProcessMergedTreeItem

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Sets_StorageLocation_To_Default_When_Folder_Exists_Only_In_FileSystem_And_Database_And_FolderMapping_Is_Not_Default_And_Has_SubFolders()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem() { FolderPath = Constants.FOLDER_ValidFolderRelativePath, ExistsInFileSystem = true, ExistsInDatabase = true, StorageLocation = Constants.FOLDER_ValidStorageLocation });
            mergedTree.Add(Constants.FOLDER_ValidSubFolderRelativePath, new FolderManager.MergedTreeItem() { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath });

            var folderMappingOfItem = new FolderMappingInfo();
            var defaultFolderMapping = new FolderMappingInfo();
            defaultFolderMapping.FolderMappingID = 1;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, 1)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Deletes_Folder_From_FileSystem_And_Database_When_Folder_Exists_Only_In_FileSystem_And_Database_And_FolderMapping_Is_Not_Default_And_Has_Not_SubFolders()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem() { FolderPath = Constants.FOLDER_ValidFolderRelativePath, ExistsInFileSystem = true, ExistsInDatabase = true, StorageLocation = Constants.FOLDER_ValidStorageLocation });

            var folderMappingOfItem = new FolderMappingInfo();

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _directory.Verify(d => d.Delete(Constants.FOLDER_ValidFolderPath, false), Times.Once());
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_FolderMapping_Is_Default()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();
            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem() { FolderPath = Constants.FOLDER_ValidFolderRelativePath, ExistsInFileSystem = true, ExistsInDatabase = true, StorageLocation = Constants.FOLDER_ValidStorageLocation });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderProviderType = "StandardFolderProvider";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Sets_StorageLocation_To_The_Highest_Priority_One_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Default_And_Folder_Does_Not_Contain_Files()
        {
            var externalStorageLocation = 15;
            var externalMappingName = "External Mapping";

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMappingOfItem.FolderProviderType = "StandardFolderProvider";

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = externalStorageLocation;
            externalFolderMapping.MappingName = externalMappingName;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(_folderInfo.Object);

            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

            var mockFolder = MockComponentProvider.CreateFolderProvider("StandardFolderProvider");

            mockFolder.Setup(mf => mf.GetFiles(_folderInfo.Object)).Returns(new string[0]);

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalMappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Default_And_Folder_Contains_Files()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMappingOfItem.FolderProviderType = "StandardFolderProvider";
            folderMappingOfItem.MappingName = "Default Mapping";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(_folderInfo.Object);

            var mockFolder = MockComponentProvider.CreateFolderProvider("StandardFolderProvider");

            mockFolder.Setup(mf => mf.GetFiles(_folderInfo.Object)).Returns(new string[1]);

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Sets_StorageLocation_To_The_Highest_Priority_One_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Different_From_Actual()
        {
            var externalStorageLocation = 15;
            var externalMappingName = "External Mapping";

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderMappingID = Constants.FOLDER_ValidStorageLocation;

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = externalStorageLocation;
            externalFolderMapping.MappingName = externalMappingName;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalMappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Equal_Than_Actual()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { Constants.FOLDER_ValidStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMappingOfItem.MappingName = "Default Mapping";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_Database_With_Default_StorageLocation_When_Folder_Exists_Only_In_FileSystem()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = false
            });

            var defaultFolderMapping = new FolderMappingInfo();
            defaultFolderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;

            _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_Database_With_Default_StorageLocation_When_Folder_Exists_Only_In_FileSystem_And_One_Or_More_FolderMappings_And_Folder_Contains_Files()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = false,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var defaultFolderMapping = new FolderMappingInfo();
            defaultFolderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            defaultFolderMapping.MappingName = "Default Mapping";

            _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            _directory.Setup(d => d.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(new string[1]);

            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, defaultFolderMapping.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_Database_With_The_Highest_Priority_StorageLocation_When_Folder_Exists_Only_In_FileSystem_And_One_Or_More_FolderMappings_And_Folder_Does_Not_Contain_Files()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = true,
                ExistsInDatabase = false,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = externalStorageLocation;
            externalFolderMapping.MappingName = "External Mapping";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            _directory.Setup(d => d.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(new string[0]);

            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_FolderMapping_Is_Default()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderProviderType = "StandardFolderProvider";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_Default_When_Folder_Exists_Only_In_Database_And_FolderMapping_Is_Not_Default_And_Folder_Has_SubFolders()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation
            });

            mergedTree.Add(Constants.FOLDER_ValidSubFolderRelativePath, new FolderManager.MergedTreeItem() { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath });

            var defaultFolderMapping = new FolderMappingInfo();
            defaultFolderMapping.FolderMappingID = 1;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(new FolderMappingInfo());
            _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, defaultFolderMapping.FolderMappingID)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Deletes_Folder_In_Database_When_Folder_Exists_Only_In_Database_And_FolderMapping_Is_Not_Default_And_Folder_Has_Not_SubFolders()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation
            });

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(new FolderMappingInfo());

            _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_Only_External_FolderMapping_When_Folder_Exists_Only_In_Database_And_One_FolderMapping_And_FolderMapping_Is_Default_But_Not_Database()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderProviderType = "StandardFolderProvider";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_More_Than_One_FolderMapping_And_FolderMapping_Is_Default_But_Not_Database()
        {
            var externalStorageLocation1 = 15;
            var externalStorageLocation2 = 16;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation1, externalStorageLocation2 }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderProviderType = "StandardFolderProvider";

            var externalFolderMapping1 = new FolderMappingInfo();
            externalFolderMapping1.FolderMappingID = externalStorageLocation1;
            externalFolderMapping1.Priority = 0;
            externalFolderMapping1.MappingName = "External Mapping";

            var externalFolderMapping2 = new FolderMappingInfo();
            externalFolderMapping2.Priority = 1;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation1)).Returns(externalFolderMapping1);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation2)).Returns(externalFolderMapping2);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation1)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping1.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Database_And_Folder_Does_Not_Contain_Files()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderProviderType = "DatabaseFolderProvider";

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = externalStorageLocation;
            externalFolderMapping.MappingName = "External Mapping";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
            _mockFolderManager.Setup(mfm => mfm.GetFiles(_folderInfo.Object)).Returns(new List<IFileInfo>());
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Database_And_Folder_Contains_Files()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderProviderType = "DatabaseFolderProvider";
            folderMappingOfItem.MappingName = "Database Mapping";

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
            _mockFolderManager.Setup(mfm => mfm.GetFiles(_folderInfo.Object)).Returns(new List<IFileInfo>() { new FileInfo() });
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Different_From_Actual()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = externalStorageLocation;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_More_Than_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_IsDifferent_From_Actual()
        {
            var externalStorageLocation1 = 15;
            var externalStorageLocation2 = 16;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { externalStorageLocation1, externalStorageLocation2 }
            });

            var folderMappingOfItem = new FolderMappingInfo();

            var externalFolderMapping1 = new FolderMappingInfo();
            externalFolderMapping1.FolderMappingID = externalStorageLocation1;
            externalFolderMapping1.Priority = 0;
            externalFolderMapping1.MappingName = "External Mapping";

            var externalFolderMapping2 = new FolderMappingInfo();
            externalFolderMapping2.Priority = 1;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation1)).Returns(externalFolderMapping1);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation2)).Returns(externalFolderMapping2);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.UpdateStorageLocation(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation1)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping1.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Equal_Than_Actual()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { Constants.FOLDER_ValidStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderMappingID = Constants.FOLDER_ValidStorageLocation;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_More_Than_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Equal_Than_Actual()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = true,
                StorageLocation = Constants.FOLDER_ValidStorageLocation,
                ExistsInFolderMappings = new List<int> { Constants.FOLDER_ValidStorageLocation, externalStorageLocation }
            });

            var folderMappingOfItem = new FolderMappingInfo();
            folderMappingOfItem.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            folderMappingOfItem.Priority = 0;
            folderMappingOfItem.MappingName = "External Mapping";

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = externalStorageLocation;
            externalFolderMapping.Priority = 1;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(folderMappingOfItem);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Creates_Folder_In_Database_With_The_Highest_Priority_StorageLocation_When_Folder_Exists_Only_In_One_FolderMapping()
        {
            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = false,
                ExistsInFolderMappings = new List<int> { Constants.FOLDER_ValidStorageLocation }
            });

            var externalFolderMapping = new FolderMappingInfo();
            externalFolderMapping.FolderMappingID = Constants.FOLDER_ValidStorageLocation;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(externalFolderMapping);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.IsNull(result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Creates_Folder_In_Database_With_The_Highest_Priority_StorageLocation_When_Folder_Exists_Only_In_More_Than_One_FolderMapping()
        {
            var externalStorageLocation = 15;

            var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>();

            mergedTree.Add(Constants.FOLDER_ValidFolderRelativePath, new FolderManager.MergedTreeItem()
            {
                FolderPath = Constants.FOLDER_ValidFolderRelativePath,
                ExistsInFileSystem = false,
                ExistsInDatabase = false,
                ExistsInFolderMappings = new List<int> { Constants.FOLDER_ValidStorageLocation, externalStorageLocation }
            });

            var externalFolderMapping1 = new FolderMappingInfo();
            externalFolderMapping1.FolderMappingID = Constants.FOLDER_ValidStorageLocation;
            externalFolderMapping1.Priority = 0;
            externalFolderMapping1.MappingName = "External Mapping";

            var externalFolderMapping2 = new FolderMappingInfo();
            externalFolderMapping2.FolderMappingID = externalStorageLocation;
            externalFolderMapping2.Priority = 1;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidStorageLocation)).Returns(externalFolderMapping1);
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping2);

            _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
            _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidStorageLocation)).Verifiable();

            var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping1.MappingName), result);
            _mockFolderManager.Verify();
            _mockFolderManager.Verify(mfm => mfm.UpdateStorageLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        #endregion
    }
}
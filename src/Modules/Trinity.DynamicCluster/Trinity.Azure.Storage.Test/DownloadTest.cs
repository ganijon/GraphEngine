﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Trinity.Storage;

namespace Trinity.Azure.Storage.Test
{
    [TestClass]
    public class DownloadTest
    {
        private BlobStoragePersistentStorage m_storage;
        private CloudBlobClient m_client;
        private readonly Guid m_version = Guid.NewGuid();

        [TestInitialize]
        public void Init()
        {
            ConfigInit.Init();
            m_storage = new BlobStoragePersistentStorage();
            m_client = m_storage._test_getclient();
            var container = m_client.GetContainerReference(BlobStorageConfig.Instance.ContainerName);
            container.CreateIfNotExists();
            var dir = container.GetDirectoryReference(m_version.ToString());
            Chunk c1 = new Chunk(0, 10);
            Chunk c2 = new Chunk(100, 110);
            string idx = string.Join("\n", new[] {c1, c2}.Select(JsonConvert.SerializeObject));
            dir.GetBlockBlobReference(Constants.c_index).UploadText(idx);
            byte[] f1 = new byte[14];
            byte[] f2 = new byte[16];
            f2[0] = 0xEB;
            f2[10] = 0x02;
            f2[14] = 0xDE;
            f2[15] = 0xAD;
            dir.GetBlockBlobReference(c1.Id.ToString()).UploadFromByteArray(f1, 0, f1.Length);
            dir.GetBlockBlobReference(c2.Id.ToString()).UploadFromByteArray(f2, 0, f2.Length);
            dir.GetBlockBlobReference(Constants.c_finished).UploadText("");
        }

        [TestMethod]
        public async Task GetLatestVersion()
        {
            var v = await m_storage.GetLatestVersion();
            Assert.AreEqual(m_version, v);
        }
    }
}

// Author: JiangHao <jianghao01@hetao101.com>

using System;
using System.Collections.Generic;
using System.IO;
using VeryFS.Framework.Runtime.Utilities;


namespace VeryFS.Framework.Runtime.Resource
{
    public enum AssetBundleNameStyle
    {
        BundleName = 0,
        HashName = 1,
        BundleName_HashName = 2,
    }

    public class AssetManifest : IBinarySerializable
    {
        public string address;
        // public string guid;
        public string path;
        public int bundleId;
        
        public void Serialize(BytesWriter writer)
        {
            writer.WriteString(address);
            // writer.WriteString(guid);
            writer.WriteString(path);
            writer.WriteInt32(bundleId);
        }

        public void Deserialize(BytesReader reader)
        {
            address = reader.ReadString();
            // guid = reader.ReadString();
            path = reader.ReadString();
            bundleId = reader.ReadInt32();
        }
    }

    public class RawFileManifest : IBinarySerializable
    {
        // public string group;
        public string address;
        public string path;
        public uint crc;
        public string hash;
        public long size;
        

        public void Serialize(BytesWriter writer)
        {
            writer.WriteString(address);
            writer.WriteString(path);
            writer.WriteUint32(crc);
            writer.WriteString(hash);
            writer.WriteInt64(size);
        }

        public void Deserialize(BytesReader reader)
        {
            address = reader.ReadString();
            path = reader.ReadString();
            crc = reader.ReadUint32();
            hash = reader.ReadString();
            size = reader.ReadInt64();
        }
    }

    public class BundleManifest : IBinarySerializable
    {
        public string bundleName;
        public uint unityCRC;
        public uint fileCRC;
        public string fileHash;
        public long fileSize;
        public int[] dependencies;


        public void Serialize(BytesWriter writer)
        {
            writer.WriteString(bundleName);
            writer.WriteUint32(unityCRC);
            writer.WriteUint32(fileCRC);
            writer.WriteString(fileHash);
            writer.WriteInt64(fileSize);
            int cnt = dependencies != null ? dependencies.Length : 0;

            writer.WriteCount(cnt);
            for (int i = 0; i < cnt; i++)
            {
                writer.WriteInt32(dependencies[i]);
            }
        }

        public void Deserialize(BytesReader reader)
        {
            bundleName = reader.ReadString();
            unityCRC = reader.ReadUint32();
            fileCRC = reader.ReadUint32();
            fileHash = reader.ReadString();
            fileSize = reader.ReadInt64();
            int cnt = reader.ReadCount();
            dependencies = new int[cnt];
            for (int i = 0; i < cnt; i++)
            {
                dependencies[i] = reader.ReadInt32();
            }
        }


        public string GetBundleFileName(AssetBundleNameStyle style)
        {
            string destFile;
            switch (style)
            {
                case AssetBundleNameStyle.HashName:
                    destFile = this.fileHash + ResourcePath.BundleExtension;
                    break;
                case AssetBundleNameStyle.BundleName_HashName:
                    destFile = Path.GetFileNameWithoutExtension(bundleName) + "_" + fileHash + ResourcePath.BundleExtension ;
                    break;
                default:
                    destFile = bundleName;
                    break;
            }

            return destFile;
        }
    }

    public class PackageManifest : IBinarySerializable
    {
        public string packageName;
        public string buildVersion;
        public long buildTime;
        public AssetBundleNameStyle style;
        public int version = 1;

        public List<AssetManifest> assets;
        public List<BundleManifest> bundles;
        public List<RawFileManifest> raws;
        public Dictionary<string, BundleInfo> bundleMap;

        public struct BundleInfo
        {
            public int bundleId;
            public string tag;
        }

        public BundleManifest GetBundle(int idx)
        {
            if (idx < 0 || idx >= bundles.Count)
            {
                throw new Exception($"{packageName}: Invalid BundleId {idx}");
            }

            return bundles[idx];
        }



        public void WriteToFile(string path)
        {
            byte[] data = new byte[1024 * 1024 * 2];
            var writer = new BytesWriter(ref data, data.Length);
            this.Serialize(writer);
            //I8E2M(data, writer.Position, (uint) writer.Position);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, writer.Position);
            }
        }
        

        public static PackageManifest CreateFromData(byte[] data)
        {
            var reader = new BytesReader(ref data, data.Length);
            var manifest = new PackageManifest();
            manifest.Deserialize(reader);
            return manifest;
        }

        public void Serialize(BytesWriter writer)
        {
            writer.WriteInt32(version);
            writer.WriteString(packageName);
            writer.WriteString(buildVersion);
            writer.WriteInt64(buildTime);
            writer.WriteUint8((byte)style);

            writer.WriteSerializableList(assets);
            writer.WriteSerializableList(raws);
            writer.WriteSerializableList(bundles);
            if (bundleMap != null)
            {
                writer.WriteCount(bundleMap.Count);
                foreach (var pair in bundleMap)
                {
                    writer.WriteString(pair.Key);
                    writer.WriteInt32(pair.Value.bundleId);
                    writer.WriteString(pair.Value.tag);
                }
            }
        }

        public void Deserialize(BytesReader reader)
        {
            version = reader.ReadInt32();
            packageName = reader.ReadString();
            buildVersion = reader.ReadString();
            buildTime = reader.ReadInt64();
            style = (AssetBundleNameStyle)reader.ReadUint8();
            assets = reader.ReadSerializableList<AssetManifest>();
            raws = reader.ReadSerializableList<RawFileManifest>();
            bundles = reader.ReadSerializableList<BundleManifest>();
            int cnt = reader.ReadCount();
            bundleMap = new();
            for (int i = 0; i < cnt; i++)
            {
                string key = reader.ReadString();
                bundleMap.Add(key, new BundleInfo()
                {
                    bundleId = reader.ReadInt32(),
                    tag = reader.ReadString()
                });
            }
        }

        public BundleManifest[] CalculateBundleDifferences(PackageManifest other)
        {
            List<BundleManifest> list = new();
            //命名规则变了
            if (other.style != this.style)
            {
                return bundles.ToArray();
            }
            
            foreach (var bundle in bundles)
            {
                var bundle2 = other.bundles.Find(m => m.bundleName == bundle.bundleName);
                if (bundle2 == null ||
                    (bundle2.fileSize != bundle.fileSize ||
                     bundle2.fileCRC != bundle.fileCRC ||
                     bundle2.fileHash != bundle.fileHash))
                {
                    list.Add(bundle);
                }
            }

            return list.ToArray();
        }

        public RawFileManifest[] CalculateRawDifferences(PackageManifest other)
        {
            List<RawFileManifest> list = new();
            foreach (var raw in raws)
            {
                var raw2 = other.raws.Find(m =>  m.path==raw.path);
                if (raw2 == null ||
                    (raw2.size != raw.size ||
                     raw2.crc != raw.crc ||
                     raw2.hash != raw.hash))
                {
                    list.Add(raw);
                }
            }
            return list.ToArray();
        }
    }


}
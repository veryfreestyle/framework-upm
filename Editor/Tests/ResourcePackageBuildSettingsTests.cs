// Author: JiangHao <jianghao01@hetao101.com>

using System;
using NUnit.Framework;
using VeryFS.Framework.Editor.Resource;

namespace VeryFS.Framework.Editor.Tests
{
    public class ResourcePackageBuildSettingsTests
    {
        [Test]
        public void FormatVersion_日期加当日分钟数()
        {
            // 13:02 → 13*60+2 = 782
            string v = ResourcePackageBuildSettings.FormatVersion(new DateTime(2026, 7, 15, 13, 2, 0));
            Assert.AreEqual("20260715_782", v);
        }

        [Test]
        public void FormatVersion_零点_分钟数为0()
        {
            string v = ResourcePackageBuildSettings.FormatVersion(new DateTime(2026, 7, 15, 0, 0, 0));
            Assert.AreEqual("20260715_0", v);
        }

        [Test]
        public void FormatVersion_2359_分钟数为1439()
        {
            // 23:59 → 23*60+59 = 1439
            string v = ResourcePackageBuildSettings.FormatVersion(new DateTime(2026, 1, 5, 23, 59, 0));
            Assert.AreEqual("20260105_1439", v);
        }
    }
}

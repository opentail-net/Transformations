namespace Transformations.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class FileInfoHelperAdditionalTests
    {
        [Test]
        public void MoveTo_MovesFilesToTargetDirectory()
        {
            string root = Path.Combine(Path.GetTempPath(), "Transformations_FileInfo_Move_" + Guid.NewGuid().ToString("N"));
            string sourceDir = Path.Combine(root, "src");
            string targetDir = Path.Combine(root, "dst");
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(targetDir);

            try
            {
                string filePath = Path.Combine(sourceDir, "a.txt");
                File.WriteAllText(filePath, "data");
                FileInfo[] files = { new FileInfo(filePath) };

                files.MoveTo(targetDir);

                Assert.That(File.Exists(Path.Combine(targetDir, "a.txt")), Is.True);
                Assert.That(File.Exists(filePath), Is.False);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        [Test]
        public void SetAttributesAdditive_AppendsRequestedAttribute()
        {
            string root = Path.Combine(Path.GetTempPath(), "Transformations_FileInfo_Attr_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            try
            {
                string filePath = Path.Combine(root, "b.txt");
                File.WriteAllText(filePath, "data");
                var file = new FileInfo(filePath);
                file.Attributes = FileAttributes.Normal;

                FileInfo[] files = { file };
                files.SetAttributesAdditive(FileAttributes.ReadOnly);

                var refreshed = new FileInfo(filePath);
                Assert.That((refreshed.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly, Is.True);

                refreshed.Attributes = FileAttributes.Normal;
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    foreach (var file in Directory.GetFiles(root))
                    {
                        var fi = new FileInfo(file);
                        fi.Attributes = FileAttributes.Normal;
                    }

                    Directory.Delete(root, true);
                }
            }
        }
    }
}

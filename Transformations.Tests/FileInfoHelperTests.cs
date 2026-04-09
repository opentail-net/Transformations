namespace Transformations.Tests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class FileInfoHelperTests
    {
        private string _testDir = null!;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "TransformationsTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        #region Rename

        [Test]
        public void Rename_ValidFile_RenamesSuccessfully()
        {
            //// Setup
            string filePath = Path.Combine(_testDir, "original.txt");
            File.WriteAllText(filePath, "test content");
            var file = new FileInfo(filePath);

            //// Act
            file.Rename("renamed.txt");

            //// Assert
            Assert.That(File.Exists(Path.Combine(_testDir, "renamed.txt")), Is.True);
        }

        [Test]
        public void Rename_EmptyName_DoesNotRename()
        {
            //// Setup
            string filePath = Path.Combine(_testDir, "original.txt");
            File.WriteAllText(filePath, "test content");
            var file = new FileInfo(filePath);

            //// Act
            file.Rename(string.Empty);

            //// Assert
            Assert.That(File.Exists(filePath), Is.True);
        }

        #endregion Rename

        #region RenameFileWithoutExtension

        [Test]
        public void RenameFileWithoutExtension_ValidFile_KeepsExtension()
        {
            //// Setup
            string filePath = Path.Combine(_testDir, "original.txt");
            File.WriteAllText(filePath, "test content");
            var file = new FileInfo(filePath);

            //// Act
            file.RenameFileWithoutExtension("newname");

            //// Assert
            Assert.That(File.Exists(Path.Combine(_testDir, "newname.txt")), Is.True);
        }

        #endregion RenameFileWithoutExtension

        #region ChangeExtension

        [Test]
        public void ChangeExtension_ValidFile_ChangesExtension()
        {
            //// Setup
            string filePath = Path.Combine(_testDir, "test.txt");
            File.WriteAllText(filePath, "test content");
            var file = new FileInfo(filePath);

            //// Act
            file.ChangeExtension("xml");

            //// Assert
            Assert.That(File.Exists(Path.Combine(_testDir, "test.xml")), Is.True);
        }

        #endregion ChangeExtension

        #region SetAttributes

        [Test]
        public void SetAttributes_ValidFiles_SetsAttributes()
        {
            //// Setup
            string file1 = Path.Combine(_testDir, "f1.txt");
            string file2 = Path.Combine(_testDir, "f2.txt");
            File.WriteAllText(file1, "a");
            File.WriteAllText(file2, "b");
            var files = new[] { new FileInfo(file1), new FileInfo(file2) };

            //// Act
            files.SetAttributes(FileAttributes.ReadOnly);

            //// Assert
            Assert.That(new FileInfo(file1).Attributes.HasFlag(FileAttributes.ReadOnly), Is.True);
            Assert.That(new FileInfo(file2).Attributes.HasFlag(FileAttributes.ReadOnly), Is.True);

            //// Cleanup - remove readonly so TearDown can delete
            files.SetAttributes(FileAttributes.Normal);
        }

        #endregion SetAttributes

        #region CopyTo

        [Test]
        public void CopyTo_ValidFiles_CopiesSuccessfully()
        {
            //// Setup
            string file1 = Path.Combine(_testDir, "f1.txt");
            File.WriteAllText(file1, "content");
            var files = new[] { new FileInfo(file1) };
            string targetDir = Path.Combine(_testDir, "target");
            Directory.CreateDirectory(targetDir);

            //// Act
            FileInfo[] copied = files.CopyTo(targetDir);

            //// Assert
            Assert.That(copied.Length, Is.EqualTo(1));
            Assert.That(File.Exists(Path.Combine(targetDir, "f1.txt")), Is.True);
        }

        #endregion CopyTo

        #region Delete

        [Test]
        public void Delete_ValidFiles_DeletesAll()
        {
            //// Setup
            string file1 = Path.Combine(_testDir, "d1.txt");
            string file2 = Path.Combine(_testDir, "d2.txt");
            File.WriteAllText(file1, "a");
            File.WriteAllText(file2, "b");
            var files = new[] { new FileInfo(file1), new FileInfo(file2) };

            //// Act
            files.Delete();

            //// Assert
            Assert.That(File.Exists(file1), Is.False);
            Assert.That(File.Exists(file2), Is.False);
        }

        #endregion Delete
    }
}

namespace Transformations.Tests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class DirectoryInfoHelperTests
    {
        private string _testDir = null!;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "TransformationsTests_Dir_" + Guid.NewGuid().ToString("N"));
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

        #region CopyTo

        [Test]
        public void CopyTo_ValidDirectory_CopiesContents()
        {
            //// Setup
            string sourceDir = Path.Combine(_testDir, "source");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(sourceDir, "file2.txt"), "content2");
            string targetDir = Path.Combine(_testDir, "target");

            //// Act
            var result = new DirectoryInfo(sourceDir).CopyTo(targetDir);

            //// Assert
            Assert.That(result.Exists, Is.True);
            Assert.That(File.Exists(Path.Combine(targetDir, "file1.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(targetDir, "file2.txt")), Is.True);
        }

        [Test]
        public void CopyTo_WithSubDirectories_CopiesRecursively()
        {
            //// Setup
            string sourceDir = Path.Combine(_testDir, "source");
            string subDir = Path.Combine(sourceDir, "sub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "nested.txt"), "nested content");
            string targetDir = Path.Combine(_testDir, "target");

            //// Act
            new DirectoryInfo(sourceDir).CopyTo(targetDir);

            //// Assert
            Assert.That(File.Exists(Path.Combine(targetDir, "sub", "nested.txt")), Is.True);
        }

        #endregion CopyTo

        #region FindFileRecursive

        [Test]
        public void FindFileRecursive_PatternMatch_ReturnsFile()
        {
            //// Setup
            string subDir = Path.Combine(_testDir, "sub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "target.txt"), "found me");
            var dir = new DirectoryInfo(_testDir);

            //// Act
            FileInfo? actual = dir.FindFileRecursive("target.txt");

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Name, Is.EqualTo("target.txt"));
        }

        [Test]
        public void FindFileRecursive_NoMatch_ReturnsNull()
        {
            //// Setup
            var dir = new DirectoryInfo(_testDir);

            //// Act
            FileInfo? actual = dir.FindFileRecursive("nonexistent.xyz");

            //// Assert
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void FindFileRecursive_Predicate_ReturnsMatchingFile()
        {
            //// Setup
            File.WriteAllText(Path.Combine(_testDir, "test.xml"), "xml content");
            var dir = new DirectoryInfo(_testDir);

            //// Act
            FileInfo? actual = dir.FindFileRecursive(f => f.Extension == ".xml");

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Extension, Is.EqualTo(".xml"));
        }

        [Test]
        public void FindFileRecursive_PredicateNoMatch_ReturnsNull()
        {
            //// Setup
            File.WriteAllText(Path.Combine(_testDir, "test.txt"), "content");
            var dir = new DirectoryInfo(_testDir);

            //// Act
            FileInfo? actual = dir.FindFileRecursive(f => f.Extension == ".xyz");

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion FindFileRecursive

        #region GetFiles (multiple patterns)

        [Test]
        public void GetFiles_MultiplePatterns_ReturnsAllMatching()
        {
            //// Setup
            File.WriteAllText(Path.Combine(_testDir, "a.txt"), "a");
            File.WriteAllText(Path.Combine(_testDir, "b.xml"), "b");
            File.WriteAllText(Path.Combine(_testDir, "c.csv"), "c");
            var dir = new DirectoryInfo(_testDir);

            //// Act
            FileInfo[] actual = dir.GetFiles("*.txt", "*.xml");

            //// Assert
            Assert.That(actual.Length, Is.EqualTo(2));
        }

        [Test]
        public void GetFiles_NoMatch_ReturnsEmpty()
        {
            //// Setup
            File.WriteAllText(Path.Combine(_testDir, "a.txt"), "a");
            var dir = new DirectoryInfo(_testDir);

            //// Act
            FileInfo[] actual = dir.GetFiles("*.xyz");

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        #endregion GetFiles (multiple patterns)
    }
}

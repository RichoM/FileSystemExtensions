using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSystemExtensions.Test
{
    [TestClass]
    public class SafeEnumerateTest
    {
        [TestMethod]
        public void Test001_OneLevelFolderSingleFile()
        {
            var dir = new DirectoryInfo(@"Resources\A");
            var expected = new[] { "1.txt" };
            var actual = dir.SafeEnumerateFiles(searchOption: SearchOption.AllDirectories)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test002_TwoLevelsDeepSingleFileEachFolderExceptRoot()
        {
            var dir = new DirectoryInfo(@"Resources\B");
            var expected = new[] { "2.txt", "3.txt" };
            var actual = dir.SafeEnumerateFiles(searchOption: SearchOption.AllDirectories)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test003_TwoLevelsDeepTopDirectoryOnly()
        {
            var dir = new DirectoryInfo(@"Resources\B");
            var expected = new string[0];
            var actual = dir.SafeEnumerateFiles(searchOption: SearchOption.TopDirectoryOnly)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test004_TwoLevelsDeepSingleFileEachFolder()
        {
            var dir = new DirectoryInfo(@"Resources\C");
            var expected = new[] { "4.txt", "5.txt", "6.txt" };
            var actual = dir.SafeEnumerateFiles(searchOption: SearchOption.AllDirectories)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test005_TwoLevelsDeepSingleFileEachFolderTopDirectoryOnly()
        {
            var dir = new DirectoryInfo(@"Resources\C");
            var expected = new[] { "4.txt" };
            var actual = dir.SafeEnumerateFiles(searchOption: SearchOption.TopDirectoryOnly)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test006_SearchPattern1()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new[] { "6.txt", "9.txt" };
            var actual = dir.SafeEnumerateFiles("*.txt", SearchOption.AllDirectories)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test007_SearchPattern2()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new[] { "7.bmp" };
            var actual = dir.SafeEnumerateFiles("*.bmp", SearchOption.AllDirectories)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test008_SearchPattern3TopDirectoryOnly()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new string[] { "6.txt" };
            var actual = dir.SafeEnumerateFiles("*.txt", SearchOption.TopDirectoryOnly)
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}

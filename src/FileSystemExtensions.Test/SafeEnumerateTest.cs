using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSystemExtensions.Test
{
    [TestClass]
    public class SafeEnumerateTest
    {
        private IEnumerable<DirectoryInfo> GetDirectories(params string[] names)
        {
            return names.Select(each => new DirectoryInfo(each));
        }

        [TestMethod]
        public void Test001_OneLevelFolderSingleFile()
        {
            var dir = new DirectoryInfo(@"Resources\A");
            var expected = new[] { "1.txt" };
            var actual = dir.SafeEnumerateFiles()
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test002_TwoLevelsDeepSingleFileEachFolderExceptRoot()
        {
            var dir = new DirectoryInfo(@"Resources\B");
            var expected = new[] { "2.txt", "3.txt" };
            var actual = dir.SafeEnumerateFiles()
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
            var actual = dir.SafeEnumerateFiles()
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
            var actual = dir.SafeEnumerateFiles("*.txt")
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test007_SearchPattern2()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new[] { "7.bmp" };
            var actual = dir.SafeEnumerateFiles("*.bmp")
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

        [TestMethod]
        public void Test009_ExclusionsEmpty()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new string[] { "6.txt", "7.bmp", "8.csv", "9.txt", "10.xls", "11.gif" };
            var actual = dir.SafeEnumerateFiles(exclusions: new DirectoryInfo[0])
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test010_ExclusionsSingleElement()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new string[] { "6.txt", "8.csv", "9.txt", "10.xls", "11.gif" };
            var actual = dir.SafeEnumerateFiles(exclusions: GetDirectories(@"Resources\D\DA"))
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test011_ExclusionsMultipleElements()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new string[] { "6.txt", "10.xls", "11.gif" };
            var actual = dir.SafeEnumerateFiles(exclusions: GetDirectories(@"Resources\D\DA", @"Resources\D\DB"))
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test012_ExclusionsDeep1()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new string[] { "6.txt", "7.bmp", "8.csv", "9.txt" };
            var actual = dir.SafeEnumerateFiles(exclusions: GetDirectories(@"Resources\D\DC"))
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test013_ExclusionsDeep2()
        {
            var dir = new DirectoryInfo(@"Resources\D");
            var expected = new string[] { "6.txt", "7.bmp", "8.csv", "9.txt", "10.xls" };
            var actual = dir.SafeEnumerateFiles(exclusions: GetDirectories(@"Resources\D\DC\DCA"))
                .Select(f => f.Name)
                .ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}

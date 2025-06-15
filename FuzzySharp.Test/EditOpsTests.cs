using NUnit.Framework;
using Raffinert.FuzzySharp.Edits;

namespace Raffinert.FuzzySharp.Test;

[TestFixture]
public class EditOpsTests
{
    [Test]
    public void GetEditOps_KittenToSitting_ReturnsExpectedEditOps()
    {
        // Arrange
        string source = "kitten";
        string target = "sitting";

        // Act
        var ops = Levenshtein.GetEditOps(source, target);

        // Assert
        Assert.IsNotNull(ops);
        Assert.AreEqual(3, ops.Length);

        Assert.AreEqual(EditType.REPLACE, ops[0].EditType);
        Assert.AreEqual(0, ops[0].SourcePos);
        Assert.AreEqual(0, ops[0].DestPos);

        Assert.AreEqual(EditType.REPLACE, ops[1].EditType);
        Assert.AreEqual(4, ops[1].SourcePos);
        Assert.AreEqual(4, ops[1].DestPos);

        Assert.AreEqual(EditType.INSERT, ops[2].EditType);
        Assert.AreEqual(6, ops[2].SourcePos);
        Assert.AreEqual(6, ops[2].DestPos);
    }

    [Test]
    public void GetEditOps_putinIsWarCriminal_ReturnsExpectedEditOps()
    {
        // Arrange
        string source = "putin";
        string target = "war criminal";

        // Act
        var ops = Levenshtein.GetEditOps(source, target);

        Assert.That(ops, Is.EquivalentTo(new[]
        {
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 0,
                DestPos = 0
            },
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 0,
                DestPos = 1
            },
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 0,
                DestPos = 2
            },
            new EditOp
            {
                EditType = EditType.REPLACE,
                SourcePos = 0,
                DestPos = 3
            },
            new EditOp
            {
                EditType = EditType.REPLACE,
                SourcePos = 1,
                DestPos = 4
            },
            new EditOp
            {
                EditType = EditType.REPLACE,
                SourcePos = 2,
                DestPos = 5
            },
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 4,
                DestPos = 7
            },
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 4,
                DestPos = 8
            },
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 5,
                DestPos = 10
            },
            new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = 5,
                DestPos = 11
            }
        }));
    }
}
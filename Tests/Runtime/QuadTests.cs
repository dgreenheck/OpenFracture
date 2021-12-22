using NUnit.Framework;

public class QuadTests
{
    [Test]
    public void TestInit()
    {
        int q1 = 1;
        int q2 = 2;
        int q3 = 3;
        int q4 = 4;
        int t1 = 5;
        int t2 = 6;
        int t1L = 7;
        int t1R = 8;
        int t2L = 9;
        int t2R = 10;

        Quad quad = new Quad(q1, q2, q3, q4, t1, t2, t1L, t1R, t2L, t2R);

        Assert.AreEqual(q1, quad.q1);
        Assert.AreEqual(q2, quad.q2);
        Assert.AreEqual(q3, quad.q3);
        Assert.AreEqual(q4, quad.q4);
        Assert.AreEqual(t1, quad.t1);
        Assert.AreEqual(t2, quad.t2);
        Assert.AreEqual(t1L, quad.t1L);
        Assert.AreEqual(t1R, quad.t1R);
        Assert.AreEqual(t2L, quad.t2L);
        Assert.AreEqual(t2R, quad.t2R);
    }
}
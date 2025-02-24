namespace Curve.Primitives.Tests;

using ECP = Point<EllipticCurve>;

public class EllipticCurveTests
{
    // Test parameters (y² = x³ + 2x + 3 mod 17)
    private const int A = 2;
    private const int B = 3;
    private const int PRIME = 17;
    private static readonly ECP ValidPoint = new(3, 6);
    private static readonly ECP InvalidPoint = new(3, 7);
    private static readonly ECP PointInverse = new(3, 11); // -6 mod 17 = 11
    private static readonly ECP ExpectedDouble = new(12, 2);
    private static readonly ECP ExpectedSum = new(15, 5);
    
    private static EllipticCurve CreateTestCurve() => new(
        a: A,
        b: B,
        prime: PRIME,
        basePoint: ValidPoint,
        order: 19
    );

    [Fact]
    public void IsOnCurve_WithValidPoint_ReturnsTrue()
    {
        var curve = CreateTestCurve();
        Assert.True(curve.IsOnCurve(ValidPoint));
    }

    [Fact]
    public void IsOnCurve_WithInvalidPoint_ReturnsFalse()
    {
        var curve = CreateTestCurve();
        Assert.False(curve.IsOnCurve(InvalidPoint));
    }

    [Fact]
    public void Add_WithIdentityPoint_ReturnsOtherPoint()
    {
        var curve = CreateTestCurve();
        Assert.Equal(ValidPoint, EllipticCurve.Add(curve, ECP.Identity, ValidPoint));
        Assert.Equal(ValidPoint, EllipticCurve.Add(curve, ValidPoint, ECP.Identity));
    }

    [Fact]
    public void Add_WithSamePoint_ReturnsCorrectPoint()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.Add(curve, ValidPoint, ValidPoint);
        Assert.Equal(ExpectedDouble, result);
    }

    [Fact]
    public void Add_WithDifferentPoints_ReturnsCorrectPoint()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.Add(curve, ValidPoint, ExpectedDouble);
        Assert.Equal(ExpectedSum, result);
    }

    [Fact]
    public void Add_WithInversePoints_ReturnsIdentity()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.Add(curve, ValidPoint, PointInverse);
        Assert.Equal(ECP.Identity, result);
    }

    [Fact]
    public void Add_ResultIsOnCurve()
    {
        var curve = CreateTestCurve();

        // Test doubling
        var doubleResult = EllipticCurve.Add(curve, ValidPoint, ValidPoint);
        Assert.True(curve.IsOnCurve(doubleResult));

        // Test distinct point addition
        var sumResult = EllipticCurve.Add(curve, ValidPoint, ExpectedDouble);
        Assert.True(curve.IsOnCurve(sumResult));

        // Test identity result
        var inverseResult = EllipticCurve.Add(curve, ValidPoint, PointInverse);
        Assert.True(curve.IsOnCurve(inverseResult));
    }

    [Fact]
    public void ScalarMultiply_WithValidParameters_ReturnsCorrectPoint()
    {
        var curve = CreateTestCurve();

        // 2 * (3,6) = (12,2)
        // in real-world cryptography, the numbers used are much larger [256-bits+] culminating in the Discrete Logarithm Problem
        var doubleResult = EllipticCurve.ScalarMultiply(curve, ValidPoint, 2);
        Assert.Equal(ExpectedDouble, doubleResult);

        // 3 * (3,6) = (3,6) + (12,2) = (15,5)
        var tripleResult = EllipticCurve.ScalarMultiply(curve, ValidPoint, 3);
        Assert.Equal(ExpectedSum, tripleResult);
    }

    [Fact]
    public void ScalarMultiply_WithZero_ReturnsIdentity()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.ScalarMultiply(curve, ValidPoint, 0);
        Assert.Equal(ECP.Identity, result);
    }

    [Fact]
    public void ScalarMultiply_WithNegative_ReturnsInverse()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.ScalarMultiply(curve, ValidPoint, -1);
        Assert.Equal(PointInverse, result);
    }
}
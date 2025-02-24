// ReSharper disable ConvertToConstant.Local
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
    public void Add_AssociativityProperty()
    {
        var curve = CreateTestCurve();
        var p1 = ValidPoint;
        var p2 = ExpectedDouble;
        var p3 = ExpectedSum;
    
        // (P1 + P2) + P3 should equal P1 + (P2 + P3)
        var left = EllipticCurve.Add(curve, EllipticCurve.Add(curve, p1, p2), p3);
        var right = EllipticCurve.Add(curve, p1, EllipticCurve.Add(curve, p2, p3));
        Assert.Equal(left, right);
    }

    [Fact]
    public void Add_CommutativityProperty()
    {
        var curve = CreateTestCurve();
        // Use ValidPoint and its double as two known valid points
        var p1 = ValidPoint;  // (3,6)
        var p2 = new ECP(12, 2);  // Using the known double point
    
        var sum1 = EllipticCurve.Add(curve, p1, p2);
        var sum2 = EllipticCurve.Add(curve, p2, p1);
        Assert.Equal(sum1, sum2);
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
    public void Add_DoubleNegativePoint_EqualsDoublePoint()
    {
        var curve = CreateTestCurve();
        var point = ValidPoint;
        var negPoint = EllipticCurve.Negate(curve, point);
    
        var doubleNeg = EllipticCurve.Add(curve, negPoint, negPoint);
        var doublePos = EllipticCurve.Add(curve, point, point);
        Assert.Equal(doubleNeg, EllipticCurve.Negate(curve, doublePos));
    }

    [Fact]
    public void ScalarMultiply_OrderMinusOne_EqualsNegativeBasePoint()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.ScalarMultiply(curve, ValidPoint, curve.GetOrder() - 1);
        Assert.Equal(EllipticCurve.Negate(curve, ValidPoint), result);
    }

    
    [Fact]
    public void ScalarMultiply_WithOrder_ReturnsIdentity()
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.ScalarMultiply(curve, ValidPoint, 19);
        Assert.Equal(ECP.Identity, result);
    }

    [Fact]
    public void ScalarMultiply_CyclicProperty_ReturnsExpectedPoints()
    {
        var curve = CreateTestCurve();
        var points = new List<ECP>();
    
        // Collect all points in the cyclic subgroup, up to order-1
        for (var i = 1; i < curve.GetOrder(); i++)
        {
            var point = EllipticCurve.ScalarMultiply(curve, ValidPoint, i);
            Assert.True(curve.IsOnCurve(point)); // Verify each point is valid
            points.Add(point);
        }
    
        // Verify next point cycles back (kP where k = order should give Identity)
        var cyclePoint = EllipticCurve.ScalarMultiply(curve, ValidPoint, curve.GetOrder());
        Assert.Equal(ECP.Identity, cyclePoint);
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
    
    [Fact]
    public void ScalarMultiply_DistributiveProperty()
    {
        var curve = CreateTestCurve();
        // Use smaller numbers to avoid going over the order
        var k1 = 2;
        var k2 = 3;
        var point = ValidPoint;
    
        // (k1 + k2)P should equal k1P + k2P
        var left = EllipticCurve.ScalarMultiply(curve, point, k1 + k2);
        var right = EllipticCurve.Add(curve,
            EllipticCurve.ScalarMultiply(curve, point, k1),
            EllipticCurve.ScalarMultiply(curve, point, k2));
    
        Assert.True(curve.IsOnCurve(left));
        Assert.True(curve.IsOnCurve(right));
        Assert.Equal(left, right);
    }


    [Fact]
    public void ScalarMultiply_NegativeScalarEqualsNegativePoint()
    {
        var curve = CreateTestCurve();
        var k = 5;
        var point = ValidPoint;
    
        // (-k)P should equal -(kP)
        var negativeScalar = EllipticCurve.ScalarMultiply(curve, point, -k);
        var negativePoint = EllipticCurve.Negate(curve, EllipticCurve.ScalarMultiply(curve, point, k));
        Assert.Equal(negativeScalar, negativePoint);
    }
    
    [Fact]
    public void IsOnCurve_IdentityPoint_ReturnsTrue()
    {
        var curve = CreateTestCurve();
        Assert.True(curve.IsOnCurve(ECP.Identity));
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ScalarMultiply_WithLargeScalars_ResultStillOnCurve(int k)
    {
        var curve = CreateTestCurve();
        var result = EllipticCurve.ScalarMultiply(curve, ValidPoint, k);
        Assert.True(curve.IsOnCurve(result));
    }
}
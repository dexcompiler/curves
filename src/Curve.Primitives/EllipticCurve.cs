using System;
using System.Numerics;

namespace Curve.Primitives;

using ECPoint = Point<EllipticCurve>;

/*
        the elliptic curve equation is defined by:
            y^2 == (x^3 + Ax + B) mod P
        where A and B are curve parameters and
              P is a prime number indicating the field over which the curve is defined.
        All operations are performed modulo P, which keeps the points on the curve within a finite field.
        This is crucial for cryptographic applications.        
*/

public interface IEllipticCurve<TCurve>
    where TCurve : struct, IEllipticCurve<TCurve>
{
    /// <summary>
    /// Checks if the given <see cref="ECPoint"/> lies on the curve.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    bool IsOnCurve(Point<TCurve> point);

    /// <summary>
    /// Adds two points on the curve.
    /// </summary> 
    /// <param name="curve">The concrete type of the elliptic curve implementing this trait</param>
    /// <param name="p1">The first <see cref="ECPoint"/> object</param>
    /// <param name="p2">The second <see cref="ECPoint"/> object</param>
    /// <returns>A <see cref="ECPoint"/> instance</returns>
    static abstract Point<TCurve> Add(TCurve curve, in Point<TCurve> p1, in Point<TCurve> p2);

    /// <summary>
    /// Retrieves the order of the elliptic curve, representing the total number of points on the curve.
    /// </summary>
    /// <returns>The order of the elliptic curve as a <see cref="BigInteger"/>.</returns>
    BigInteger GetOrder();

    /// <summary>
    /// Retrieves the base point of the elliptic curve, which serves as the starting point for scalar multiplication and other operations.
    /// </summary>
    /// <returns>A <see cref="ECPoint"/> representing the base point of the curve.</returns>
    Point<TCurve> GetBasePoint();

    /// <summary>
    /// Computes the negation of a given <see cref="ECPoint"/> on the elliptic curve.
    /// </summary>
    /// <param name="curve">The elliptic curve on which the operation is performed.</param>
    /// <param name="point">The <see cref="ECPoint"/> to be negated.</param>
    /// <returns>A new <see cref="ECPoint"/> representing the negation of the input point on the curve.</returns>
    static abstract Point<TCurve> Negate(TCurve curve, Point<TCurve> point);

    /// <summary>
    /// Performs scalar multiplication of a point on the elliptic curve.
    /// </summary>
    /// <param name="curve">The elliptic curve on which the operation is performed.</param>
    /// <param name="p">The <see cref="ECPoint"/> to be multiplied.</param>
    /// <param name="k">The scalar value as a <see cref="BigInteger"/>.</param>
    /// <returns>A new <see cref="ECPoint"/> representing the result of the scalar multiplication.</returns>
    static abstract Point<TCurve> ScalarMultiply(TCurve curve, Point<TCurve> p, BigInteger k);
}

/// <summary>
/// Represents an elliptic curve defined by the equation y^2 = x^3 + Ax + B over a finite field of prime order P.
/// </summary>
/// <br/>
/// <br/>
/// The elliptic curve is defined by the equation:
/// <br/>
/// y^2 ≡ x^3 + Ax + B (mod P)
/// <br/>
/// where:
/// <br/>
/// - A and B are curve parameters.
/// <br/>
/// - P is a prime number that defines the finite field over which the curve is defined.
/// <br/>
/// <br/>
/// Properties of elliptic curves:
/// <br/>
/// - The curve is non-linear and continuous.
/// <br/>
/// - The curve is closed under addition (closure); the addition of any 2 points on the curve will yield a third point on the curve
/// <br/>
/// - The curve is associative under addition (associativity); i.e., (P + Q) + R = P + (Q + R) for any points P, Q, and R on the curve.
/// <br/>
/// - Multiplication of a point by a scalar is supported, providing the basis for elliptic curve cryptographic operations. 
/// <br/>
/// - The curve has an identity element (0, 0) that acts as the additive identity.
/// <br/>
/// <br/>
/// This structure implements the necessary operations for elliptic curve arithmetic, including point addition,
/// scalar multiplication, and point negation, while verifying that points lie on the curve.
public readonly record struct EllipticCurve : IEllipticCurve<EllipticCurve>
{
    private readonly BigInteger _a;
    private readonly BigInteger _b;
    private readonly BigInteger _p;
    private readonly ECPoint _basePoint;
    private readonly BigInteger _order;
    
   public EllipticCurve(
        BigInteger a, 
        BigInteger b, 
        BigInteger prime,
        ECPoint basePoint,
        BigInteger order)
    {
        _a = a;
        _b = b;
        _p = prime;
        _basePoint = basePoint;
        _order = order;
    }
    public BigInteger GetOrder() => _order;
    public ECPoint GetBasePoint() => _basePoint;
    public static ECPoint Negate(EllipticCurve curve, ECPoint point) => point with {Y = (-point.Y + curve._p) % curve._p};

    public bool IsOnCurve(ECPoint point)
    {
        if (point == ECPoint.Identity)
            return true;

        var lhs = (point.Y * point.Y) % _p;
        var rhs = (BigInteger.Pow(point.X, 3) + _a * point.X + _b) % _p;
        return lhs == rhs;
    }

    public static ECPoint ScalarMultiply(
        EllipticCurve curve,
        ECPoint p,
        BigInteger k)
    {
        k %= curve.GetOrder();
        if (k < 0)
        {
            k = -k;
            p = Negate(curve, p);
        }

        var temp = p;
        var result = ECPoint.Identity;

        while (k > 0)
        {
            if ((k & 1) == 1)
                result = Add(curve, result, temp);
            temp = Add(curve, temp, temp);
            k >>= 1;
        }

        return result;
    }

    public static ECPoint Add(EllipticCurve curve, in ECPoint p1, in ECPoint p2)
    {
        if (p1 == ECPoint.Identity) return p2;
        if (p2 == ECPoint.Identity) return p1;

        if (p1.X == p2.X && p1.Y != p2.Y) return ECPoint.Identity;

        BigInteger slope;
        var p = curve._p;
        if (p1 == p2)
        {
            if (p1.Y == 0) return ECPoint.Identity;
            slope = (3 * p1.X * p1.X + curve._a) * GetModularInverse(2 * p1.Y, p) % p;
        }
        else
        {
            slope = (p2.Y - p1.Y) * GetModularInverse(p2.X - p1.X, p) % p;
        }

        var x3 = (slope * slope - p1.X - p2.X) % p;
        var y3 = (slope * (p1.X - x3) - p1.Y) % p;

        return new((x3 + p) % p, (y3 + p) % p);
    }
    
    // TODO: Implement Point<EllipticCurve> Compression and Decompression
    // for Compression branch, consider Jacobian coordinates
    // for Decompression branch, consider the Tonelli-Shanks algorithm

    private static BigInteger GetModularInverse(BigInteger a, BigInteger m)
    {
        var (g, x, _) = ComputeGcd(a, m);
        if (g != 1) throw new ArithmeticException($"Modular inverse of {a} does not exist for the given modulus {m}.");
        return (x % m + m) % m;
    }

    private static (BigInteger g, BigInteger x, BigInteger y) ComputeGcd(BigInteger a, BigInteger b)
    {
        BigInteger x0 = BigInteger.One, x1 = BigInteger.Zero;
        BigInteger y0 = BigInteger.Zero, y1 = BigInteger.One;

        while (b != 0)
        {
            var quotient = a / b;
            (a, b) = (b, a % b);
            (x0, x1) = (x1, x0 - quotient * x1);
            (y0, y1) = (y1, y0 - quotient * y1);
        }

        return (a, x0, y0);
    }
}

/// <summary>
/// Represents a point on an elliptic curve defined over a finite field.
/// </summary>
/// <br/>
/// <br/>
/// The elliptic curve point consists of an X-coordinate and a Y-coordinate, both represented as integers within a finite field.
/// Points can be added together or multiplied by scalars according to the mathematical rules governing elliptic curves.
/// <br/>
/// <br/>
/// Properties:
/// <br/>
/// - The point at infinity is represented as the identity element and is used in elliptic curve operations.
/// <br/>
/// - Valid points satisfy the elliptic curve equation: y^2 ≡ x^3 + Ax + B (mod P), depending on the curve's parameters A, B, and P.
/// <br/>
/// - Points have a symmetry property such that for a point (X, Y) on the curve, the point (X, -Y) is also on the curve (mod P).
/// <br/>
/// <br/>
/// The Point structure is a fundamental component in elliptic curve cryptography, facilitating public-key operations such as
/// key generation, encryption, and digital signature schemes.
public readonly record struct Point<TCurve>(BigInteger X, BigInteger Y)
    where TCurve : struct, IEllipticCurve<TCurve>
{
    public static Point<TCurve> Identity { get; } = new(BigInteger.Zero, BigInteger.Zero);
}